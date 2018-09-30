module BuildStats.App

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Serilog
open Giraffe

[<EntryPoint>]
let main args =
    let logLevel =
        match isNotNull args && args.Length > 0 with
        | false -> Events.LogEventLevel.Warning
        | true  ->
            match args.[0] with
            | "verbose" -> Events.LogEventLevel.Verbose
            | "debug"   -> Events.LogEventLevel.Debug
            | "info"    -> Events.LogEventLevel.Information
            | "warning" -> Events.LogEventLevel.Warning
            | "error"   -> Events.LogEventLevel.Error
            | "fatal"   -> Events.LogEventLevel.Fatal
            | _         -> Events.LogEventLevel.Warning

    Log.Logger <-
        (new LoggerConfiguration())
            .MinimumLevel.Is(logLevel)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger()
    try
        try
            Log.Information "Starting BuildStats.info..."
            Log.Information (sprintf "API Secret: %s" Web.apiSecret)

            WebHostBuilder()
                .UseSerilog()
                .UseKestrel()
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