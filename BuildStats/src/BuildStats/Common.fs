module BuildStats.Common

open System
open System.Net
open System.Net.Http
open System.IO
open System.Threading.Tasks
open Newtonsoft.Json

/// -------------------------------------
/// String helper functions
/// -------------------------------------

module Str =

    let matches (name1 : string)
                (name2 : string) =
        name1.Equals(name2, StringComparison.CurrentCultureIgnoreCase)

    let toOption str =
        match str with
        | null | "" -> None
        | _         -> Some str

/// -------------------------------------
/// Serialization
/// -------------------------------------

module Json =

    let serialize (x : obj) =
        JsonConvert.SerializeObject x

    let deserialize (json : string) =
        JsonConvert.DeserializeObject json

/// -------------------------------------
/// Http
/// -------------------------------------

module Http =

    let httpClient = new HttpClient()
    httpClient.DefaultRequestHeaders.Accept.Add(Headers.MediaTypeWithQualityHeaderValue("application/json"))

    let getJson (url : string) =
        async {
            let! result = httpClient.GetAsync url |> Async.AwaitTask
            match result.StatusCode with
            | HttpStatusCode.OK ->
                return!
                    result.Content.ReadAsStringAsync()
                    |> Async.AwaitTask
            | _ -> return ""
        }