open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful
open PackageServices
open Suave.Json
open Serializers
open Suave.RequestErrors

let JSON obj =
    serializeJson obj
    |> OK
    >=> Writers.setMimeType "application/json; charset=utf-8"

let getNuGetPackage packageName =
    fun (ctx : HttpContext) ->
        async {
            let includePreReleases =
                match ctx.request.queryParam "includePreReleases" with
                | Choice1Of2 value  -> bool.Parse value
                | _                 -> false
            let! package = getNuGetPackageAsync packageName includePreReleases
            return!
                match package with
                | None      -> NOT_FOUND (sprintf "NuGet package %s could not be found." packageName) ctx
                | Some pkg  -> JSON pkg ctx
        }

let app = 
    GET >=> choose [
        pathScan "/nuget/%s" getNuGetPackage
    ]

[<EntryPoint>]
let main argv = 
    startWebServer defaultConfig app
    0