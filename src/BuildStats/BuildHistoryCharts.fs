module BuildStats.BuildHistoryCharts

open System
open System.Net
open System.Net.Http
open System.Net.Http.Headers
open Microsoft.FSharp.Core.Option
open Newtonsoft.Json.Linq
open Giraffe
open FSharp.Control.Tasks.V2.ContextInsensitive
open BuildStats.Common

// -------------------------------------------
// Common Types and Functions
// -------------------------------------------

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

let pullRequestFilter   (inclFromPullRequest : bool)
                        (build  : Build) =
    inclFromPullRequest || not build.FromPullRequest

let calculateTimeTaken (started   : Nullable<DateTime>)
                       (finished  : Nullable<DateTime>) =
    match started.HasValue with
    | true ->
        match finished.HasValue with
        | true  -> finished.Value - started.Value
        | false -> TimeSpan.Zero
    | false     -> TimeSpan.Zero

// -------------------------------------------
// Build Metrics
// -------------------------------------------

module BuildMetrics =

    let longestBuildTime (builds : Build list) =
        match builds.Length with
        | 0 -> TimeSpan.Zero
        | _ ->
            builds
            |> List.maxBy (fun x -> x.TimeTaken.TotalMilliseconds)
            |> fun x -> x.TimeTaken

    let shortestBuildTime (builds : Build list) =
        match builds.Length with
        | 0 -> TimeSpan.Zero
        | _ ->
            builds
            |> List.minBy (fun x -> x.TimeTaken.TotalMilliseconds)
            |> fun x -> x.TimeTaken

    let averageBuildTime (builds : Build list) =
        match builds.Length with
        | 0 -> TimeSpan.Zero
        | _ ->
            builds
            |> List.averageBy (fun x -> x.TimeTaken.TotalMilliseconds)
            |> TimeSpan.FromMilliseconds

// -------------------------------------------
// AppVeyor
// -------------------------------------------

[<RequireQualifiedAccess>]
module AppVeyor =

    let parseToJArray (json : string) =
        let obj = Json.deserialize json :?> JObject
        obj.Value<JArray> "builds"

    let parseStatus (status : string) =
        match status with
        | "success"             -> Success
        | "failed"              -> Failed
        | "cancelled"           -> Cancelled
        | "queued" | "running"  -> Pending
        | _                     -> Unkown

    let isPullRequest (pullRequestId : string) =
        isNotNull pullRequestId

    let convertToBuilds (items : JArray option) =
        match items with
        | None       -> []
        | Some items ->
            items
            |> Seq.map (fun x ->
                let started  = x.Value<Nullable<DateTime>> "started"
                let finished = x.Value<Nullable<DateTime>> "finished"
                {
                    Id              = x.Value<int>    "buildId"
                    BuildNumber     = x.Value<int>    "buildNumber"
                    Status          = x.Value<string> "status"        |> parseStatus
                    Branch          = x.Value<string> "branch"
                    FromPullRequest = x.Value<string> "pullRequestId" |> isPullRequest
                    TimeTaken       = calculateTimeTaken started finished
                })
            |> Seq.toList

    let getBuilds   (authToken           : string option) // ToDo
                    (account             : string)
                    (project             : string)
                    (buildCount          : int)
                    (branch              : string option)
                    (inclFromPullRequest : bool) =
        task {
            let additionalFilter =
                match branch with
                | Some b -> sprintf "&branch=%s" b
                | None   -> ""

            let url =
                sprintf "https://ci.appveyor.com/api/projects/%s/%s/history?recordsNumber=%d%s"
                    account project (5 * buildCount) additionalFilter

            let! json = Http.getJson url

            return json
                |> (Str.toOption
                >> map parseToJArray
                >> convertToBuilds)
                |> List.filter (pullRequestFilter inclFromPullRequest)
                |> List.truncate buildCount
        }

// -------------------------------------------
// TravisCI
// -------------------------------------------

[<RequireQualifiedAccess>]
module TravisCI =

    let parseToJArray (json : string) =
        let obj = Json.deserialize json :?> JObject
        obj.Value<JArray> "builds"

    let parseStatus (status : string) =
        match status with
        | "failed"  | "broken"
        | "failing" | "errored" -> Failed
        | "passed"  | "fixed"   -> Success
        | "canceled"            -> Cancelled
        | "pending"             -> Pending
        | _                     -> Unkown

    let isPullRequest eventType = eventType = "pull_request"

    let convertToBuilds (items : JArray option) =
        match items with
        | None -> []
        | Some items ->
            items
            |> Seq.map (fun x ->
                let started  = x.Value<Nullable<DateTime>> "started_at"
                let finished = x.Value<Nullable<DateTime>> "finished_at"
                let state    = x.Value<string>             "state"
                {
                    Id              = x.Value<int>    "id"
                    BuildNumber     = x.Value<int>    "number"
                    Branch          = (x.Value<JObject> "branch").Value<string> "name"
                    FromPullRequest = x.Value<string> "event_type" |> isPullRequest
                    TimeTaken       = calculateTimeTaken started finished
                    Status          = parseStatus state
                })
            |> Seq.toList

    let rec getBuilds   (forceFallback       : bool)
                        (authToken           : string option)
                        (account             : string)
                        (project             : string)
                        (buildCount          : int)
                        (branch              : string option)
                        (inclFromPullRequest : bool) =
        task {
            let request = new HttpRequestMessage()
            request.Method <- HttpMethod.Get
            request.Headers.Add("Travis-API-Version", "3")
            request.Headers.TryAddWithoutValidation("User-Agent", "https://buildstats.info") |> ignore

            let topLevelDomain =
                match forceFallback, authToken with
                | true, _     -> "org"
                | false, None -> "com"
                | false, Some token ->
                    request.Headers.Authorization <- AuthenticationHeaderValue("token", token)
                    "com"

            let branchFilter =
                match branch with
                | Some b -> sprintf "&branch.name=%s" b
                | None   -> ""

            let eventFilter =
                match inclFromPullRequest with
                | true -> ""
                | false -> "&build.event_type[]=push&build.event_type[]=cron&build.event_type[]=api"

            let url =
                sprintf "https://api.travis-ci.%s/repo/%s/builds?limit=%d%s%s"
                    topLevelDomain
                    (WebUtility.UrlEncode (sprintf "%s/%s" account project))
                    buildCount
                    branchFilter
                    eventFilter

            request.RequestUri <- new Uri(url)

            let! json = Http.sendRequest request

            if (Str.toOption json).IsNone && authToken.IsNone
            then
                return!
                    getBuilds
                        true
                        authToken
                        account
                        project
                        buildCount
                        branch
                        inclFromPullRequest
            else
                return json
                    |> (Str.toOption
                    >> map parseToJArray
                    >> convertToBuilds)
        }

// -------------------------------------------
// CircleCI
// -------------------------------------------

[<RequireQualifiedAccess>]
module CircleCI =

    let parseToJArray (json : string) =
        Json.deserialize json :?> JArray

    let parseStatus (status : string) =
        match status with
        | "success"   | "fixed"                 | "no_tests"    -> Success
        | "failed"    | "infrastructure_fail"   | "timedout"    -> Failed
        | "canceled"  | "not_run" | "retried"   | "not_running" -> Cancelled
        | "scheduled" | "queued"                | "running"     -> Pending
        | _                                                     -> Unkown

    let isPullRequest (subject : string) =
        isNotNull subject && subject.ToLowerInvariant().Contains("pull request")

    let convertToBuilds (items : JArray option) =
        match items with
        | None       -> []
        | Some items ->
            items
            |> Seq.map (fun x ->
                let started  = x.Value<Nullable<DateTime>> "start_time"
                let finished = x.Value<Nullable<DateTime>> "stop_time"
                {
                    Id              = x.Value<int>    "build_num"
                    BuildNumber     = x.Value<int>    "build_num"
                    Status          = x.Value<string> "status"  |> parseStatus
                    Branch          = x.Value<string> "branch"
                    FromPullRequest = x.Value<string> "subject" |> isPullRequest
                    TimeTaken       = calculateTimeTaken started finished
                })
            |> Seq.toList

    let getBuilds   (authToken           : string option)
                    (account             : string)
                    (project             : string)
                    (buildCount          : int)
                    (branch              : string option)
                    (inclFromPullRequest : bool) =
        task {
            let additionalFilter =
                match branch with
                | Some b -> sprintf "/tree/%s" <| WebUtility.UrlEncode b
                | None   -> ""

            // CircleCI has a max limit of 100 items per request
            // ToDo: Refactor to pull more items when buildCount is higher
            let limit = min 100 (5 * buildCount)

            let url =
                sprintf "https://circleci.com/api/v1/project/%s/%s%s?limit=%i"
                    account project additionalFilter limit

            let! json = Http.getJson url

            return json
                |> (Str.toOption
                >> map parseToJArray
                >> convertToBuilds)
                |> List.filter (pullRequestFilter inclFromPullRequest)
                |> List.truncate buildCount
        }