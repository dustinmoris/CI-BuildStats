open System
open System.Configuration
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
// Config
// -------------------------------------------

let fallback backup value = if value = null then backup else value

let getConfigValue key =
    Environment.GetEnvironmentVariable key
    |> fallback ConfigurationManager.AppSettings.[key]
    |> fallback ""

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

let md5 (text : string) =
    text
    |> System.Text.Encoding.UTF8.GetBytes
    |> System.Security.Cryptography.MD5.Create().ComputeHash
    |> Array.map (fun x -> x.ToString("x2"))
    |> String.concat ""

let calculateETag viewModel =
    Common.Serializer.toJson viewModel
    |> md5

let setETag viewModel =
    let eTag = viewModel |> calculateETag
    Writers.setHeader "ETag" eTag

let SVG template viewModel =
    page template viewModel
    >=> Writers.setMimeType "image/svg+xml"
    >=> Writers.setHeader "Cache-Control" "no-cache"
    >=> setETag viewModel

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

let sentryDsn   = getConfigValue "SENTRY_DSN"
let ravenClient = new SharpRaven.RavenClient(sentryDsn)

let logInSentry (ex : Exception) (ctx : HttpContext) =
    ex.Data.Add("request-url", ctx.request.url.AbsoluteUri)
    ravenClient.Capture(new SharpRaven.Data.SentryEvent(ex))

let svgErrorHandler (ex : Exception) (msg : string) (ctx : HttpContext) =    
    Log.log ctx.runtime.logger "App" Suave.Logging.LogLevel.Error ex.Message
    logInSentry ex |> ignore
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

[<EntryPoint>]
let main argv = 
    startWebServer config app
    0