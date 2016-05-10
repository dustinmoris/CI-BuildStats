module BuildHistoryCharts

open System
open RestClient
open Newtonsoft.Json.Linq
open Serializers

type BuildStatus =
    | Success
    | Failed
    | Cancelled
    | Pending
    | Unkown

type Build =
    {
        Id              : int
        BuildNumber     : int
        TimeTaken       : TimeSpan
        Status          : BuildStatus
        Branch          : string
        FromPullRequest : bool
    }

module BuildMetrics =
    
    let longestBuildTime (builds : Build list) =
        builds
        |> List.maxBy (fun x -> x.TimeTaken.TotalMilliseconds)
        |> fun x -> x.TimeTaken

    let shortestBuildTime (builds : Build list) =
        builds
        |> List.minBy (fun x -> x.TimeTaken.TotalMilliseconds)
        |> fun x -> x.TimeTaken

    let averageBuildTime (builds : Build list) =
        builds
        |> List.averageBy (fun x -> x.TimeTaken.TotalMilliseconds)
        |> TimeSpan.FromMilliseconds

module AppVeyor =

    let deserializeJson (json : string) =
        let obj = deserializeJson json :?> JObject
        obj.Value<JArray> "builds"

    let parseStatus (status : string) =
        match status with
        | "success"             -> Success
        | "failed"              -> Failed
        | "cancelled"           -> Cancelled
        | "queued" | "running"  -> Pending
        | _                     -> Unkown

    let getTimeTaken (started   : Nullable<DateTime>)
                     (finished  : Nullable<DateTime>) =
        match started.HasValue with
        | true ->
            match finished.HasValue with
            | true  -> finished.Value - started.Value
            | false -> TimeSpan.Zero
        | false     -> TimeSpan.Zero

    let isPullRequest pullRequestId =
        false

    let convertToBuilds (items : JArray) =
        items 
        |> Seq.map (fun x ->
            let started  = x.Value<Nullable<DateTime>> "started"
            let finished = x.Value<Nullable<DateTime>> "finished"
            {
                Id              = x.Value<int> "buildId"
                BuildNumber     = x.Value<int> "buildNumber"
                TimeTaken       = getTimeTaken started finished
                Status          = x.Value<string> "status" |> parseStatus
                Branch          = x.Value<string> "branch"
                FromPullRequest = x.Value<string> "pullRequestId" |> isPullRequest
            })
        |> Seq.toList

    let getBuilds   (account : string) 
                    (project : string) 
                    (buildCount : int) 
                    (branch : string option) 
                    (includeBuildsFromPullRequest : bool) = 
        async {
            let additionalFilter =
                match branch with
                | Some b -> sprintf "&branch=%s" b
                | None   -> ""

            // Pulling a bit more builds in case some get excluded by the pull request filter
            let recordsNumber = 5 * buildCount

            let url = sprintf "https://ci.appveyor.com/api/projects/%s/%s/history?recordsNumber=%d%s" account project recordsNumber additionalFilter

            let pullRequestFilter build =
                includeBuildsFromPullRequest || not build.FromPullRequest

            let! json = getAsync url Json

            return json
                |> deserializeJson
                |> convertToBuilds
                |> List.filter pullRequestFilter
                |> List.truncate buildCount
        }
    