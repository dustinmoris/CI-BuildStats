namespace BuildStats.Web.Fsharp

open Owin
open Nancy
open BuildStats.Core.Fsharp
open System.Threading.Tasks

type BuildStatsModule(nugetClient : INuGetClient) as this =
    inherit NancyModule()

    do this.Get.["/"] <- fun _ -> 
        "Hello World"
        |> box        

type Startup() =
    member this.Configuration (app : IAppBuilder) =
        app.UseNancy()
        |> ignore