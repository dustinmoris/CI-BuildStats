module ViewModels

open System
open System.Drawing
open PackageServices
open BuildHistoryCharts

// -------------------------------------------
// Common ViewModel functions
// -------------------------------------------

let isRunningOnMono = Type.GetType("Mono.Runtime") = null |> not
let strFormat       = new StringFormat(StringFormat.GenericTypographic)
let bitmap          = new Bitmap(1, 1)
let graphics        = Graphics.FromImage(bitmap)
let rect            = new RectangleF(0.0f, 0.0f, 1000.0f, 1000.0f)

graphics.TextRenderingHint <- Text.TextRenderingHint.AntiAlias

let measureTextWidth (fontSize  : int)
                     (fontStyle : FontStyle)
                     (text      : string) =
    [| new System.Drawing.CharacterRange(0, text.Length) |]
    |> strFormat.SetMeasurableCharacterRanges   
    let fSize       = if isRunningOnMono then float32 fontSize - 1.5f else float32 fontSize
    let font        = new Font("Arial", fSize, fontStyle, GraphicsUnit.Pixel)
    let regions     = graphics.MeasureCharacterRanges(text, font, rect, strFormat)
    let dimensions  = regions.[0].GetBounds(graphics)
    dimensions.Right + 1.0f |> int
    
// -------------------------------------------
// Build History Chart ViewModel
// -------------------------------------------

type TextModel =
    {
        X    : int
        Y    : int
        Text : string
    }

type BuildBarModel =
    {
        X       : int
        Y       : int
        Height  : int
        BuildId : int
        Colour  : string
    }

type BuildHistoryViewModel =
    {      
        TotalWidth          : int
        TotalHeight         : int
        FontSize            : int
        FontFamily          : string
        FontColour          : string
        BranchTextColour    : string
        BranchText          : TextModel
        LongestBuildText    : TextModel
        ShortestBuildText   : TextModel
        AverageBuildText    : TextModel
        ShowStats           : bool
        BarWidth            : int
        Builds              : BuildBarModel list
    }

let createBuildHistoryViewModel (builds     : Build list)
                                (showStats  : bool) =
    let fontSize = 12
    let barWidth = 5
    let gap = 3
    let maxBarHeight = 50    

    let linesOfText =
        match showStats with
        | true  -> 4
        | false -> 1

    let branches = builds |> List.distinctBy (fun x -> x.Branch)

    let branchText =
        match branches.Length with
        | 0 -> "No builds found"
        | 1 -> sprintf "Build history for %s (last %i builds)" branches.[0].Branch builds.Length
        | _ -> sprintf "Build history for all branches (last %i builds)" builds.Length

    let longestBuildTime = BuildMetrics.longestBuildTime builds
    
    let formatTimeSpan (timeSpan : TimeSpan) = timeSpan.ToString("hh\:mm\:ss\.ff")

    let longestBuildText =
        "Longest build time: " + (longestBuildTime |> formatTimeSpan)

    let shortestBuildText =
        "Shortest build time: " + (BuildMetrics.shortestBuildTime builds |> formatTimeSpan)

    let averageBuildText =
        "Average build time: " + (BuildMetrics.averageBuildTime builds |> formatTimeSpan)    

    let totalHeight = maxBarHeight + fontSize * (linesOfText + 1) + gap * linesOfText    
    let branchTextWidth = measureTextWidth fontSize FontStyle.Bold branchText
    let chartsWidth = builds.Length * (barWidth + gap) - gap
    let totalWidth = max 180 (max branchTextWidth chartsWidth)

    {        
        TotalWidth       = totalWidth + 1
        TotalHeight      = totalHeight + 1
        FontSize         = fontSize
        FontFamily       = "Helvetica,Arial,sans-serif"
        FontColour       = "#777777"
        BranchTextColour = "#000000"
        BranchText =
            {
                X    = 0
                Y    = fontSize
                Text = branchText
            }
        LongestBuildText =
            {
                X    = 0
                Y    = fontSize * 2 + gap
                Text = longestBuildText
            }
        ShortestBuildText =
            {
                X    = 0
                Y    = fontSize * 3 + gap * 2
                Text = shortestBuildText
            }
        AverageBuildText =
            {
                X    = 0
                Y    = fontSize * 4 + gap * 3
                Text = averageBuildText
            }
        ShowStats = showStats
        BarWidth  = barWidth
        Builds =
            builds
            |> List.rev
            |> List.mapi (
                fun index build ->
                    let percent = 
                        match longestBuildTime.TotalMilliseconds with
                        | 0.0 -> 0.0
                        | _ ->
                            build.TimeTaken.TotalMilliseconds /
                            longestBuildTime.TotalMilliseconds

                    let height = int(Math.Floor(Math.Max(percent * float maxBarHeight, 3.0)))

                    {
                        X       = index * (barWidth + gap)
                        Y       = totalHeight - height
                        Height  = height
                        Colour  = 
                            match build.Status with
                            | Success   -> "#04b431"
                            | Failed    -> "#ff0000"
                            | Pending   -> "#ffbf00"
                            | Cancelled -> "#888888"   
                            | Unkown    -> "#ffffff"                
                        BuildId = build.Id
                    })
    }

// -------------------------------------------
// Package ViewModel
// -------------------------------------------

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

let createPackageViewModel (package : Package)  =

    let fontSize = 12
    let padding =
        match isRunningOnMono with
        | true  -> 7
        | false -> 7
    let version = sprintf "v%s" package.Version
    let sign = "▾ "
    let downloads =
        let million  = 1000000
        let thousand = 1000
        match package.Downloads with
        | dl when dl >= million  -> float dl / float million    |> sprintf "%.2fm"
        | dl when dl >= thousand -> float dl / float thousand   |> sprintf "%.1fk"
        | dl                     -> dl                          |> sprintf "%i"
    
    let addPadding   width  = width + padding * 2
    let addSignWidth width  = width + 15

    let feedWidth       = package.Feed  |> measureTextWidth fontSize FontStyle.Regular |> addPadding
    let versionWidth    = version       |> measureTextWidth fontSize FontStyle.Regular |> addPadding
    let downloadsWidth  = downloads     |> measureTextWidth fontSize FontStyle.Regular |> addPadding |> addSignWidth

    {
        Feed                = package.Feed
        Version             = version
        Downloads           = sign + downloads
        X                   = 0
        Y                   = 0
        Padding             = padding
        TotalWidth          = feedWidth + versionWidth + downloadsWidth
        TotalHeight         = 20
        FontFamily          = "Helvetica,Arial,sans-serif"
        FontSize            = fontSize
        FontColour          = "#000000"
        CornerRadius        = 2
        FeedBgColour        = "#333333"
        FeedWidth           = feedWidth
        VersionBgColour     = "#00b359"
        VersionWidth        = versionWidth
        DownloadsBgColour   = "#483C32"
        DownloadsWidth      = downloadsWidth
    }