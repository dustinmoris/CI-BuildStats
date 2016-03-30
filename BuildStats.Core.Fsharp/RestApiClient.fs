namespace BuildStats.Core.Fsharp

open System
open System.Net
open System.Net.Http

type AcceptType =
    | Json
    | Xml
    member x.AsString =
        match x with
        | Json  -> "application/json"
        | Xml   -> "application/xml"

type IRestApiClient =
    abstract member GetAsync : string -> AcceptType -> Async<Option<string>>

type RestApiClient() =
    interface IRestApiClient with
        member this.GetAsync (url : string) (acceptType : AcceptType) =
            async {
                use httpClient = new HttpClient()
                httpClient.DefaultRequestHeaders.Add("accept", acceptType.AsString)
                let! response = httpClient.GetAsync(url) |> Async.AwaitTask                
                match response.StatusCode with
                | HttpStatusCode.OK -> 
                    let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
                    return Some content
                | _ -> return None
            }

type PackageInfo =
    {
        Name        : string
        Version     : string
        Downloads   : int
    }

//type NuGetClient() =
//    let client = new RestApiClient()
//    let x =
//        ()