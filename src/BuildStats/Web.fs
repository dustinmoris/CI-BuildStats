namespace BuildStats

[<RequireQualifiedAccess>]
module HttpHandlers =
    open System
    open Microsoft.Extensions.Logging
    open Microsoft.AspNetCore.Http
    open FSharp.Control.Tasks.V2.ContextInsensitive
    open Giraffe
    open Giraffe.GiraffeViewEngine

    let accessForbidden =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let logger = ctx.GetLogger()
            logger.LogWarning(
                "Unauthorized request to '{url}' has been blocked.",
                ctx.GetRequestUrl())
            RequestErrors.FORBIDDEN
                "Access denied. Please provide a valid API secret in order to access this resource." next ctx

    let private validateApiSecret (ctx : HttpContext) =
        match ctx.TryGetRequestHeader "X-API-SECRET" with
        | Some v -> Environment.apiSecret.Equals v
        | None   ->
            match ctx.TryGetQueryStringValue "apiSecret" with
            | Some v -> Environment.apiSecret.Equals v
            | None   -> false

    let requiresApiSecret = authorizeRequest validateApiSecret accessForbidden

    let css (bundle : string) =
        setHttpHeader "Content-Type" "text/css"
        >=> setHttpHeader "Cache-Control" "public, max-age=31536000"
        >=> setHttpHeader "ETag" Views.cssHash
        >=> setBodyFromString bundle

    let cachedSvg (body : string) =
        responseCaching
            (Public (TimeSpan.FromSeconds 90.0))
            (Some "Accept-Encoding")
            (Some [|
                "packageVersion"
                "includePreReleases"
                "includeBuildsFromPullRequest"
                "buildCount"
                "showStats"
                "authToken"
                "vWidth"
                "dWidth"
            |])
        >=> setHttpHeader "Content-Type" "image/svg+xml"
        >=> setBodyFromString body

    let notFound msg = setStatusCode 404 >=> text msg

    let private package getPackageFunc slug =
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

                let packageVersion =
                    ctx.TryGetQueryStringValue "packageVersion"

                let! package = getPackageFunc httpClient slug preRelease packageVersion
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

    let nuget           = package NuGet.getPackageAsync
    let mygetOfficial   = package MyGet.getPackageFromOfficialFeedAsync
    let mygetEnterprise = package MyGet.getPackageFromEnterpriseFeedAsync

    let private getBuildHistory getBuildsFunc slug =
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

    let appVeyor slug =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let client = ctx.GetService<AppVeyorHttpClient>()
            getBuildHistory client.GetBuildsAsync slug next ctx

    let azure slug =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let client = ctx.GetService<AzurePipelinesHttpClient>()
            getBuildHistory client.GetBuildsAsync slug next ctx

    let circleCi slug =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let client = ctx.GetService<CircleCIHttpClient>()
            getBuildHistory client.GetBuildsAsync slug next ctx

    let travisCi slug =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let client = ctx.GetService<TravisCIHttpClient>()
            getBuildHistory client.GetBuildsAsync slug next ctx

    let encryptAuthToken : HttpHandler =
        fun (_ : HttpFunc) (ctx : HttpContext) ->
            task {
                let plainText = (ctx.Request.Form.["plaintext"]).ToString()
                let cipherText = AES.encryptToUrlEncodedString Environment.cryptoKey plainText
                return! ctx.WriteTextAsync (sprintf "Encrypted auth token: %s" cipherText)
            }

    let genericError (ex : Exception) (logger : ILogger) =
        logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
        clearResponse >=> setStatusCode 500 >=> text ex.Message

module WebApp =
    open Giraffe
    open Giraffe.GiraffeViewEngine

    let routes : HttpHandler =
        choose [
            choose [ GET; HEAD ] >=>
                choose [
                    // Assets
                    route Views.cssPath >=> HttpHandlers.css Views.minifiedCss

                    // HTML Views
                    route "/"       >=> htmlView Views.indexView
                    route "/create" >=> htmlView Views.createApiTokenView

                    // Protected
                    route "/tests"
                    >=> HttpHandlers.requiresApiSecret
                    >=> htmlView Views.visualTestsView

                    route "/chars"
                    >=> HttpHandlers.requiresApiSecret
                    >=> (SVGs.measureCharsSVG |> renderXmlNode |> HttpHandlers.cachedSvg)

                    // Health status
                    route "/ping"    >=> text "pong"
                    route "/version" >=> json {| version = Environment.appVersion |}
                    if not Environment.isProduction then route "/error" >=> warbler (fun _ -> json(1/0))

                    // SVG endpoints
                    routef "/nuget/%s"       HttpHandlers.nuget
                    routef "/myget/%s/%s/%s" HttpHandlers.mygetEnterprise
                    routef "/myget/%s/%s"    HttpHandlers.mygetOfficial
                    routef "/appveyor/chart/%s/%s" HttpHandlers.appVeyor
                    routef "/travisci/chart/%s/%s" HttpHandlers.travisCi
                    routef "/circleci/chart/%s/%s" HttpHandlers.circleCi
                    routef "/azurepipelines/chart/%s/%s/%i" HttpHandlers.azure
                ]
            POST >=> route "/create" >=> HttpHandlers.encryptAuthToken
            HttpHandlers.notFound "Not Found" ]