module BuildStats.App

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Serilog
open Serilog.Events
open Giraffe
open BuildStats.Common

[<EntryPoint>]
let main args =
    let parseLogLevel =
        function
        | "verbose" -> LogEventLevel.Verbose
        | "debug"   -> LogEventLevel.Debug
        | "info"    -> LogEventLevel.Information
        | "warning" -> LogEventLevel.Warning
        | "error"   -> LogEventLevel.Error
        | "fatal"   -> LogEventLevel.Fatal
        | _         -> LogEventLevel.Warning

    let logLevelConsole =
        match isNotNull args && args.Length > 0 with
        | true  -> args.[0]
        | false -> Config.logLevelConsole
        |> parseLogLevel

    Log.Logger <-
        (LoggerConfiguration())
            .MinimumLevel.Information()
            .Enrich.WithProperty("Environment", Config.environmentName)
            .Enrich.WithProperty("Application", "CI-BuildStats")
            .WriteTo.Console(logLevelConsole)
            .CreateLogger()

    try
        try
            Log.Information "Starting BuildStats.info..."

            Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(
                    fun webHostBuilder ->
                        webHostBuilder
                            .UseSentry(
                                fun sentry ->
                                      sentry.Debug            <- false
                                      sentry.Environment      <- Config.environmentName
                                      sentry.Release          <- Config.version
                                      sentry.AttachStacktrace <- true
                                      sentry.Dsn              <- Config.sentryDsn)
                            .ConfigureKestrel(
                                fun k -> k.AddServerHeader <- false)
                            .UseContentRoot(Directory.GetCurrentDirectory())
                            .Configure(Web.configureApp)
                            .ConfigureServices(Web.configureServices)
                            |> ignore)
                .Build()
                .Run()
            0
        with ex ->
            Log.Fatal(ex, "Host terminated unexpectedly.")
            1
    finally
        Log.CloseAndFlush()