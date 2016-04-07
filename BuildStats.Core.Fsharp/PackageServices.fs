namespace BuildStats.Core.Fsharp

open Newtonsoft.Json.Linq
open System.Net

type Package =
    {
        Name        : string
        Version     : string
        Downloads   : int
    }

type INuGetClient =
    abstract member GetPackageAsync : string -> bool -> Async<Option<Package>>

type IMyGetClient =
    abstract member GetPackageAsync : string -> string -> bool -> Async<Option<Package>>

type NuGetClient(restApiClient : IRestApiClient, serializer : ISerializer) =

    let deserialize (json : string) =
        let obj = (serializer.Deserialize json) :?> JObject
        let data = obj.Value<JArray> "data"
        {
            Name = data.[0].Value<string> "id"
            Version = data.[0].Value<string> "version"
            Downloads = data.[0].Value<int> "totalDownloads"
        }

    interface INuGetClient with
        member this.GetPackageAsync (packageName : string) (includePreReleases : bool) =
            async {
                let url = sprintf "https://api-v3search-0.nuget.org/query?q=%s&skip=0&take=1&prerelease=%b" packageName includePreReleases
                let! content = restApiClient.GetAsync url Json
                match content with
                | Some json -> return Some <| deserialize json
                | None      -> return None
            }

type MyGetClient(restApiClient : IRestApiClient, serializer : ISerializer) =
    
    let deserialize (json : string) =
        let obj = (serializer.Deserialize json) :?> JObject
        let data = obj.Value<JArray> "d"
        {
            Name = data.[0].Value<string> "Id"
            Version = data.[0].Value<string> "Version"
            Downloads = data.[0].Value<int> "DownloadCount"
        }

    interface IMyGetClient with
        member this.GetPackageAsync (feedName : string) (packageName : string) (includePreReleases : bool) =
            async {
                let filter = sprintf "Id eq '%s'" packageName |> WebUtility.UrlEncode
                let url = sprintf "https://www.myget.org/F/%s/api/v2/Packages()?$filter=%s&$orderby=Published desc&$top=1" feedName filter
                let! content = restApiClient.GetAsync url Json
                match content with
                | Some json -> return Some <| deserialize json
                | None      -> return None
            }