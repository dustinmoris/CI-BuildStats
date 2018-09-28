module BuildStats.Web

open System
open System.Text
open System.Net.Http
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.HttpOverrides
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
        let loggerFactory = ctx.GetService<ILoggerFactory>()
        let logger = loggerFactory.CreateLogger "BuildStats.ApiSecretHandler"
        logger.LogWarning (sprintf "Unauthorized request to '%s' has been blocked." (ctx.Request.Path.ToString()))
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
        (Public (TimeSpan.FromSeconds 60.0))
        (Some "Accept-Encoding")
        (Some [| "includePreReleases"; "includeBuildsFromPullRequest"; "buildCount"; "showStats"; "authToken" |])
    >=> setHttpHeader "Content-Type" "image/svg+xml"
    >=> setBodyFromString body

let notFound msg = setStatusCode 404 >=> text msg

let packageHandler getPackageFunc slug =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let httpClientFactory = ctx.GetService<IHttpClientFactory>()
            let httpClient = httpClientFactory.CreateClient(HttpClientConfig.defaultClientName)

            let preRelease =
                match ctx.TryGetQueryStringValue "includePreReleases" with
                | Some value -> bool.Parse value
                | None       -> false
            let! package = getPackageFunc httpClient slug preRelease
            return!
                match package with
                | Some pkg ->
                    pkg
                    |> PackageModel.FromPackage
                    |> SVGs.packageSVG
                    |> renderXmlNodes
                    |> cachedSvg
                | None -> notFound "Package not found"
                <|| (next, ctx)
        }

let nugetHandler           = packageHandler NuGet.getPackageAsync
let mygetOfficialHandler   = packageHandler MyGet.getPackageFromOfficialFeedAsync
let mygetEnterpriseHandler = packageHandler MyGet.getPackageFromEnterpriseFeedAsync

let getBuildHistory (getBuildsFunc) slug =
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

            let httpClientFactory = ctx.GetService<IHttpClientFactory>()
            let httpClient = httpClientFactory.CreateClient(HttpClientConfig.defaultClientName)

            let! builds = getBuildsFunc httpClient authToken slug buildCount branch includePullRequests
            return!
                builds
                |> BuildHistoryModel.FromBuilds showStats
                |> SVGs.buildHistorySVG
                |> renderXmlNode
                |> cachedSvg
                <|| (next, ctx)
        }

let appVeyorHandler = getBuildHistory AppVeyor.getBuilds
let azureHandler    = getBuildHistory AzurePipelines.getBuilds
let circleCiHandler = getBuildHistory CircleCI.getBuilds
let travisCiHandler = getBuildHistory (TravisCI.getBuilds false)

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
        GET >=>
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
                routef "/nuget/%s"    nugetHandler
                routef "/myget/%s/%s/%s" mygetEnterpriseHandler
                routef "/myget/%s/%s" mygetOfficialHandler
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
       .UseCloudflareFirewall(true)
       .UseResponseCaching()
       .UseGiraffe(webApp)

let configureServices (services : IServiceCollection) =
    services
        .AddResponseCaching()
        .AddGiraffe()
        .AddFirewall()
        .AddHttpClient(
            HttpClientConfig.defaultClientName,
            fun client ->
                client.DefaultRequestHeaders.Accept.Add(Headers.MediaTypeWithQualityHeaderValue("application/json"))
            )
            .SetHandlerLifetime(TimeSpan.FromHours 1.0)
            .AddPolicyHandler(HttpClientConfig.tooManyRequestsPolicy)
            |> ignore