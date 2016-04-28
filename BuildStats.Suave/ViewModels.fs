module ViewModels

open System
open System.Drawing
open PackageServices

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
        FeedBgColour        : string
        FeedWidth           : int        
        VersionBgColour     : string
        VersionWidth        : int        
        DownloadsBgColour   : string
        DownloadsWidth      : int
    }

let createPackageModel (package : Package)
                       (feed    : string) =

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