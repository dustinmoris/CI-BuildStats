open System
open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.RequestErrors
open Suave.DotLiquid
open PackageServices
open BuildHistoryCharts
open Serializers
open QueryParameterHelper
open ViewModels

let SVG template model =
    page template model
    >=> Writers.setMimeType "image/svg+xml"

let getPackage (getPackageFunc : bool -> Async<Package option>) =
    fun (ctx : HttpContext) ->
        match getBoolFromUrlQuery ctx "includePreReleases" false with
        | Value includePreReleases ->
            async { 
                let! package = getPackageFunc includePreReleases
                return!
                    match package with
                    | None      -> NOT_FOUND <| sprintf "Package could not be found." <| ctx
                    | Some pkg  -> SVG "Package.liquid" <| createPackageModel pkg <| ctx
            }
        | ParsingError          -> BAD_REQUEST "Could not parse query parameter \"includePreReleases\" into a Boolean value." ctx

let nugetFunc packageName = 
    fun includePreReleases -> 
        async { 
            return! NuGet.getPackageAsync packageName includePreReleases 
        }

let mygetFunc (feedName, packageName) =
    fun includePreReleases -> 
        async { 
            return! MyGet.getPackageAsync feedName packageName includePreReleases 
        }

let test (account, project) =
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
                        let! builds = AppVeyor.getBuilds account project buildCount branch inclPullRequests
                        let model = createBuildHistoryModel builds showStats
                        return!         SVG "BuildHistory.liquid" model ctx
                    }
                | ParsingError       -> BAD_REQUEST "Could not parse query parameter \"showStats\" into a Boolean value." ctx
            | ParsingError           -> BAD_REQUEST "Could not parse query parameter \"includeBuildsFromPullRequest\" into a Boolean value." ctx
        | ParsingError               -> BAD_REQUEST "Could not parse query parameter \"buildCount\" into a Int32 value." ctx
        
let app = 
    choose [
        GET >=> choose [
            pathScan "/nuget/%s"    (fun x -> nugetFunc x |> getPackage)
            pathScan "/myget/%s/%s" (fun x -> mygetFunc x |> getPackage)
            pathScan "/appveyor/chart/%s/%s" test
        ]
        NOT_FOUND "The requested resource could not be found. Please note that URLs are case sensitive."
    ]

[<EntryPoint>]
let main argv = 
    startWebServer defaultConfig app
    0