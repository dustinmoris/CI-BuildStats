namespace BuildStats

module Program =
    open System
    open System.IO
    open System.Net.Http
    open System.Collections.Generic
    open Microsoft.AspNetCore.Builder
    open Microsoft.AspNetCore.Hosting
    open Microsoft.Extensions.Hosting
    open Microsoft.Extensions.Caching.Memory
    open Microsoft.Extensions.DependencyInjection
    open Giraffe
    open Logfella
    open Logfella.LogWriters
    open Logfella.AspNetCore
    open Logfella.Adapters

    let private googleCloudLogWriter =
        GoogleCloudLogWriter
            .Create(Environment.logSeverity)
            .AddServiceContext(
                Environment.appName,
                Environment.appVersion)
            .UseGoogleCloudTimestamp()
            .AddLabels(
                dict [
                    "appName", Environment.appName
                    "appVersion", Environment.appVersion
                ])

    let private muteFilter =
        Func<Severity, string, IDictionary<string, obj>, exn, bool>(
            fun severity msg data ex ->
                msg.StartsWith "The response could not be cached for this request")

    let private defaultLogWriter =
        Mute.When(muteFilter)
            .Otherwise(
                match Environment.isProduction with
                | true  -> googleCloudLogWriter.AsLogWriter()
                | false -> ConsoleLogWriter(Environment.logSeverity).AsLogWriter())

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
            .AddSingleton<AppVeyorHttpClient>(
                fun svc -> AppVeyorHttpClient(createResilientHttpClient svc))
            .AddSingleton<CircleCIHttpClient>(
                fun svc -> CircleCIHttpClient(createResilientHttpClient svc))
            .AddSingleton<AzurePipelinesHttpClient>(
                fun svc -> AzurePipelinesHttpClient(createResilientHttpClient svc))
            .AddSingleton<GitHubActionsHttpClient>(
                fun svc -> GitHubActionsHttpClient(createResilientHttpClient svc))
            .AddTransient<PackageHttpClient>(
                fun svc ->
                    PackageHttpClient(
                        FallbackHttpClient(
                            BaseHttpClient(
                                svc.GetService<IHttpClientFactory>())))
            )
            .AddProxies(
                Environment.proxyCount,
                Environment.knownProxyNetworks,
                Environment.knownProxies)
            .AddResponseCaching()
            .AddGiraffe()
            |> ignore

    let configureApp (app : IApplicationBuilder) =
        app.UseGiraffeErrorHandler(HttpHandlers.genericError)
           .UseWhen(
                (fun _ -> Environment.isProduction),
                fun x ->
                    x.UseRequestBasedLogWriter(
                        fun ctx ->
                            Mute.When(muteFilter)
                                .Otherwise(
                                    googleCloudLogWriter
                                        .AddHttpContext(ctx)
                                        .AddCorrelationId(Guid.NewGuid().ToString("N"))
                                        .AsLogWriter()))
                    |> ignore)
           .UseGiraffeErrorHandler(HttpHandlers.genericError)
           .UseRequestLogging(Environment.enableRequestLogging, false)
           .UseForwardedHeaders()
           .UseHttpsRedirection(Environment.domainName)
           .UseResponseCaching()
           .UseGiraffe(WebApp.routes)

    [<EntryPoint>]
    let main args =
        try
            Log.SetDefaultLogWriter(defaultLogWriter)
            Logging.outputEnvironmentSummary Environment.summary

            Host.CreateDefaultBuilder()
                .UseLogfella()
                .ConfigureWebHost(
                    fun webHostBuilder ->
                        webHostBuilder
                            .ConfigureSentry(
                                Environment.sentryDsn,
                                Environment.name,
                                Environment.appVersion)
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