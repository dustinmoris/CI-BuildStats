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

let parseAppVeyorContent (json : string) =
    let obj = (deserializeJson json) :?> JObject
    [ 
        { 
            Id = 1
            BuildNumber = 1
            TimeTaken = TimeSpan.FromDays(1.0)
            Status = Success
            Branch = "test"
            FromPullRequest = true
        }
    ]

let getAppVeyorBuilds   (account : string) 
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
            |> parseAppVeyorContent
            |> Seq.filter pullRequestFilter
            |> Seq.truncate buildCount
    }
    