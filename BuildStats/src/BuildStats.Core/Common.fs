module BuildStats.Core.Common

open System
open System.Net
open System.Net.Http
open System.IO
open System.Threading.Tasks
open Newtonsoft.Json

/// -------------------------------------
/// Common functions
/// -------------------------------------

let inline isNotNull x = isNull x |> not

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
