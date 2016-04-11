module PackageServices

open System.Net
open Newtonsoft.Json.Linq
open Serializers
open RestClient

type Package =
    {
        Name        : string
        Version     : string
        Downloads   : int
    }

let getNuGetPackageAsync    (packageName : string) 
                            (includePreReleases : bool) =
    async {
        let deserialize (json : string) =
            let obj = deserializeJson json :?> JObject
            let data = obj.Value<JArray> "data"
            
            {
                Name = data.[0].Value<string> "id"
                Version = data.[0].Value<string> "version"
                Downloads = data.[0].Value<int> "totalDownloads"
            }

        let url = sprintf "https://api-v3search-0.nuget.org/query?q=%s&skip=0&take=10&prerelease=%b" packageName includePreReleases
        let! json = getAsync url Json
        return deserialize json
    }

let getMyGetPackageAsync    (feedName : string) 
                            (packageName : string)
                            (includePreReleases : bool) =
    async {
        let deserialize (json : string) =
            let obj = deserializeJson json :?> JObject
            let data = obj.Value<JArray> "d"
            {
                Name = data.[0].Value<string> "Id"
                Version = data.[0].Value<string> "Version"
                Downloads = data.[0].Value<int> "DownloadCount"
            }

        let filter = sprintf "Id eq '%s'" packageName |> WebUtility.UrlEncode
        let url = sprintf "https://www.myget.org/F/%s/api/v2/Packages()?$filter=%s&$orderby=Published desc&$top=1" feedName filter
        let! json = getAsync url Json
        return deserialize json
    }