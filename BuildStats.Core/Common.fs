module Common

open System
open System.Net
open System.Net.Http
open System.IO
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

    let runTaskAsyncAsChild<'T> (task : Task<'T>) =
        async {
            let! token = task |> Async.AwaitTask |> Async.StartChild            
            return! token
        }

    let createHttpPostRequest (url : string) =
        let request = HttpWebRequest.CreateHttp(url)
        // Setting HTTP header "Connection: Close" to free up
        // the underlying TCP connection after a successful request
        request.KeepAlive                       <- false
        // Disabling the automatic addition of the "Expect: 100-continue"
        // HTTP header to save an additional round trip
        // See: http://haacked.com/archive/2004/05/15/http-web-request-expect-100-continue.aspx/
        request.ServicePoint.Expect100Continue  <- false
        request

    let setAcceptHeader (acceptType : AcceptType)
                        (request    : HttpWebRequest) =
            request.Accept <- acceptType.AsString
            request

    let setHttpVerb (verb       : string)
                    (request    : HttpWebRequest) =
            request.Method <- verb
            request

    let sendRequestAsyc (request : HttpWebRequest) =
        async {
            use! response = request.GetResponseAsync() |> runTaskAsyncAsChild
            let statusCode = (response :?> HttpWebResponse).StatusCode
            use reader = new StreamReader(response.GetResponseStream())
            let! body = reader.ReadToEndAsync() |> runTaskAsyncAsChild
            return statusCode, body
        }

    let getAsync (url        : string)
                 (acceptType : AcceptType) =
        async {
            let key = sprintf "%s|%s" url acceptType.AsString

            match MemoryCache.get key with
            | Some obj -> return obj
            | None ->
                let! statusCode, body =
                    url
                    |> createHttpPostRequest
                    |> setHttpVerb "GET"
                    |> setAcceptHeader acceptType
                    |> sendRequestAsyc

                if statusCode = HttpStatusCode.OK then
                    MemoryCache.set key body
                    return body
                else return ""
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