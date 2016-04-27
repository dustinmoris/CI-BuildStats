open System
open System.Drawing
open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.RequestErrors
open Suave.DotLiquid
open PackageServices
open Serializers

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

let JSON obj =
    serializeJson obj
    |> OK
    >=> Writers.setMimeType "application/json; charset=utf-8"

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
                    | Some pkg  -> JSON pkg ctx
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
                    | Some pkg  -> JSON pkg ctx
            }
        | ParsingError          -> BAD_REQUEST preReleaseQueryParamParseErrorMsg ctx


type PackageViewModel = 
    {
        Feed                : string
        Version             : string
        Downloads           : string

        X                   : int
        Y                   : int
        Padding             : int
        TotalWidth          : int
        TotalHeight         : int
        
        FontFamily          : string
        FontSize            : int
        FontColour          : string
        
        CornerRadius        : int
        
        FeedBgColour       : string
        FeedWidth          : int
        
        VersionBgColour     : string
        VersionWidth        : int
        
        DownloadsBgColour   : string
        DownloadsWidth      : int
    }

let createPackageViewModel (package : Package)
                           (feed : string) =

    let version = sprintf "v%s" package.Version

    let divideAndRound x y = 
        Math.Round(float x / float y, 2)

    let downloads =
        let million  = 1000000
        let thousand = 1000
        match package.Downloads with
        | dl when dl >= million  -> sprintf "▾ %fm" <| divideAndRound dl million
        | dl when dl >= thousand -> sprintf "▾ %fk" <| divideAndRound dl thousand
        | dl                     -> sprintf "▾ %i"  <| dl

    let fontSize = 12
    let padding = 5

    let measureTextWidth (text : string) =
        let bitmap = new Bitmap(1, 1)
        let graphics = Graphics.FromImage(bitmap)
        let font = new Font(FontFamily.GenericSansSerif, float32 (fontSize - 3))
        let dimension = graphics.MeasureString(text, font)
        int (Math.Ceiling(float dimension.Width))

    let addPadding width = width + padding * 2

    let feedWidth       = feed      |> measureTextWidth |> addPadding
    let versionWidth    = version   |> measureTextWidth |> addPadding
    let downloadsWidth  = downloads |> measureTextWidth |> addPadding

    {
        Feed = feed
        Version = version
        Downloads = downloads
        X = 0
        Y = 0
        Padding = padding
        TotalWidth = feedWidth + versionWidth + downloadsWidth
        TotalHeight = 20
        FontFamily = "Helvetica,Arial,sans-serif"
        FontSize = fontSize
        FontColour = "#000000"
        CornerRadius = 2
        FeedBgColour = "#333333"
        FeedWidth = feedWidth
        VersionBgColour = "#00b359"
        VersionWidth = versionWidth
        DownloadsBgColour = "#483C32"
        DownloadsWidth = downloadsWidth
    }

let app = 
    GET >=> choose [
        pathScan "/nuget/%s"    getNuGetPackage
        pathScan "/myget/%s/%s" getMyGetPackage
        path     "/test"        >=> page "PackageBadge.svg" { Name = "Dustin"; Version = "1.1.1"; Downloads = 100 } >=> Writers.setMimeType "image/svg+xml"
    ]


[<EntryPoint>]
let main argv = 
    startWebServer defaultConfig app
    0