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
open QueryParameterHelper
open ViewModels

// -------------------------------------------
// Web Framework methods
// -------------------------------------------

let SVG template model =
    page template model
    >=> Writers.setMimeType "image/svg+xml"

// -------------------------------------------
// Package Endpoints
// -------------------------------------------

let getPackage getPackageFunc slug =
    fun (ctx : HttpContext) ->
        match getBoolFromUrlQuery ctx "includePreReleases" false with
        | Value includePreReleases ->
            async { 
                let! package = getPackageFunc slug includePreReleases
                return!
                    match package with
                    | None      -> NOT_FOUND <| sprintf "Package could not be found." <| ctx
                    | Some pkg  -> SVG "Package.liquid" <| createPackageViewModel pkg <| ctx
            }
        | ParsingError          -> BAD_REQUEST "Could not parse query parameter \"includePreReleases\" into a Boolean value." ctx

// -------------------------------------------
// Build History Endpoints
// -------------------------------------------

//type Result<'TSuccess, 'TFailure> = 
//    | Success of 'TSuccess
//    | Failure of 'TFailure
//
//let bind switchFunction twoTrackInput = 
//    match twoTrackInput with
//    | Success s -> switchFunction s
//    | Failure f -> Failure f

let getBuildHistory (getBuildsFunc)
                    (account, project) =
    fun (ctx : HttpContext) ->
        match getInt32FromUrlQuery ctx "buildCount" 25 with
        | Value buildCount  ->
            match getBoolFromUrlQuery ctx "includeBuildsFromPullRequest" true with
            | Value inclPullRequests ->
                match getBoolFromUrlQuery ctx "showStats" true with
                | Value showStats    ->
                    let branch = 
                        match ctx.request.queryParam "branch" with
                        | Choice1Of2 value  -> Some value
                        | _                 -> None
                    async {
                        let! builds = getBuildsFunc account project buildCount branch inclPullRequests
                        let model = createBuildHistoryViewModel builds showStats
                        return!         SVG "BuildHistory.liquid" model ctx
                    }
                | ParsingError       -> BAD_REQUEST "Could not parse query parameter \"showStats\" into a Boolean value." ctx
            | ParsingError           -> BAD_REQUEST "Could not parse query parameter \"includeBuildsFromPullRequest\" into a Boolean value." ctx
        | ParsingError               -> BAD_REQUEST "Could not parse query parameter \"buildCount\" into a Int32 value." ctx

// -------------------------------------------
// Web Application
// -------------------------------------------
    
let app = 
    choose [
        GET >=> choose [
            path     "/"                     >=> file "index.html"
            path     "/tests"                >=> file "tests.html"
            pathScan "/nuget/%s"             (getPackage NuGet.getPackageAsync)
            pathScan "/myget/%s/%s"          (getPackage MyGet.getPackageAsync)
            pathScan "/appveyor/chart/%s/%s" (getBuildHistory AppVeyor.getBuilds)
            pathScan "/travisci/chart/%s/%s" (getBuildHistory TravisCI.getBuilds)
        ]
        NOT_FOUND "The requested resource could not be found. Please note that URLs are case sensitive."
    ]

[<EntryPoint>]
let main argv = 
    startWebServer defaultConfig app
    0