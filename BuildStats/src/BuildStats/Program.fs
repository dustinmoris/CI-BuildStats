module BuildStats.App

open System
open System.IO
open System.Text
open System.Security.Cryptography
open System.Collections.Generic
open System.Reflection
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe.Common
open Giraffe.HttpHandlers
open Giraffe.Middleware
open Giraffe.XmlViewEngine
open Giraffe.HttpContextExtensions
open BuildStats.PackageServices
open BuildStats.Models

// ---------------------------------
// Web app
// ---------------------------------

let md5 (str : string) =
    str
    |> Encoding.UTF8.GetBytes
    |> MD5.Create().ComputeHash
    |> Array.map (fun b -> b.ToString "x2")
    |> String.concat ""

let renderXmlNodes (nodes : XmlNode list) =
    nodes
    |> List.map renderXmlString
    |> String.Concat

let svg (body : string) =
    setHttpHeader "Content-Type" "image/svg+xml"
    >=> setHttpHeader "Cache-Control" "no-cache"
    >=> setHttpHeader "Pragma" "no-cache"
    >=> setHttpHeader "Expires" "-1"
    >=> setHttpHeader "ETag" (md5 body)
    >=> setBodyAsString body

let notFound msg = setStatusCode 404 >=> text msg

let packageHandler getPackageFunc slug =
    fun (ctx : HttpContext) ->
        async {
            let preRelease =
                match ctx.TryGetQueryStringValue "includePreReleases" with
                | Some value -> bool.Parse value                
                | None       -> false
            let! package = getPackageFunc slug preRelease
            return!
                match package with
                | Some pkg ->
                    pkg
                    |> PackageModel.FromPackage
                    |> Views.packageView
                    |> renderXmlNodes
                    |> svg
                | None -> notFound "Package not found"
                <| ctx
        }

let nugetHandler = packageHandler NuGet.getPackageAsync
let mygetHandler = packageHandler MyGet.getPackageAsync

let webApp = 
    choose [
        GET >=>
            choose [
                route "/"             >=> htmlFile "/pages/index.html"
                route "/tests"        >=> htmlFile "/pages/tests.html"
                route "/ping"         >=> text "pong"
                routef "/nuget/%s"    nugetHandler
                routef "/myget/%s/%s" mygetHandler
            ]
        notFound "Not Found" ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) (ctx : HttpContext) =
    logger.LogError(EventId(0), ex, "An unhandled exception has occurred while executing the request.")
    ctx |> (clearResponse >=> setStatusCode 500 >=> text ex.Message)

// ---------------------------------
// Config and Main
// ---------------------------------

let configureApp (app : IApplicationBuilder) = 
    app.UseGiraffeErrorHandler(errorHandler)
    app.UseGiraffe(webApp)

let configureLogging (loggerFactory : ILoggerFactory) =
    loggerFactory.AddConsole(LogLevel.Error).AddDebug() |> ignore

[<EntryPoint>]
let main argv =
    WebHostBuilder()
        .UseKestrel()
        .UseContentRoot(Directory.GetCurrentDirectory())
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureLogging(Action<ILoggerFactory> configureLogging)
        .Build()
        .Run()
    0