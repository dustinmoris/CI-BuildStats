namespace BuildStats

[<RequireQualifiedAccess>]
module Environment =
    open System
    open System.Diagnostics
    open Logfella

    [<RequireQualifiedAccess>]
    module private Keys =
        let APP_NAME = "APP_NAME"
        let ENV_NAME = "ASPNETCORE_ENVIRONMENT"
        let LOG_LEVEL = "LOG_LEVEL"
        let SENTRY_DSN = "SENTRY_DSN"
        let DOMAIN_NAME = "DOMAIN_NAME"
        let CRYPTO_KEY = "CRYPTO_KEY"
        let API_SECRET = "API_SECRET"
        let ENABLE_REQUEST_LOGGING = "ENABLE_REQUEST_LOGGING"
        let ENABLE_ERROR_ENDPOINT = "ENABLE_ERROR_ENDPOINT"
        let PROXY_COUNT = "PROXY_COUNT"
        let KNOWN_PROXIES = "KNOWN_PROXIES"
        let KNOWN_PROXY_NETWORKS = "KNOWN_PROXY_NETWORKS"

    let appName =
        Config.environmentVarOrDefault
            Keys.APP_NAME
            "BuildStats"

    let appVersion =
        System.Reflection.Assembly.GetExecutingAssembly().Location
        |> FileVersionInfo.GetVersionInfo
        |> fun v-> v.ProductVersion

    let name =
        Config.environmentVarOrDefault
            Keys.ENV_NAME
            "Unknown"

    let isProduction =
        name.Equals(
            "Production",
            StringComparison.OrdinalIgnoreCase)

    let logLevel =
        Config.environmentVarOrDefault
            Keys.LOG_LEVEL
            "info"

    let logSeverity =
        logLevel.ParseSeverity()

    let sentryDsn = Config.environmentVar Keys.SENTRY_DSN

    let domainName =
        Config.environmentVarOrDefault
            Keys.DOMAIN_NAME
            "buildstats.info"

    let baseUrl =
        match isProduction with
        | true  -> sprintf "https://%s" domainName
        | false -> "http://localhost:5000"

    let cryptoKey =
        Config.environmentVarOrDefault
            Keys.CRYPTO_KEY
            (Guid.NewGuid().ToString("N"))

    let apiSecret =
        Config.environmentVarOrDefault
            Keys.API_SECRET
            (Guid.NewGuid().ToString("N"))

    let enableRequestLogging =
        Config.InvariantCulture.typedEnvironmentVarOrDefault<bool>
            Keys.ENABLE_REQUEST_LOGGING
            false

    let enableErrorEndpoint =
        Config.InvariantCulture.typedEnvironmentVarOrDefault<bool>
            Keys.ENABLE_ERROR_ENDPOINT
            false

    let proxyCount =
        Config.InvariantCulture.typedEnvironmentVarOrDefault<int>
            Keys.PROXY_COUNT
            0

    let knownProxies =
        Keys.KNOWN_PROXIES
        |> Config.environmentVarList
        |> Array.map Network.tryParseIPAddress
        |> Array.filter Option.isSome
        |> Array.map Option.get

    let knownProxyNetworks =
        Keys.KNOWN_PROXY_NETWORKS
        |> Config.environmentVarList
        |> Array.map Network.tryParseNetworkAddress
        |> Array.filter Option.isSome
        |> Array.map Option.get

    let summary =
        dict [
            "App", dict [
                "App", appName
                "Version", appVersion
            ]
            "Logging", dict [
                "Environment", name
                "Log Level", logLevel
                "Sentry DSN", sentryDsn.ToSecret()
            ]
            "URLs", dict [
                "Domain", domainName
                "Base URL", baseUrl
            ]
            "Security", dict [
                "Crypto Key", cryptoKey.ToSecret()
                "API Secret", apiSecret.ToSecret()
            ]
            "Proxies", dict [
                "Proxy count", proxyCount.ToString()
                "Known proxies", knownProxies.ToPrettyString()
                "Known proxy networks", knownProxyNetworks.ToPrettyString()
            ]
            "Debugging", dict [
                "Request logging enabled", enableRequestLogging.ToString()
                "Error endpoint enabled", enableErrorEndpoint.ToString()
            ]
        ]