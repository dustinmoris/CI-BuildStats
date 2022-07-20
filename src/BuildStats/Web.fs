namespace BuildStats
open System.Threading.Tasks

[<RequireQualifiedAccess>]
module HttpHandlers =
    open System
    open Microsoft.Extensions.Logging
    open Microsoft.AspNetCore.Http
    open Giraffe
    open Giraffe.ViewEngine

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
        | Some v -> Env.apiSecret.Equals v
        | None   ->
            match ctx.TryGetQueryStringValue "apiSecret" with
            | Some v -> Env.apiSecret.Equals v
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
                "branch"
                "authToken"
                "vWidth"
                "dWidth"
            |])
        >=> setHttpHeader "Content-Type" "image/svg+xml"
        >=> setBodyFromString body

    let notFound msg = setStatusCode 404 >=> text msg

    let private package logoSvg getPackageFunc slug =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let httpClient = ctx.GetService<PackageHttpClient>()

                let preRelease =
                    match ctx.TryGetQueryStringValue "includePreReleases" with
                    | Some value -> Parse.boolOrDefault false value
                    | None       -> false
                let versionWidth =
                    match ctx.TryGetQueryStringValue "vWidth" with
                    | Some value ->
                        match Int32.TryParse value with
                        | true, v  -> Some v
                        | false, _ -> None
                    | None       -> None
                let downloadsWidth =
                    match ctx.TryGetQueryStringValue "dWidth" with
                    | Some value ->
                        match Int32.TryParse value with
                        | true, v  -> Some v
                        | false, _ -> None
                    | None       -> None

                let packageVersion =
                    ctx.TryGetQueryStringValue "packageVersion"

                let! package = (getPackageFunc httpClient slug preRelease packageVersion) : Task<_>
                return!
                    match package with
                    | Some pkg ->
                        pkg
                        |> PackageModel.FromPackage versionWidth downloadsWidth
                        |> SVGs.package logoSvg
                        |> RenderView.AsString.xmlNodes
                        |> cachedSvg
                    | None -> notFound "Package not found"
                    <|| (next, ctx)
            }

    let crate           = package SVGs.rust Crate.getPackageAsync
    let nuget           = package SVGs.nuget NuGet.getPackageAsync
    let mygetOfficial   = package SVGs.nuget MyGet.getPackageFromOfficialFeedAsync
    let mygetEnterprise = package SVGs.nuget MyGet.getPackageFromEnterpriseFeedAsync

    let private getBuildHistory getBuildsFunc slug =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let includePullRequests =
                    match ctx.TryGetQueryStringValue "includeBuildsFromPullRequest" with
                    | Some x -> Parse.boolOrDefault true x
                    | None   -> true
                let buildCount =
                    match ctx.TryGetQueryStringValue "buildCount" with
                    | Some x -> Parse.intOrDefault 25 x
                    | None   -> 25
                let showStats =
                    match ctx.TryGetQueryStringValue "showStats" with
                    | Some x -> Parse.boolOrDefault true x
                    | None   -> true

                let branch    = ctx.TryGetQueryStringValue "branch"
                let authToken = ctx.TryGetQueryStringValue "authToken"
                let! builds   =
                    (getBuildsFunc authToken slug buildCount branch includePullRequests) : Task<_>
                return!
                    builds
                    |> BuildHistoryModel.FromBuilds showStats
                    |> SVGs.buildHistorySVG
                    |> RenderView.AsString.xmlNode
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

    let github slug =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let client = ctx.GetService<GitHubActionsHttpClient>()
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
                let cipherText = AES.encryptToUrlEncodedString Env.cryptoKey plainText
                return! ctx.WriteTextAsync (sprintf "Encrypted auth token: %s" cipherText)
            }

    let genericError (ex : Exception) (logger : ILogger) =
        logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
        clearResponse >=> setStatusCode 500 >=> text ex.Message

module WebApp =
    open Giraffe
    open Giraffe.ViewEngine
    open Giraffe.EndpointRouting

    let endpoints : Endpoint list =
        [
            GET_HEAD [
                // Assets
                route Views.cssPath (HttpHandlers.css Views.minifiedCss)

                // HTML Views
                route "/"       (htmlView Views.indexView)
                route "/create" (htmlView Views.createApiTokenView)

                // Protected
                route "/tests"
                    (HttpHandlers.requiresApiSecret
                    >=> htmlView Views.visualTestsView)

                route "/chars"
                    (HttpHandlers.requiresApiSecret
                    >=> (SVGs.measureCharsSVG |> RenderView.AsString.xmlNode |> HttpHandlers.cachedSvg))

                // Health status
                route "/ping"    (text "pong")
                route "/version" (json {| version = Env.appVersion |})
                if not Env.isProduction then route "/error" (warbler (fun _ -> json(1/0)))

                // SVG endpoints
                routef "/crate/%s"       HttpHandlers.crate
                routef "/nuget/%s"       HttpHandlers.nuget
                routef "/myget/%s/%s/%s" HttpHandlers.mygetEnterprise
                routef "/myget/%s/%s"    HttpHandlers.mygetOfficial
                routef "/appveyor/chart/%s/%s" HttpHandlers.appVeyor
                routef "/travisci/chart/%s/%s" HttpHandlers.travisCi
                routef "/circleci/chart/%s/%s" HttpHandlers.circleCi
                routef "/azurepipelines/chart/%s/%s/%i" HttpHandlers.azure
                routef "/github/chart/%s/%s" HttpHandlers.github
            ]
            POST [ route "/create" HttpHandlers.encryptAuthToken ]
        ]
