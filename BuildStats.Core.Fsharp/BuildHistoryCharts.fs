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

    let getTimeTaken started finished =
        TimeSpan.FromSeconds(200.0)

    let isPullRequest pullRequestId =
        false

    let convertToBuilds (items : JArray) =
        items 
        |> Seq.map (fun x ->
            let startedDate  = x.Value<Nullable<DateTime>> "started"
            let finishedDate = x.Value<Nullable<DateTime>> "finished"
            let timeTaken =
                match startedDate.HasValue with
                | true ->
                    match finishedDate.HasValue with
                    | true  -> finishedDate.Value - startedDate.Value
                    | false -> TimeSpan.Zero
                | false     -> TimeSpan.Zero
            { 
                Id              = x.Value<int> "buildId"
                BuildNumber     = x.Value<int> "buildNumber"
                TimeTaken       = timeTaken
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

            let recordsNumber = 5 * buildCount

            let url = sprintf "https://ci.appveyor.com/api/projects/%s/%s/history?recordsNumber=%d%s" account project recordsNumber additionalFilter

            let pullRequestFilter build =
                includeBuildsFromPullRequest || not build.FromPullRequest

            let! json = getAsync url Json

            return json
                |> deserializeJson
                |> convertToBuilds
                |> List.filter pullRequestFilter
                |> List.rev
                |> List.truncate buildCount
        }
    