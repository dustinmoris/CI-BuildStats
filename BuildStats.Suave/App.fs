open System
open Suave
open Suave.Files
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.RequestErrors
open Suave.DotLiquid
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

let getBuildCount           = queryResult (getInt32 "buildCount")                  BuildHistoryModel.SetBuildCount
let getInclFromPullRequests = queryResult (getBool "includeBuildsFromPullRequest") BuildHistoryModel.SetIncludeFromPullRequests
let getShowStats            = queryResult (getBool "showStats")                    BuildHistoryModel.SetShowStats

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
            >> bind getInclFromPullRequests
            >> bind getShowStats
            >> bind getBranch)
        match result with
        | Failure msg   -> BAD_REQUEST msg ctx
        | Success model ->
            async {
                let! builds = getBuildsFunc account project model.BuildCount model.Branch model.IncludeFromPullRequests
                let  viewModel  = createBuildHistoryViewModel builds model.ShowStats
                return! SVG "BuildHistory.liquid" viewModel ctx
            }

// -------------------------------------------
// Web Application
// -------------------------------------------
    
let app = 
    choose [
        GET >=> choose [
            path     "/"                     >=> file "index.html"
            path     "/tests"                >=> file "tests.html"
            pathScan "/nuget/%s"             <| getPackage NuGet.getPackageAsync
            pathScan "/myget/%s/%s"          <| getPackage MyGet.getPackageAsync
            pathScan "/appveyor/chart/%s/%s" <| getBuildHistory AppVeyor.getBuilds
            pathScan "/travisci/chart/%s/%s" <| getBuildHistory TravisCI.getBuilds
        ]
        NOT_FOUND "The requested resource could not be found. Please note that URLs are case sensitive."
    ]

[<EntryPoint>]
let main argv = 
    startWebServer defaultConfig app
    0