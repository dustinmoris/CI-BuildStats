namespace BuildStats

// -------------------------------------------
// Common Types
// -------------------------------------------

[<AutoOpen>]
module BuildChartTypes =
    open System

    type BuildStatus =
        | Success
        | Failed
        | Cancelled
        | Pending
        | Unknown

    type Build =
        {
            Id              : int
            BuildNumber     : int
            TimeTaken       : TimeSpan
            Status          : BuildStatus
            Branch          : string
            FromPullRequest : bool
        }

// -------------------------------------------
// Helper Functions
// -------------------------------------------

[<RequireQualifiedAccess>]
module BuildStatsHelper =
    open System

    let prFilter   (inclFromPullRequest : bool)
                   (build               : Build) =
        inclFromPullRequest || not build.FromPullRequest

    let timeTaken (started   : Nullable<DateTime>)
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

[<RequireQualifiedAccess>]
module BuildMetrics =
    open System

    let longestBuildTime (builds : Build list) =
        builds
        |> List.filter (fun b -> b.Status = Success)
        |> fun builds ->
            match builds.Length with
            | 0 -> TimeSpan.Zero
            | _ ->
                builds
                |> List.maxBy (fun x -> x.TimeTaken.TotalMilliseconds)
                |> fun x -> x.TimeTaken

    let shortestBuildTime (builds : Build list) =
        builds
        |> List.filter (fun b -> b.Status = Success)
        |> fun builds ->
            match builds.Length with
            | 0 -> TimeSpan.Zero
            | _ ->
                builds
                |> List.minBy (fun x -> x.TimeTaken.TotalMilliseconds)
                |> fun x -> x.TimeTaken

    let averageBuildTime (builds : Build list) =
        builds
        |> List.filter (fun b -> b.Status = Success)
        |> fun builds ->
            match builds.Length with
            | 0 -> TimeSpan.Zero
            | _ ->
                builds
                |> List.averageBy (fun x -> x.TimeTaken.TotalMilliseconds)
                |> TimeSpan.FromMilliseconds

// -------------------------------------------
// CI HTTP Clients
// -------------------------------------------

[<AutoOpen>]
module BuildChartHttpClients =
    open System
    open System.Net
    open System.Net.Http
    open System.Net.Http.Headers
    open Microsoft.Extensions.Caching.Memory
    open Microsoft.FSharp.Core.Option
    open FSharp.Control.Tasks
    open Newtonsoft.Json.Linq
    open Giraffe

    // ...........
    // AppVeyor
    // ```````````

    type AppVeyorHttpClient (httpClient : FallbackHttpClient) =

        let parseToJArray (json : string) =
            let obj = Json.deserialize json :?> JObject
            obj.Value<JArray> "builds"

        let parseStatus (status : string) =
            match status with
            | "success"             -> Success
            | "failed"              -> Failed
            | "cancelled"           -> Cancelled
            | "queued" | "running"  -> Pending
            | _                     -> Unknown

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
                        TimeTaken       = BuildStatsHelper.timeTaken started finished
                    })
                |> Seq.toList

        member __.GetBuildsAsync (authToken           : string option)
                                 (slug                : string * string)
                                 (buildCount          : int)
                                 (branch              : string option)
                                 (inclFromPullRequest : bool) =
            task {
                let account, project = slug
                let branchFilter =
                    match branch with
                    | Some b -> sprintf "&branch=%s" b
                    | None   -> ""

                let url =
                    sprintf "https://ci.appveyor.com/api/projects/%s/%s/history?recordsNumber=%d%s"
                        account project (5 * buildCount) branchFilter

                let requestFactory =
                    fun _ ->
                        let request = new HttpRequestMessage(HttpMethod.Get, url)
                        if authToken.IsSome then
                            let token = AES.decryptUrlEncodedString Env.cryptoKey authToken.Value
                            request.Headers.Authorization <- AuthenticationHeaderValue("Bearer", token)
                        request

                let! json = httpClient.SendAsync requestFactory

                return json
                    |> (Str.toOption
                    >> map parseToJArray
                    >> convertToBuilds)
                    |> List.filter (BuildStatsHelper.prFilter inclFromPullRequest)
                    |> List.truncate buildCount
            }

    // ...........
    // TravisCI
    // ```````````

    type TravisCIHttpClient (httpClient : FallbackHttpClient, cache : IMemoryCache) =

        let tryGetTLD account project =
            let key = sprintf "%s%s" account project
            match cache.TryGetValue<string> key with
            | true, tld -> Some tld
            | false, _  -> None

        let cacheTLD account project tld =
            let key = sprintf "%s%s" account project
            cache.Set(key, tld) |> ignore

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
            | _                     -> Unknown

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
                        TimeTaken       = BuildStatsHelper.timeTaken started finished
                        Status          = parseStatus state
                    })
                |> Seq.toList

        let rec getBuildsAsync (isFallback          : bool)
                               (authToken           : string option)
                               (slug                : string * string)
                               (buildCount          : int)
                               (branch              : string option)
                               (inclFromPullRequest : bool) =
            task {
                let account, project = slug

                let topLevelDomain, token =
                    match isFallback, authToken with
                    | true, _       -> "org", None
                    | false, None   -> defaultArg (tryGetTLD account project) "com", None
                    | false, Some t ->
                        let tkn = AES.decryptUrlEncodedString Env.cryptoKey t
                        "com", Some tkn

                let branchFilter =
                    match branch with
                    | Some b -> sprintf "&branch.name=%s" b
                    | None   -> ""

                let eventFilter =
                    match inclFromPullRequest with
                    | true  -> ""
                    | false -> "&build.event_type[]=push&build.event_type[]=cron&build.event_type[]=api"

                let url =
                    sprintf "https://api.travis-ci.%s/repo/%s/builds?limit=%d%s%s"
                        topLevelDomain
                        (WebUtility.UrlEncode (sprintf "%s/%s" account project))
                        buildCount
                        branchFilter
                        eventFilter

                let requestFactory =
                    fun _ ->
                        let request = new HttpRequestMessage()
                        request.Method <- HttpMethod.Get
                        request.Headers.Add("Travis-API-Version", "3")
                        request.Headers.TryAddWithoutValidation("User-Agent", "BuildStats.info-API") |> ignore
                        request.RequestUri <- Uri(url)
                        match token with
                        | None   -> ()
                        | Some t ->
                            request.Headers.Authorization <- AuthenticationHeaderValue("token", t)
                        request

                let! json = httpClient.SendAsync requestFactory

                let builds =
                    json
                    |> (Str.toOption
                    >> map parseToJArray
                    >> convertToBuilds)

                if isFallback || not builds.IsEmpty then
                    cacheTLD account project topLevelDomain
                    return builds
                else
                    return!
                        getBuildsAsync
                            true
                            authToken
                            slug
                            buildCount
                            branch
                            inclFromPullRequest
            }

        member __.GetBuildsAsync (authToken           : string option)
                                 (slug                : string * string)
                                 (buildCount          : int)
                                 (branch              : string option)
                                 (inclFromPullRequest : bool) =
            getBuildsAsync false authToken slug buildCount branch inclFromPullRequest

    // ...........
    // CircleCI
    // ```````````

    type CircleCIHttpClient (httpClient : FallbackHttpClient) =

        let parseToJArray (json : string) =
            Json.deserialize json :?> JArray

        let parseStatus (status : string) =
            match status with
            | "success"   | "fixed"                 | "no_tests"    -> Success
            | "failed"    | "infrastructure_fail"   | "timedout"    -> Failed
            | "canceled"  | "not_run" | "retried"   | "not_running" -> Cancelled
            | "scheduled" | "queued"                | "running"     -> Pending
            | _                                                     -> Unknown

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
                        TimeTaken       = BuildStatsHelper.timeTaken started finished
                    })
                |> Seq.toList

        member __.GetBuildsAsync  (_                   : string option)
                                  (slug                : string * string)
                                  (buildCount          : int)
                                  (branch              : string option)
                                  (inclFromPullRequest : bool) =
            task {
                let account, project = slug
                let branchFilter =
                    match branch with
                    | Some b -> sprintf "/tree/%s" <| WebUtility.UrlEncode b
                    | None   -> ""

                // CircleCI has a max limit of 100 items per request
                // ToDo: Refactor to pull more items when buildCount is higher
                let limit = min 100 (5 * buildCount)
                let url =
                    sprintf "https://circleci.com/api/v1/project/%s/%s%s?limit=%i"
                        account project branchFilter limit

                let requestFactory = fun _ -> new HttpRequestMessage(HttpMethod.Get, url)
                let! json   = httpClient.SendAsync requestFactory

                return json
                    |> (Str.toOption
                    >> map parseToJArray
                    >> convertToBuilds)
                    |> List.filter (BuildStatsHelper.prFilter inclFromPullRequest)
                    |> List.truncate buildCount
            }

    // ...........
    // Azure Pipelines
    // ```````````

    type AzurePipelinesHttpClient (httpClient : FallbackHttpClient) =

        let parseToJArray (json : string) =
            let obj = Json.deserialize json :?> JObject
            obj.Value<JArray> "value"

        let parseStatus (status : string) =
            match status with
            | "succeeded"                     -> Success
            | "failed" | "partiallySucceeded" -> Failed
            | "canceled"                      -> Cancelled
            | "none"                          -> Pending
            | _                               -> Unknown

        let isPullRequest (reason : string) =
            isNotNull reason && reason.Equals("pullRequest")

        let convertToBuilds (items : JArray option) =
            match items with
            | None       -> []
            | Some items ->
                items
                |> Seq.map (fun x ->
                    let started  = x.Value<Nullable<DateTime>> "startTime"
                    let finished = x.Value<Nullable<DateTime>> "finishTime"
                    {
                        Id              = x.Value<int>     "id"
                        BuildNumber     = x.Value<int>     "id"
                        Status          = x.Value<string>  "result"  |> parseStatus
                        Branch          = (x.Value<string> "sourceBranch").Replace("refs/heads/", "")
                        FromPullRequest = x.Value<string>  "reason" |> isPullRequest
                        TimeTaken       = BuildStatsHelper.timeTaken started finished
                    })
                |> Seq.toList

        member __.GetBuildsAsync  (_                   : string option)
                                  (slug                : string * string * int)
                                  (buildCount          : int)
                                  (branch              : string option)
                                  (inclFromPullRequest : bool) =
            task {
                let account, project, definitionId = slug
                let branchFilter =
                    match branch with
                    | Some b -> sprintf "&branchName=refs/heads/%s" <| WebUtility.UrlEncode b
                    | None   -> ""

                let limit = min 200 (4 * buildCount)
                let apiVersion = "4.1"
                let url =
                    sprintf "https://dev.azure.com/%s/%s/_apis/build/builds?api-version=%s&definitions=%i&$top=%i%s"
                        account project apiVersion definitionId limit branchFilter

                let requestFactory = fun _ -> new HttpRequestMessage(HttpMethod.Get, url)
                let! json   = httpClient.SendAsync requestFactory

                return json
                    |> (Str.toOption
                    >> map parseToJArray
                    >> convertToBuilds)
                    |> List.filter (BuildStatsHelper.prFilter inclFromPullRequest)
                    |> List.truncate buildCount
            }

    // ...........
    // GitHub Actions
    // ```````````

    type GitHubActionsHttpClient (httpClient : FallbackHttpClient) =

        let parseToJArray (json : string) =
            let obj = Json.deserialize json :?> JObject
            obj.Value<int> "total_count", obj.Value<JArray> "workflow_runs"

        let parseStatus (status : string) (conclusion : string) =
            match status with
            | "queued" | "in_progress" -> Pending
            | "completed" ->
                match conclusion with
                | "success"               -> Success
                | "failure" | "timed_out" -> Failed
                | "cancelled"             -> Cancelled
                | "action_required"       -> Pending
                | "neutral" | "skipped"   -> Unknown
                | _                       -> Unknown
            | _ -> Unknown

        let isPullRequest (event : string) =
            isNotNull event && event.Equals("pull_request")

        let convertToBuilds (items : JArray) =
            items
            |> Seq.map (fun x ->
                let started  = x.Value<Nullable<DateTime>> "created_at"
                let finished = x.Value<Nullable<DateTime>> "updated_at"

                let status     = x.Value<string>  "status"
                let conclusion = x.Value<string>  "conclusion"
                let outcome    = parseStatus status conclusion

                {
                    Id              = x.Value<int>     "id"
                    BuildNumber     = x.Value<int>     "run_number"
                    Status          = outcome
                    Branch          = (x.Value<string> "head_branch")
                    FromPullRequest = x.Value<string>  "event" |> isPullRequest
                    TimeTaken       = BuildStatsHelper.timeTaken started finished
                })
            |> Seq.toList

        member __.GetBuildsAsync  (_                   : string option)
                                  (slug                : string * string)
                                  (buildCount          : int)
                                  (branch              : string option)
                                  (inclFromPullRequest : bool) =
            task {
                let owner, repo = slug
                let branchFilter =
                    match branch with
                    | Some b -> sprintf "?branch=%s" <| WebUtility.UrlEncode b
                    | None   -> ""

                // ToDo: Only shows a max of 30 runs per page
                // Check `link` HTTP header for next page
                // or check `total_count` property
                let url =
                    sprintf "https://api.github.com/repos/%s/%s/actions/runs%s"
                        owner repo branchFilter

                let requestFactory = fun _ -> new HttpRequestMessage(HttpMethod.Get, url)
                let! json   = httpClient.SendAsync requestFactory

                let totalCount, jArray =
                    Str.toOption json
                    |> function
                        | None -> 0, None
                        | Some j ->
                            let count, arr = parseToJArray j
                            count, Some arr

                return
                    jArray
                    |> function
                        | None -> []
                        | Some arr ->
                            arr
                            |> convertToBuilds
                            |> List.filter (BuildStatsHelper.prFilter inclFromPullRequest)
                            |> List.truncate buildCount
            }
