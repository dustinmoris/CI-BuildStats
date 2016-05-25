open System
open System.Net
open Suave
open Suave.Files
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.RequestErrors
open Suave.DotLiquid
open Suave.Logging.Loggers
open PackageServices
open BuildHistoryCharts
open QueryParameterParser
open Models
open ViewModels

// -------------------------------------------
// Common methods
// -------------------------------------------

type Result<'T> =
    | Success of 'T
    | Failure of string

let bind switchFunc result =
    match result with
    | Success s -> switchFunc s
    | Failure f -> Failure f

let SVG template viewModel =
    page template viewModel
    >=> Writers.setMimeType "image/svg+xml"

// -------------------------------------------
// Package Endpoints
// -------------------------------------------

let getIncludePreReleases (ctx : HttpContext) =
    match getBool "includePreReleases" ctx with
    | NotSet            -> Success true
    | Value value       -> Success value
    | ParsingError msg  -> Failure msg

let getPackage getPackageFunc slug =
    fun (ctx : HttpContext) ->
        match getIncludePreReleases ctx with
        | Failure msg             -> BAD_REQUEST msg ctx
        | Success inclPreReleases ->
            async { 
                let! package = getPackageFunc slug inclPreReleases
                return!
                    match package with
                    | None      -> NOT_FOUND <| sprintf "Package could not be found." <| ctx
                    | Some pkg  -> SVG "Package.liquid" <| createPackageViewModel pkg <| ctx
            }

// -------------------------------------------
// Build History Endpoints
// -------------------------------------------

let queryResult<'T> queryParamFunc
                    (setValueFunc : BuildHistoryModel -> 'T -> BuildHistoryModel)
                    (ctx, model) =
    match queryParamFunc ctx with
    | NotSet            -> Success (ctx, model)
    | Value value       -> Success (ctx, setValueFunc model value)
    | ParsingError msg  -> Failure msg

let getBuildCount       = queryResult (getInt32 "buildCount")                  BuildHistoryModel.SetBuildCount
let getInclPullRequests = queryResult (getBool "includeBuildsFromPullRequest") BuildHistoryModel.SetIncludePullRequests
let getShowStats        = queryResult (getBool "showStats")                    BuildHistoryModel.SetShowStats

let getBranch (ctx   : HttpContext,
               model : BuildHistoryModel) =
    match getString "branch" ctx with
    | None          -> Success model
    | Some value    -> Success <| BuildHistoryModel.SetBranch model (Some value)

let getBuildHistory (getBuildsFunc) (account, project) =
    fun (ctx : HttpContext) ->
        let result =
            (ctx, BuildHistoryModel.Default)
            |> (getBuildCount
            >> bind getInclPullRequests
            >> bind getShowStats
            >> bind getBranch)
        match result with
        | Failure msg   -> BAD_REQUEST msg ctx
        | Success model ->
            async {
                let! builds = getBuildsFunc account project model.BuildCount model.Branch model.IncludePullRequests
                let  viewModel  = createBuildHistoryViewModel builds model.ShowStats
                return! SVG "BuildHistory.liquid" viewModel ctx
            }

// -------------------------------------------
// Error Handler
// -------------------------------------------

type Error =
    {
        Type       : string
        Message    : string
        StackTrace : string
    }

let svgErrorHandler (ex : Exception) (msg : string) (ctx : HttpContext) =
    let viewModel = { Type = ex.GetType().ToString(); Message = ex.Message; StackTrace = ex.StackTrace }
    SVG "Error.liquid" viewModel ctx

// -------------------------------------------
// Web Application
// -------------------------------------------
    
let app = 
    choose [
        GET >=> choose [
            path     "/"                     >=> browseFileHome "index.html"
            path     "/tests"                >=> browseFileHome "tests.html"
            path     "/ping"                 >=> OK "pong"
            pathScan "/nuget/%s"             <| getPackage NuGet.getPackageAsync
            pathScan "/myget/%s/%s"          <| getPackage MyGet.getPackageAsync
            pathScan "/appveyor/chart/%s/%s" <| getBuildHistory AppVeyor.getBuilds
            pathScan "/travisci/chart/%s/%s" <| getBuildHistory TravisCI.getBuilds
            pathScan "/circleci/chart/%s/%s" <| getBuildHistory CircleCI.getBuilds
        ]
        NOT_FOUND "The requested resource could not be found. Please note that URLs are case sensitive."
    ]


let config =
    { defaultConfig with
        bindings = [ HttpBinding.mk HTTP (IPAddress.Parse "0.0.0.0") 8083us ]
        errorHandler = svgErrorHandler }
        // ToDo: Provide different logger to log to Elasticsearch or similar

[<EntryPoint>]
let main argv = 
    startWebServer config app
    0