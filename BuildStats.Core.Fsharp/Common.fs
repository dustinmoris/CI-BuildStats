module Common

open System
open System.Net
open System.Net.Http
open Newtonsoft.Json

module RESTful =

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

module Json =

    let serialize obj =
        JsonConvert.SerializeObject(obj)

    let deserialize(json : string) =
        JsonConvert.DeserializeObject(json)

module Str =

    let matches (name1 : string) 
                (name2 : string) =
        name1.Equals(name2, StringComparison.InvariantCultureIgnoreCase)

    let neutralize str =
        match str with
        | null | "" -> None
        | _         -> Some str