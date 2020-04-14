module BuildStats.PackageServices

open System.Net
open System.Net.Http
open Microsoft.FSharp.Core.Option
open FSharp.Control.Tasks.V2.ContextInsensitive
open Newtonsoft.Json.Linq
open BuildStats.Common
open BuildStats.HttpClients

type Package =
    {
        Feed        : string
        Name        : string
        Version     : string
        Downloads   : int
    }

type PackageHttpClient (httpClient : FallbackHttpClient) =
    member __.SendAsync request =
        httpClient.SendAsync request

module NuGet =

    let deserialize (json : string) =
        let obj = Json.deserialize json :?> JObject
        obj.Value<JArray> "data"

    let tryFindByName  (packageName : string)
                       (data        : JArray)  =
        data |> Seq.tryFind(fun item -> item.Value<string> "id" |> Str.equalsCi packageName)

    let convertIntoPackage (packageVersion : string option) (item : JToken) =
        match packageVersion with
        | None ->
            {
                Feed = "nuget"
                Name = item.Value<string> "id"
                Version = item.Value<string> "version"
                Downloads = item.Value<int> "totalDownloads"
            } |> Some
        | Some version ->
            item.Value<JArray> "versions"
            |> fun a -> a.Children()
            |> Seq.tryFind(fun x -> (x .Value<string> "version").Equals version)
            |> function
                | None -> None
                | Some item ->
                    {
                        Feed = "nuget"
                        Name = item.Value<string> "id"
                        Version = item.Value<string> "version"
                        Downloads = item.Value<int> "downloads"
                    } |> Some

    let getPackageAsync (httpClient         : PackageHttpClient)
                        (packageName        : string)
                        (includePreReleases : bool)
                        (packageVersion     : string option) =
        task {
            let url = sprintf "https://api-v2v3search-0.nuget.org/query?q=%s&skip=0&take=10&prerelease=%b" packageName includePreReleases
            let request = new HttpRequestMessage(HttpMethod.Get, url)
            let! json = httpClient.SendAsync request
            return
                json
                |> (Str.toOption
                >> map deserialize
                >> bind (tryFindByName packageName)
                >> bind (convertIntoPackage packageVersion))
        }

module MyGet =

    let skipIfNoPackageFound (json : string) =
        match json with
        | "{\"d\":[]}"  -> None
        | _             -> Some json

    let deserialize (downloadCountKey : string) (json : string) =
        let obj = Json.deserialize json :?> JObject
        let data = obj.Value<JArray> "d"
        {
            Feed = "myget"
            Name = data.[0].Value<string> "Id"
            Version = data.[0].Value<string> "Version"
            Downloads = data.[0].Value<int> downloadCountKey
        }

    let validatePackage packageName package =
        if packageName |> Str.equalsCi package.Name
        then Some package
        else None

    let getPackageAsync (httpClient         : PackageHttpClient)
                        (subDomain          : string)
                        (feedName           : string,
                         packageName        : string)
                        (includePreReleases : bool)
                        (packageVersion     : string option) =
        task {
            let filter =
                match packageVersion with
                | None   -> sprintf "Id eq '%s'" packageName
                | Some v -> sprintf "Id eq '%s' and Version eq '%s'" packageName v
                |> WebUtility.UrlEncode

            let downloadCountKey =
                match packageVersion with
                | Some _ -> "VersionDownloadCount"
                | None   -> "DownloadCount"

            let url = sprintf "https://%s.myget.org/F/%s/api/v2/Packages()?$filter=%s&$orderby=Published desc&$top=1" subDomain feedName filter
            let request = new HttpRequestMessage(HttpMethod.Get, url)
            let! json = httpClient.SendAsync request
            return
                json
                |> (Str.toOption
                >> bind skipIfNoPackageFound
                >> map (deserialize downloadCountKey)
                >> bind (validatePackage packageName))
        }

    let getPackageFromOfficialFeedAsync (httpClient         : PackageHttpClient)
                                        (slug               : string * string)
                                        (includePreReleases : bool)
                                        (packageVersion     : string option) =
        getPackageAsync httpClient "www" slug includePreReleases packageVersion

    let getPackageFromEnterpriseFeedAsync (httpClient         : PackageHttpClient)
                                          (slug               : string * string * string)
                                          (includePreReleases : bool)
                                          (packageVersion     : string option) =
        let (subDomain, feedName, packageName) =  slug
        getPackageAsync httpClient subDomain (feedName, packageName) includePreReleases packageVersion