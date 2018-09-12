module BuildStats.App

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Serilog

[<EntryPoint>]
let main _ =
    Log.Logger <-
        (new LoggerConfiguration())
            .MinimumLevel.Warning()
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