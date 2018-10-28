module BuildStats.Web

open System
open System.Text
open System.Threading.Tasks
open System.Net.Http
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.HttpOverrides
open Microsoft.Extensions.Caching.Memory
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open Giraffe.GiraffeViewEngine
open Firewall
open BuildStats.Common
open BuildStats.PackageServices
open BuildStats.BuildHistoryCharts
open BuildStats.ViewModels
open BuildStats.HttpClients

// ---------------------------------
// Web app
// ---------------------------------

let devApiSecret = Guid.NewGuid().ToString("n").Substring(0, 10)

let apiSecret =
        Environment.GetEnvironmentVariable "API_SECRET"
        |> Str.toOption
        |> function
            | Some v -> v
            | None   -> devApiSecret

let accessForbidden =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let logger = ctx.GetLogger()
        logger.LogWarning(
            "Unauthorized request to '{url}' has been blocked.",
            ctx.GetRequestUrl())
        RequestErrors.FORBIDDEN
            "Access denied. Please provide a valid API secret in order to access this resource." next ctx

let finish = Some >> Task.FromResult

let requiresApiSecret =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        (match ctx.TryGetQueryStringValue "apiSecret" with
        | Some secretFromQuery ->
            match apiSecret.Equals secretFromQuery with
            | true  -> next
            | false -> accessForbidden finish
        | None      -> accessForbidden finish) ctx

let cssHandler (bundle : string) =
    setHttpHeader "Content-Type" "text/css"
    >=> setHttpHeader "Cache-Control" "public, max-age=31536000"
    >=> setHttpHeader "ETag" Views.cssHash
    >=> setBodyFromString bundle

let cachedSvg (body : string) =
    responseCaching
        (Public (TimeSpan.FromSeconds 90.0))
        (Some "Accept-Encoding")
        (Some [| "includePreReleases"; "includeBuildsFromPullRequest"; "buildCount"; "showStats"; "authToken"; "vWidth"; "dWidth" |])
    >=> setHttpHeader "Content-Type" "image/svg+xml"
    >=> setBodyFromString body

let notFound msg = setStatusCode 404 >=> text msg

let packageHandler getPackageFunc slug =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let httpClient = ctx.GetService<PackageHttpClient>()

            let preRelease =
                match ctx.TryGetQueryStringValue "includePreReleases" with
                | Some value -> bool.Parse value
                | None       -> false

            let versionWidth =
                match ctx.TryGetQueryStringValue "vWidth" with
                | Some value -> Some (Int32.Parse value)
                | None       -> None

            let downloadsWidth =
                match ctx.TryGetQueryStringValue "dWidth" with
                | Some value -> Some (Int32.Parse value)
                | None       -> None

            let! package = getPackageFunc httpClient slug preRelease
            return!
                match package with
                | Some pkg ->
                    pkg
                    |> PackageModel.FromPackage versionWidth downloadsWidth
                    |> SVGs.packageSVG
                    |> renderXmlNodes
                    |> cachedSvg
                | None -> notFound "Package not found"
                <|| (next, ctx)
        }

let nugetHandler           = packageHandler NuGet.getPackageAsync
let mygetOfficialHandler   = packageHandler MyGet.getPackageFromOfficialFeedAsync
let mygetEnterpriseHandler = packageHandler MyGet.getPackageFromEnterpriseFeedAsync

let getBuildHistory getBuildsFunc slug =
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

            let branch    = ctx.TryGetQueryStringValue "branch"
            let authToken = ctx.TryGetQueryStringValue "authToken"
            let! builds   = getBuildsFunc authToken slug buildCount branch includePullRequests
            return!
                builds
                |> BuildHistoryModel.FromBuilds showStats
                |> SVGs.buildHistorySVG
                |> renderXmlNode
                |> cachedSvg
                <|| (next, ctx)
        }

let appVeyorHandler slug =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let client = ctx.GetService<AppVeyorHttpClient>()
        getBuildHistory client.GetBuildsAsync slug next ctx

let azureHandler slug =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let client = ctx.GetService<AzurePipelinesHttpClient>()
        getBuildHistory client.GetBuildsAsync slug next ctx

let circleCiHandler slug =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let client = ctx.GetService<CircleCIHttpClient>()
        getBuildHistory client.GetBuildsAsync slug next ctx

let travisCiHandler slug =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let client = ctx.GetService<TravisCIHttpClient>()
        getBuildHistory client.GetBuildsAsync slug next ctx

let createHandler : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let plainText = (ctx.Request.Form.["plaintext"]).ToString()
            let cipherText = AES.encryptToUrlEncodedString AES.key plainText
            return! ctx.WriteTextAsync (sprintf "Encrypted auth token: %s" cipherText)
        }

let debugHandler : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let sb = new StringBuilder()

            ctx.Connection.RemoteIpAddress.ToString()
            |> sprintf "RemoteIpAddress: %s"
            |> sb.AppendLine |> ignore

            ctx.Connection.RemotePort
            |> sprintf "RemotePort: %i"
            |> sb.AppendLine |> ignore

            ctx.Request.Headers.Keys
            |> Seq.iter (fun k ->
                ctx.Request.Headers.[k].ToString()
                |> sprintf "%s: %s" k
                |> sb.AppendLine
                |> ignore)

            return! ctx.WriteTextAsync (sb.ToString())
        }

let webApp =
    choose [
        choose [ GET; HEAD ] >=>
            choose [
                // Assets
                route Views.cssPath   >=> cssHandler Views.minifiedCss

                // HTML Views
                route "/"             >=> htmlView Views.indexView
                route "/create"       >=> htmlView Views.createApiTokenView

                // Protected
                route "/debug"        >=> requiresApiSecret >=> debugHandler
                route "/tests"        >=> requiresApiSecret >=> htmlView Views.visualTestsView
                route "/chars"        >=> requiresApiSecret >=> (SVGs.measureCharsSVG |> renderXmlNode |> cachedSvg)

                // Health status
                route "/ping"         >=> text "pong"

                // SVG endpoints
                routef "/nuget/%s"       nugetHandler
                routef "/myget/%s/%s/%s" mygetEnterpriseHandler
                routef "/myget/%s/%s"    mygetOfficialHandler
                routef "/appveyor/chart/%s/%s" appVeyorHandler
                routef "/travisci/chart/%s/%s" travisCiHandler
                routef "/circleci/chart/%s/%s" circleCiHandler
                routef "/azurepipelines/chart/%s/%s/%i" azureHandler
            ]
        POST >=> route "/create" >=> createHandler
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
    let forwardedHeadersOptions =
        new ForwardedHeadersOptions(
            ForwardedHeaders = ForwardedHeaders.XForwardedFor,
            ForwardLimit = new Nullable<int>(1)
        )

    app.UseForwardedHeaders(forwardedHeadersOptions)
       .UseGiraffeErrorHandler(errorHandler)
       .UseFirewall(
            FirewallRulesEngine
                .DenyAllAccess()
                .ExceptFromCloudflare()
                .ExceptFromLocalhost())
       .UseResponseCaching()
       .UseGiraffe(webApp)

let createResilientHttpClient (svc : IServiceProvider) =
    new FallbackHttpClient(
        new CircuitBreakerHttpClient(
            new RetryHttpClient(
                new BaseHttpClient(
                    svc.GetService<IHttpClientFactory>()),
                svc.GetService<ILogger<RetryHttpClient>>(),
                maxRetries = 1),
            svc.GetService<ILogger<CircuitBreakerHttpClient>>(),
            minBreakDuration = TimeSpan.FromSeconds 1.0),
        svc.GetService<ILogger<FallbackHttpClient>>())

let configureServices (services : IServiceCollection) =
    services
        .AddMemoryCache()
        .AddHttpClient()
        .AddSingleton<TravisCIHttpClient>(
            fun svc -> new TravisCIHttpClient(createResilientHttpClient svc, svc.GetService<IMemoryCache>()))
        .AddSingleton<AppVeyorHttpClient>(
            fun svc -> new AppVeyorHttpClient(createResilientHttpClient svc))
        .AddSingleton<CircleCIHttpClient>(
            fun svc -> new CircleCIHttpClient(createResilientHttpClient svc))
        .AddSingleton<AzurePipelinesHttpClient>(
            fun svc -> new AzurePipelinesHttpClient(createResilientHttpClient svc))
        .AddTransient<PackageHttpClient>(
            fun svc ->
                new PackageHttpClient(
                    new FallbackHttpClient(
                        new BaseHttpClient(
                            svc.GetService<IHttpClientFactory>()),
                        svc.GetService<ILogger<FallbackHttpClient>>()))
        )
        .AddResponseCaching()
        .AddGiraffe()
        |> ignore