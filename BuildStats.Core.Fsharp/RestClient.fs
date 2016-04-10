module RestClient

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

let getAsync (url : string) (acceptType : AcceptType) =
    async {
        use httpClient = new HttpClient()
        httpClient.DefaultRequestHeaders.Add("accept", acceptType.AsString)
        let! response = httpClient.GetAsync(url) |> Async.AwaitTask                
        match response.StatusCode with
        | HttpStatusCode.OK -> return! response.Content.ReadAsStringAsync() |> Async.AwaitTask
        | _                 -> return ""
    }