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

    do this.Get.["/some", true] <- fun (ctx, ct) -> Task.FromResult("")

//    do this.Get.["/nuget/{packageName}", true, fun (ctx, ct) -> 
//        async {
//            ""
//        }]
        

type Startup() =
    member this.Configuration (app : IAppBuilder) =
        app.UseNancy()
        |> ignore