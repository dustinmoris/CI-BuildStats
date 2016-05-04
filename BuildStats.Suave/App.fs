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
open ViewModels

module UrlQuery =

    let tryParseWith tryParseFunc = tryParseFunc >> function
        | true  , value -> Some value
        | false , _     -> None

    let tryParseBool = tryParseWith bool.TryParse

    let (|Bool|_|) = tryParseBool

    type QueryParamValue<'T> =
        | Value of 'T
        | ParsingError

    let getBool (ctx : HttpContext) 
                (key : string) 
                (defaultValue : bool) =

        match ctx.request.queryParam key with
        | Choice1Of2 value  ->
            match value with
            | Bool b    -> Value b
            | _         -> ParsingError
        | _             -> Value defaultValue

open UrlQuery

let SVG template model =
    page template model
    >=> Writers.setMimeType "image/svg+xml"

let getPackage (getPackageFunc : bool -> Async<Package option>) =
    fun (ctx : HttpContext) ->
        match getBool ctx "includePreReleases" false with
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
        async {
            let! builds = AppVeyor.getBuilds account project 25 None true
            let model = createBuildHistoryModel builds true
            return! SVG "BuildHistory.svg" model ctx
        }
    

let app = 
    choose [
        GET >=> choose [
            pathScan "/nuget/%s"    (fun x -> nugetFunc x |> getPackage)
            pathScan "/myget/%s/%s" (fun x -> mygetFunc x |> getPackage)
            pathScan "/appveyor/%s/%s" test
        ]
        NOT_FOUND "The requested resource could not be found. Please note that URLs are case sensitive."
    ]

[<EntryPoint>]
let main argv = 
    startWebServer defaultConfig app
    0