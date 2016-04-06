namespace BuildStats.Core.Fsharp

type PackageInfo =
    {
        Name        : string
        Version     : string
        Downloads   : int
    }

type INuGetClient =
    abstract member GetPackageInfo : string -> bool -> Async<Option<PackageInfo>>

type NuGetClient(restApiClient : IRestApiClient, serializer : ISerializer) =

    let deserialize (json : string) =
        let obj = (serializer.Deserialize json) :?> Newtonsoft.Json.Linq.JObject
        {
            Name = obj.Value<string> "id"
            Version = obj.Value<string> "version"
            Downloads = obj.Value<int> "totalDownloads"
        }

    interface INuGetClient with
        member this.GetPackageInfo (packageName : string) (includePreReleases : bool) =
            async {
                let url = sprintf "https://api-v3search-0.nuget.org/query?q=%s&skip=0&take=1&prerelease=%b" packageName includePreReleases
                let! content = restApiClient.GetAsync url Json
                match content with
                | Some json -> return Some <| deserialize json
                | None      -> return None
            }