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
                        (branch : string) 
                        (includeBuildsFromPullRequest : bool) = 
    async {
        let url = sprintf "https://ci.appveyor.com/api/projects/%s/%s/history?recordsNumber=%d" account project buildCount
        let! json = getAsync url Json
        return json |> parseAppVeyorContent
    }
    