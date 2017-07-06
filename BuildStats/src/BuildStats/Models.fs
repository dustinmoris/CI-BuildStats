module BuildStats.Models

open System
open BuildStats.PackageServices
open BuildStats.BuildHistoryCharts
open BuildStats.TextSize

type PackageModel =
    {
        Width          : int
        FeedWidth      : int
        VersionWidth   : int
        DownloadsWidth : int
        Padding        : int
        FeedName       : string
        Version        : string
        Downloads      : string
    }
    static member FromPackage (package : Package) =
        let padding = 7
        let downloads =
            let million  = 1000000
            let thousand = 1000
            match package.Downloads with
            | dl when dl >= million  -> float dl / float million    |> sprintf "▾ %.2fm"
            | dl when dl >= thousand -> float dl / float thousand   |> sprintf "▾ %.1fk"
            | dl                     -> dl                          |> sprintf "▾ %i"
        let version        = sprintf "v%s" package.Version
        let feedWidth      = measureTextWidth package.Feed + padding * 2
        let versionWidth   = measureTextWidth version + padding * 2
        let downloadsWidth = measureTextWidth downloads + padding * 2
        {
            Width          = feedWidth + versionWidth + downloadsWidth
            FeedWidth      = feedWidth
            VersionWidth   = versionWidth
            DownloadsWidth = downloadsWidth
            Padding        = padding
            FeedName       = package.Feed
            Version        = version
            Downloads      = downloads
        }

type BuildBar =
    {
        X      : int
        Y      : int
        Height : int
        Colour : string
    }

type BuildHistoryModel =
    {
        Width     : int
        Height    : int
        Branch    : string
        ShowStats : bool
        MaxBuild  : string
        MinBuild  : string
        AvgBuild  : string
        BuildBars : BuildBar list
    }
    static member FromBuilds (showStats : bool) (builds : Build list) =
        let branches = builds |> List.distinctBy (fun x -> x.Branch)

        let branchText =
            match branches.Length with
            | 0 -> "No builds found"
            | 1 -> sprintf "Build history for %s (last %i builds)" branches.[0].Branch builds.Length
            | _ -> sprintf "Build history for all branches (last %i builds)" builds.Length

        let longestBuildTime = BuildMetrics.longestBuildTime builds
        let formatTimeSpan (timeSpan : TimeSpan) = timeSpan.ToString("hh\:mm\:ss")
        let maxBuild = "Max build time: " + (longestBuildTime |> formatTimeSpan)
        let minBuild = "Min build time: " + (BuildMetrics.shortestBuildTime builds |> formatTimeSpan)
        let avgBuild = "Avg build time: " + (BuildMetrics.averageBuildTime builds |> formatTimeSpan)

        let linesOfText  = if showStats then 4 else 1
        let maxBarHeight = 50
        let barWidth     = 5
        let gap          = 3
        let totalHeight  = maxBarHeight + 12 * (linesOfText + 1) + gap * linesOfText
        let branchWidth  = measureTextWidth branchText
        let chartsWidth  = builds.Length * (barWidth + gap) - gap
        let totalWidth   = max 180 (max branchWidth chartsWidth)

        let buildBars =
            builds
            |> List.rev
            |> List.mapi (fun i b ->
                let percent =
                    match longestBuildTime.TotalMilliseconds with
                    | 0.0 -> 0.0
                    | _ ->
                        b.TimeTaken.TotalMilliseconds /
                        longestBuildTime.TotalMilliseconds

                let height = int(Math.Floor(Math.Max(percent * float maxBarHeight, 3.0)))

                {
                    X      = i * (barWidth + gap)
                    Y      = totalHeight - height
                    Height = height
                    Colour =
                        match b.Status with
                        | Success   -> "#04b431"
                        | Failed    -> "#ff0000"
                        | Pending   -> "#ffbf00"
                        | Cancelled -> "#888888"
                        | Unkown    -> "#ffffff"
                })
        {
            Width     = totalWidth
            Height    = totalHeight
            Branch    = branchText
            ShowStats = showStats
            MaxBuild  = maxBuild
            MinBuild  = minBuild
            AvgBuild  = avgBuild
            BuildBars = buildBars
        }