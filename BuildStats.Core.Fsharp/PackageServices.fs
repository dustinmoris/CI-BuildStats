module PackageServices

open System
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
 
let matches (name1 : string) 
            (name2 : string) =
    name1.Equals(name2, StringComparison.InvariantCultureIgnoreCase)

let getNuGetPackageAsync    (packageName : string) 
                            (includePreReleases : bool) =
    async {
        let deserialize (json : string) =
            let obj = deserializeJson json :?> JObject
            obj.Value<JArray> "data"            

        let tryFindDesiredItem (data : JArray) =
            data |> Seq.tryFind(fun item -> item.Value<string> "id" |> matches packageName)

        let convertIntoPackage (item : JToken option) =
            match item with
            | Some x ->
                Some {
                    Name = x.Value<string> "id"
                    Version = x.Value<string> "version"
                    Downloads = x.Value<int> "totalDownloads"
                }
            | None -> None

        let url = sprintf "https://api-v3search-0.nuget.org/query?q=%s&skip=0&take=10&prerelease=%b" packageName includePreReleases
        let! json = getAsync url Json
        return json
            |> deserialize 
            |> tryFindDesiredItem
            |> convertIntoPackage
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
        return
            match deserialize json with
            | p when p.Name |> matches packageName  -> Some p
            | _                                     -> None
    }