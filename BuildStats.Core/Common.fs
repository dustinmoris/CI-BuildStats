module Common

open System
open System.Net
open System.Net.Http
open System.Threading.Tasks
open Newtonsoft.Json

type AcceptType =
    | Json
    | Xml
    member x.AsString =
        match x with
        | Json  -> "application/json"
        | Xml   -> "application/xml"

module Http =

    let runTaskAsync<'T> (task : Task<'T>) =
        async {
            let! token = task |> Async.AwaitTask |> Async.StartChild            
            return! token
        }

    let getAsync (url        : string)
                 (acceptType : AcceptType) =
        async {
            use httpClient = new HttpClient()
            httpClient.DefaultRequestHeaders.Add("accept", acceptType.AsString)
            
            let! response = httpClient.GetAsync(url) |> runTaskAsync

            match response.StatusCode with
            | HttpStatusCode.OK -> return! response.Content.ReadAsStringAsync() |> runTaskAsync
            | _                 -> return ""
        }

module Json =

    let serialize obj =
        JsonConvert.SerializeObject(obj)

    let deserialize (json : string) =
        JsonConvert.DeserializeObject(json)

module Str =

    let matches (name1 : string) 
                (name2 : string) =
        name1.Equals(name2, StringComparison.InvariantCultureIgnoreCase)

    let neutralize str =
        match str with
        | null | "" -> None
        | _         -> Some str