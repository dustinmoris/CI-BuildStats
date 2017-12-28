module BuildStats.App

open System
open System.IO
open System.Text
open System.Security.Cryptography
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Giraffe
open Giraffe.GiraffeViewEngine
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
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
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
                <|| (next, ctx)
        }

let nugetHandler = packageHandler NuGet.getPackageAsync
let mygetHandler = packageHandler MyGet.getPackageAsync

let getBuildHistory (getBuildsFunc) (account, project) =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
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
                <|| (next, ctx)
        }

let appVeyorHandler = getBuildHistory AppVeyor.getBuilds
let travisCiHandler = getBuildHistory TravisCI.getBuilds
let circleCiHandler = getBuildHistory CircleCI.getBuilds

let webApp =
    choose [
        GET >=>
            choose [
                route "/"             >=> htmlFile "pages/index.html"
                route "/tests"        >=> htmlFile "pages/tests.html"
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

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(EventId(0), ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureApp (app : IApplicationBuilder) =
    app.UseGiraffeErrorHandler(errorHandler)
       .UseGiraffe(webApp)

let configureLogging (builder : ILoggingBuilder) =
    let filter (l : LogLevel) = l.Equals LogLevel.Error
    builder.AddFilter(filter).AddConsole().AddDebug() |> ignore

[<EntryPoint>]
let main _ =
    WebHostBuilder()
        .UseKestrel()
        .UseContentRoot(Directory.GetCurrentDirectory())
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureLogging(configureLogging)
        .Build()
        .Run()
    0