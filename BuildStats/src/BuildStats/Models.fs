module BuildStats.Models

open System
open BuildStats.PackageServices
open BuildStats.BuildHistoryCharts

type PackageModel =
    {
        Width          : int
        FeedWidth      : int
        VersionWidth   : int
        DownloadsWidth : int
        FeedName       : string
        Version        : string
        Downloads      : string
    }
    static member FromPackage (package : Package) =
        let downloads =
            let million  = 1000000
            let thousand = 1000
            match package.Downloads with
            | dl when dl >= million  -> float dl / float million    |> sprintf "%.2fm"
            | dl when dl >= thousand -> float dl / float thousand   |> sprintf "%.1fk"
            | dl                     -> dl                          |> sprintf "%i"
        {
            Width          = 250
            FeedWidth      = 50
            VersionWidth   = 100
            DownloadsWidth = 100
            FeedName       = package.Feed
            Version        = package.Version
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
        Builds    : BuildBar list
    }
    static member FromBuilds (builds : Build list) =
        let branches = builds |> List.distinctBy (fun x -> x.Branch)

        let branchText =
            match branches.Length with
            | 0 -> "No builds found"
            | 1 -> sprintf "Build history for %s (last %i builds)" branches.[0].Branch builds.Length
            | _ -> sprintf "Build history for all branches (last %i builds)" builds.Length

        let longestBuildTime = BuildMetrics.longestBuildTime builds    
        let formatTimeSpan (timeSpan : TimeSpan) = timeSpan.ToString("hh\:mm\:ss\.ff")
        let maxBuild = "Max build time: " + (longestBuildTime |> formatTimeSpan)
        let minBuild = "Min build time: " + (BuildMetrics.shortestBuildTime builds |> formatTimeSpan)
        let avgBuild = "Avg build time: " + (BuildMetrics.averageBuildTime builds |> formatTimeSpan)  

        {
            Width     = 200
            Height    = 200
            Branch    = branchText
            ShowStats = true
            MaxBuild  = maxBuild
            MinBuild  = minBuild
            AvgBuild  = avgBuild
            Builds    = []
        }