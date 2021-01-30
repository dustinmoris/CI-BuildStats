namespace BuildStats

module Program =
    open System
    open System.IO
    open System.Net.Http
    open System.Collections.Generic
    open Microsoft.AspNetCore.Builder
    open Microsoft.AspNetCore.Hosting
    open Microsoft.AspNetCore.Http
    open Microsoft.Extensions.Hosting
    open Microsoft.Extensions.Caching.Memory
    open Microsoft.Extensions.DependencyInjection
    open Giraffe
    open Giraffe.EndpointRouting
    open Logfella
    open Logfella.LogWriters
    open Logfella.AspNetCore
    open Logfella.Adapters

    let private muteFilter =
        Func<Severity, string, IDictionary<string, obj>, exn, bool>(
            fun severity msg data ex ->
                msg.StartsWith "The response could not be cached for this request")

    let private createLogWriter (ctx : HttpContext option) =
        match Env.isProduction with
        | false -> ConsoleLogWriter(Env.logSeverity).AsLogWriter()
        | true  ->
            let basic =
                GoogleCloudLogWriter
                    .Create(Env.logSeverity)
                    .AddServiceContext(
                        Env.appName,
                        Env.appVersion)
                    .UseGoogleCloudTimestamp()
                    .AddLabels(
                        dict [
                            "appName", Env.appName
                            "appVersion", Env.appVersion
                        ])
            let final =
                match ctx with
                | None     -> basic
                | Some ctx ->
                    basic
                        .AddHttpContext(ctx)
                        .AddCorrelationId(Guid.NewGuid().ToString("N"))
            Mute.When(muteFilter)
                .Otherwise(final)

    let private createReqLogWriter =
        Func<HttpContext, ILogWriter>(Some >> createLogWriter)

    let private toggleRequestLogging =
        Action<RequestLoggingOptions>(
            fun x -> x.IsEnabled <- Env.enableRequestLogging)

    let createResilientHttpClient (svc : IServiceProvider) =
        FallbackHttpClient(
            CircuitBreakerHttpClient(
                RetryHttpClient(
                    BaseHttpClient(svc.GetService<IHttpClientFactory>()),
                    maxRetries = 1),
                minBreakDuration = TimeSpan.FromSeconds 1.0))

    let configureServices (services : IServiceCollection) =
        services
            .AddMemoryCache()
            .AddHttpClient()
            .AddSingleton<TravisCIHttpClient>(
                fun svc -> TravisCIHttpClient(createResilientHttpClient svc, svc.GetService<IMemoryCache>()))
            .AddSingleton<AppVeyorHttpClient>(createResilientHttpClient >> AppVeyorHttpClient)
            .AddSingleton<CircleCIHttpClient>(createResilientHttpClient >> CircleCIHttpClient)
            .AddSingleton<AzurePipelinesHttpClient>(createResilientHttpClient >> AzurePipelinesHttpClient)
            .AddSingleton<GitHubActionsHttpClient>(createResilientHttpClient >>  GitHubActionsHttpClient)
            .AddTransient<PackageHttpClient>(
                fun svc ->
                    PackageHttpClient(
                        FallbackHttpClient(
                            BaseHttpClient(
                                svc.GetService<IHttpClientFactory>())))
            )
            .AddProxies(
                Env.proxyCount,
                Env.knownProxyNetworks,
                Env.knownProxies)
            .AddResponseCaching()
            .AddRouting()
            .AddGiraffe()
            |> ignore

    let configureApp (app : IApplicationBuilder) =
        app.UseGiraffeErrorHandler(HttpHandlers.genericError)
           .UseRequestScopedLogWriter(createReqLogWriter)
           .UseGiraffeErrorHandler(HttpHandlers.genericError)
           .UseRequestLogging(toggleRequestLogging)
           .UseForwardedHeaders()
           .UseHttpsRedirection(Env.forceHttps, Env.domainName)
           .UseResponseCaching()
           .UseRouting()
           .UseGiraffe(WebApp.endpoints)
           .UseGiraffe(HttpHandlers.notFound "Not Found")

    [<EntryPoint>]
    let main args =
        try
            Log.SetDefaultLogWriter(createLogWriter None)
            Logging.outputEnvironmentSummary Env.summary

            Host.CreateDefaultBuilder(args)
                .UseLogfella()
                .ConfigureWebHost(
                    fun webHostBuilder ->
                        webHostBuilder
                            .ConfigureSentry(
                                Env.sentryDsn,
                                Env.name,
                                Env.appVersion)
                            .UseKestrel(
                                fun k -> k.AddServerHeader <- false)
                            .UseContentRoot(Directory.GetCurrentDirectory())
                            .Configure(configureApp)
                            .ConfigureServices(configureServices)
                            |> ignore)
                .Build()
                .Run()
            0
        with ex ->
            Log.Emergency("Host terminated unexpectedly.", ex)
            1