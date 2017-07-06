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
open Giraffe.ComputationExpressions
open BuildStats.PackageServices
open BuildStats.BuildHistoryCharts
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

let getBuildHistory (getBuildsFunc) (account, project) =
    fun (ctx : HttpContext) ->
        async {
            let includePullRequests =
                match ctx.TryGetQueryStringValue "includeBuildsFromPullRequest" with
                | Some x -> bool.Parse x
                | None   -> true
            let buildCount =
                match ctx.TryGetQueryStringValue "buildCount" with
                | Some x -> int x
                | None   -> 25
            let showStats =
                match ctx.TryGetQueryStringValue "showStats" with
                | Some x -> bool.Parse x
                | None   -> true
            let branch = ctx.TryGetQueryStringValue "branch"
            let! builds = getBuildsFunc account project buildCount branch includePullRequests
            return!
                builds
                |> BuildHistoryModel.FromBuilds showStats
                |> Views.buildHistoryView
                |> renderXmlNode
                |> svg
                <| ctx
        }

let appVeyorHandler = getBuildHistory AppVeyor.getBuilds
let travisCiHandler = getBuildHistory TravisCI.getBuilds
let circleCiHandler = getBuildHistory CircleCI.getBuilds

let webApp =
    choose [
        GET >=>
            choose [
                route "/"             >=> htmlFile "/pages/index.html"
                route "/tests"        >=> htmlFile "/pages/tests.html"
                route "/chars"        >=> (Views.measureCharsView |> renderXmlNode |> svg)
                route "/ping"         >=> text "pong"
                routef "/nuget/%s"    nugetHandler
                routef "/myget/%s/%s" mygetHandler
                routef "/appveyor/chart/%s/%s" appVeyorHandler
                routef "/travisci/chart/%s/%s" travisCiHandler
                routef "/circleci/chart/%s/%s" circleCiHandler
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