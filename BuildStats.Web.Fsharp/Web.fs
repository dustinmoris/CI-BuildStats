namespace BuildStats.Web.Fsharp

open System.Threading.Tasks
open Owin
open Nancy
open Nancy.TinyIoc
open BuildStats.Core.Fsharp

type BuildStatsModule(nugetClient : INuGetClient) as this =
    inherit NancyModule()

    do this.Get.["/"] <- fun _ -> 
        "Hello World"
        |> box

    do this.Get.["/nuget/{packageName}", true] <- fun ctx ct ->
        async {
            let! packageInfo = nugetClient.GetPackageInfo "Lanem" false
            match packageInfo with
            | Some packageInfo  -> return packageInfo.Version :> obj
            | None              -> return "test" :> obj
        } |> Async.StartAsTask

type Bootstrapper() =
    inherit DefaultNancyBootstrapper()

    override this.ConfigureApplicationContainer(container : TinyIoCContainer) =
        base.ConfigureApplicationContainer(container)

        container.Register<INuGetClient, NuGetClient>() |> ignore

type Startup() =
    member this.Configuration (app : IAppBuilder) =
        app.UseNancy()
        |> ignore