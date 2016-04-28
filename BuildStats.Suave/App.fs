open System
open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.RequestErrors
open Suave.DotLiquid
open PackageServices
open Serializers
open ViewModels

let tryParseWith tryParseFunc = tryParseFunc >> function
    | true  , value -> Some value
    | false , _     -> None

let tryParseBool = tryParseWith bool.TryParse

let (|Bool|_|) = tryParseBool

type QueryParamValue<'T> =
    | Value of 'T
    | ParsingError

let getBoolFromQueryParam (ctx : HttpContext) (key : string) (defaultValue : bool) =
    match ctx.request.queryParam key with
    | Choice1Of2 value  ->
        match value with
        | Bool b    -> Value b
        | _         -> ParsingError
    | _             -> Value defaultValue

let SVG template model =
    page template model
    >=> Writers.setMimeType "image/svg+xml"

let preReleasesQueryParamKey = "includePreReleases"

let preReleaseQueryParamParseErrorMsg = 
    sprintf "Could not parse query parameter \"%s\" into a Boolean value." preReleasesQueryParamKey

let getNuGetPackage packageName =
    fun (ctx : HttpContext) ->       
        match getBoolFromQueryParam ctx preReleasesQueryParamKey false with
        | Value includePreReleases ->
            async { 
                let! package = getNuGetPackageAsync packageName includePreReleases
                return!
                    match package with
                    | None      -> NOT_FOUND (sprintf "NuGet package \"%s\" could not be found." packageName) ctx
                    | Some pkg  -> SVG "Package.svg" (createPackageModel pkg "nuget") ctx
            }
        | ParsingError          -> BAD_REQUEST preReleaseQueryParamParseErrorMsg ctx


let getMyGetPackage (feedName, packageName) =
    fun (ctx : HttpContext) ->
        match getBoolFromQueryParam ctx preReleasesQueryParamKey false with
        | Value includePreReleases ->
            async { 
                let! package = getMyGetPackageAsync feedName packageName includePreReleases
                return!
                    match package with
                    | None      -> NOT_FOUND (sprintf "MyGet package \"%s\" could not be found." packageName) ctx
                    | Some pkg  -> SVG "Package.svg" (createPackageModel pkg "myget") ctx
            }
        | ParsingError          -> BAD_REQUEST preReleaseQueryParamParseErrorMsg ctx


let app = 
    GET >=> choose [
        pathScan "/nuget/%s"    getNuGetPackage
        pathScan "/myget/%s/%s" getMyGetPackage
    ]


[<EntryPoint>]
let main argv = 
    startWebServer defaultConfig app
    0