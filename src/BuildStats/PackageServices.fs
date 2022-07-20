namespace BuildStats

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

[<RequireQualifiedAccess>]
module NuGet =
    open System.Net.Http
    open Microsoft.FSharp.Core.Option
    open Newtonsoft.Json.Linq

    let deserialize (json : string) =
        let obj = Json.deserialize json :?> JObject
        obj.Value<JArray> "data"

    let tryFindByName  (packageName : string)
                       (data        : JArray)  =
        data |> Seq.tryFind(fun item -> item.Value<string> "id" |> Str.equalsCi packageName)

    let mapSemVer200 (version : string) =
        version.Split([| '+' |], 2).[0]

    let convertIntoPackage (packageVersion : string option) (item : JToken) =
        match packageVersion with
        | None ->
            {
                Feed = "nuget"
                Name = item.Value<string> "id"
                Version = item.Value<string> "version" |> mapSemVer200
                Downloads = item.Value<int> "totalDownloads"
            } |> Some
        | Some version ->
            item.Value<JArray> "versions"
            |> fun a -> a.Children()
            |> Seq.tryFind(fun x ->
                (x.Value<string> "version" |> mapSemVer200).Equals version)
            |> function
                | None -> None
                | Some item ->
                    {
                        Feed = "nuget"
                        Name = item.Value<string> "id"
                        Version = item.Value<string> "version" |> mapSemVer200
                        Downloads = item.Value<int> "downloads"
                    } |> Some

    let getPackageAsync (httpClient         : PackageHttpClient)
                        (packageName        : string)
                        (includePreReleases : bool)
                        (packageVersion     : string option) =
        task {
            let url = sprintf "https://api-v2v3search-0.nuget.org/query?q=%s&skip=0&take=10&prerelease=%b&semVerLevel=2.0.0" packageName includePreReleases
            let requestFactory = fun _ -> new HttpRequestMessage(HttpMethod.Get, url)
            let! json = httpClient.SendAsync requestFactory
            return
                json
                |> (Str.toOption
                >> map deserialize
                >> bind (tryFindByName packageName)
                >> bind (convertIntoPackage packageVersion))
        }

[<RequireQualifiedAccess>]
module Crate =
    open System.Net.Http
    open Microsoft.FSharp.Core.Option
    open Newtonsoft.Json.Linq

    let deserialize (json : string) =
        Json.deserialize json :?> JObject

    let convertIntoPackage (packageVersion : string option) (obj : JObject) =
        match packageVersion with
        | None ->
            let create = obj.Value<JObject> "crate"
            {
                Feed = "crates.io"
                Name = create.Value<string> "id"
                Version = create.Value<string> "max_version"
                Downloads = create.Value<int> "downloads"
            } |> Some
        | Some version ->
            let versions = obj.Value<JArray> "versions"
            versions
            |> fun a -> a.Children()
            |> Seq.tryFind(fun x ->
                (x.Value<string> "num").Equals version)
            |> function
                | None -> None
                | Some item ->
                    {
                        Feed = "crates.io"
                        Name = item.Value<string> "crate"
                        Version = item.Value<string> "num"
                        Downloads = item.Value<int> "downloads"
                    } |> Some

    let getPackageAsync (httpClient         : PackageHttpClient)
                        (packageName        : string)
                        (includePreReleases : bool) // ToDo
                        (packageVersion     : string option) =
        task {
            let url = sprintf "https://crates.io/api/v1/crates/%s" packageName
            let requestFactory = fun _ -> new HttpRequestMessage(HttpMethod.Get, url)
            let! json = httpClient.SendAsync requestFactory

            return
                json
                |> (Str.toOption
                >> map deserialize
                >> bind (convertIntoPackage packageVersion))
        }

[<RequireQualifiedAccess>]
module MyGet =
    open System.Net.Http
    open Microsoft.FSharp.Core.Option
    open Newtonsoft.Json.Linq

    let deserialize (json : string) =
        let obj = Json.deserialize json :?> JObject
        obj.Value<JArray> "data"

    let tryFindByName  (packageName : string)
                       (data        : JArray)  =
        data |> Seq.tryFind(fun item -> item.Value<string> "id" |> Str.equalsCi packageName)

    let mapSemVer200 (version : string) =
        version.Split([| '+' |], 2).[0]

    let convertIntoPackage (packageVersion : string option) (item : JToken) =
        match packageVersion with
        | None ->
            {
                Feed = "nuget"
                Name = item.Value<string> "id"
                Version = item.Value<string> "version" |> mapSemVer200
                Downloads = item.Value<int> "totaldownloads"
            } |> Some
        | Some version ->
            item.Value<JArray> "versions"
            |> fun a -> a.Children()
            |> Seq.tryFind(fun x ->
                (x.Value<string> "version" |> mapSemVer200).Equals version)
            |> function
                | None -> None
                | Some item ->
                    {
                        Feed = "nuget"
                        Name = item.Value<string> "id"
                        Version = item.Value<string> "version" |> mapSemVer200
                        Downloads = item.Value<int> "totaldownloads"
                    } |> Some

    let getPackageAsync (httpClient         : PackageHttpClient)
                        (subDomain          : string)
                        (feedName           : string,
                         packageName        : string)
                        (includePreReleases : bool)
                        (packageVersion     : string option) =
        task {
            let url = sprintf "https://%s.myget.org/F/%s/api/v3/query?q=%s&skip=0&take=10&prerelease=%b&semVerLevel=2.0.0" subDomain feedName packageName includePreReleases
            let requestFactory = fun _ -> new HttpRequestMessage(HttpMethod.Get, url)
            let! json = httpClient.SendAsync requestFactory
            return
                json
                |> (Str.toOption
                >> map deserialize
                >> bind (tryFindByName packageName)
                >> bind (convertIntoPackage packageVersion))
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
