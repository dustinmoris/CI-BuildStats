module BuildHistoryCharts

open System
open Microsoft.FSharp.Core.Option
open Newtonsoft.Json.Linq
open Common

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

let getTimeTaken (started   : Nullable<DateTime>)
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

// -------------------------------------------
// AppVeyor
// -------------------------------------------

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
        pullRequestId <> null

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
                    TimeTaken       = getTimeTaken started finished
                })
            |> Seq.toList

    let getBuilds   (account             : string) 
                    (project             : string) 
                    (buildCount          : int) 
                    (branch              : string option) 
                    (inclFromPullRequest : bool) = 
        async {
            let additionalFilter =
                match branch with
                | Some b -> sprintf "&branch=%s" b
                | None   -> ""
                
            let url = 
                sprintf "https://ci.appveyor.com/api/projects/%s/%s/history?recordsNumber=%d%s" 
                    account project (5 * buildCount) additionalFilter

            let! json = Http.getAsync url Json

            return json
                |> (Str.neutralize
                >> map parseToJArray
                >> convertToBuilds)
                |> List.filter (pullRequestFilter inclFromPullRequest)
                |> List.truncate buildCount
        }

// -------------------------------------------
// TravisCI
// -------------------------------------------

module TravisCI =

    let parseToJArray (json : string) = 
        Json.deserialize json :?> JArray

    let parseStatus (state  : string)
                    (result : Nullable<int>) =
        match state with
        | "finished" -> 
            match result with
            | x when not x.HasValue -> Failed
            | x when x.Value = 0    -> Success
            | _                     -> Failed
        | "started" -> Pending
        | _         -> Unkown

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
                let result   = x.Value<Nullable<int>>      "result"
                {
                    Id              = x.Value<int>    "id"
                    BuildNumber     = x.Value<int>    "number"
                    Branch          = x.Value<string> "branch"
                    FromPullRequest = x.Value<string> "event_type" |> isPullRequest
                    TimeTaken       = getTimeTaken  started finished
                    Status          = parseStatus   state   result
                })
            |> Seq.toList

    let numberOfBuildsPerPage = 25
    
    let rec getBatchOfBuilds (account          : string) 
                             (project          : string)
                             (afterBuildNumber : int option)
                             (maxRequests      : int)
                             (requestCount     : int) : Async<Build list> =
        
        async {
            let additionalQuery =
                match afterBuildNumber with
                | Some x -> sprintf "?after_number=%i" x
                | None   -> ""

            let url = sprintf "https://api.travis-ci.org/repos/%s/%s/builds%s" account project additionalQuery
            let! json = Http.getAsync url Json
            
            let requestCount' = requestCount + 1

            let batch =
                json
                |> (Str.neutralize 
                >> map parseToJArray)
                |> convertToBuilds
            
            match batch with
            | x when x.IsEmpty                          -> return []
            | x when x.Length < numberOfBuildsPerPage   -> return x
            | x when requestCount' = maxRequests        -> return x
            | _ ->
                let lastBuild = batch |> Seq.last
                let! nextBatch = getBatchOfBuilds account project (Some lastBuild.BuildNumber) maxRequests requestCount'
                return batch @ nextBatch
        }

    let getBuilds   (account             : string) 
                    (project             : string) 
                    (buildCount          : int) 
                    (branch              : string option) 
                    (inclFromPullRequest : bool) = 
        async {
            let maxRequests = int(Math.Ceiling((float buildCount / float numberOfBuildsPerPage) * 5.0))
            let! builds = getBatchOfBuilds account project None maxRequests 0

            let branchFilter build = 
                match branch with
                | Some b -> build.Branch = b
                | None   -> true

            return builds
                |> List.filter branchFilter
                |> List.filter (pullRequestFilter inclFromPullRequest)
                |> List.truncate buildCount
        }

// -------------------------------------------
// CircleCI
// -------------------------------------------

// ToDo