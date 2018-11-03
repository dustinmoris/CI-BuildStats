module BuildStats.App

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Serilog
open Serilog.Events
open Serilog.Sinks.Elasticsearch
open Elasticsearch.Net
open Giraffe
open BuildStats.Common

[<EntryPoint>]
let main args =
    let logLevel =
        match isNotNull args && args.Length > 0 with
        | true  -> args.[0]
        | false -> Config.logLevel
        |> (function
            | "verbose" -> LogEventLevel.Verbose
            | "debug"   -> LogEventLevel.Debug
            | "info"    -> LogEventLevel.Information
            | "warning" -> LogEventLevel.Warning
            | "error"   -> LogEventLevel.Error
            | "fatal"   -> LogEventLevel.Fatal
            | _         -> LogEventLevel.Warning)

    let elasticOptions =
        new ElasticsearchSinkOptions(
            new Uri(Config.elasticUrl),
            AutoRegisterTemplate = true,
            AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,
            ModifyConnectionSettings =
                fun (config : ConnectionConfiguration) ->
                    config.BasicAuthentication(
                        Config.elasticUser,
                        Config.elasticPassword))

    let loggerConfig =
        (new LoggerConfiguration())
            .MinimumLevel.Is(logLevel)
            .Enrich.WithProperty("Environment", Config.environmentName)
            .Enrich.WithProperty("Application", "CI-BuildStats")

    let loggerConfig' =
        match Config.isProduction with
        | true  -> loggerConfig.WriteTo.Elasticsearch(elasticOptions)
        | false -> loggerConfig.WriteTo.Console()

    Log.Logger <- loggerConfig'.CreateLogger()

    try
        try
            Log.Information "Starting BuildStats.info..."

            if not Config.isProduction then
                Log.Information (sprintf "API Secret: %s" Config.apiSecret)

            WebHostBuilder()
                .UseSerilog()
                .UseKestrel(fun k -> k.AddServerHeader <- false)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .Configure(Action<IApplicationBuilder> Web.configureApp)
                .ConfigureServices(Web.configureServices)
                .Build()
                .Run()
            0
        with ex ->
            Log.Fatal(ex, "Host terminated unexpectedly.")
            1
    finally
        Log.CloseAndFlush()