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

    do this.Get.["/nuget/{packageName}?includePreReleases={includePreReleases}", true] <- fun ctx ct ->
        async {
            let packageName = this.GetParameter ctx "packageName"
            let includePreReleases = this.GetParameter ctx "includePreReleases"
            let! package = nugetClient.GetPackageAsync packageName includePreReleases
            match package with
            | Some package  -> return package.Version :> obj
            | None          -> return "test" :> obj
        } |> Async.StartAsTask

    member this.GetParameter<'T> (ctx : obj) (param : string) : 'T =
        ((ctx :?> Nancy.DynamicDictionary).[param] 
        :?> Nancy.DynamicDictionaryValue).Value :?> 'T

type Bootstrapper() =
    inherit DefaultNancyBootstrapper()

    override this.ConfigureApplicationContainer(container : TinyIoCContainer) =
        base.ConfigureApplicationContainer(container)

        container.Register<INuGetClient, NuGetClient>() |> ignore

type Startup() =
    member this.Configuration (app : IAppBuilder) =
        app.UseNancy()
        |> ignore