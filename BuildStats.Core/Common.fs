module Common

open System
open System.Net
open System.Net.Http
open System.Threading.Tasks
open System.Runtime.Caching
open Newtonsoft.Json

type AcceptType =
    | Json
    | Xml
    member x.AsString =
        match x with
        | Json  -> "application/json"
        | Xml   -> "application/xml"

module MemoryCache =
    
    let cache   = MemoryCache.Default

    let oneMinuteExpiration() =
        DateTimeOffset.Now + TimeSpan.FromMinutes(1.0)

    let get<'T> (key : string) =
        let obj = cache.[key]
        match obj with
        | null  -> None
        | _     -> Some (obj :?> 'T)

    let set<'T> (key : string) (value : 'T) =
        cache.Set(key, value, oneMinuteExpiration())

module Http =

    let runTaskAsync<'T> (task : Task<'T>) =
        async {
            let! token = task |> Async.AwaitTask |> Async.StartChild            
            return! token
        }

    let getAsync (url        : string)
                 (acceptType : AcceptType) =
        async {
            let key = sprintf "%s|%s" url acceptType.AsString

            match MemoryCache.get key with
            | Some obj -> return obj
            | None ->
                use httpClient = new HttpClient()
                httpClient.DefaultRequestHeaders.Add("accept", acceptType.AsString)
            
                let! response = httpClient.GetAsync(url) |> runTaskAsync

                match response.StatusCode with
                | HttpStatusCode.OK -> 
                    let! content = response.Content.ReadAsStringAsync() |> runTaskAsync
                    MemoryCache.set key content
                    return content
                | _ -> return ""
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