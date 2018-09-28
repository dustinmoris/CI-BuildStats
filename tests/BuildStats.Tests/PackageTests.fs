module BuildStats.Tests.PackageTests

open System.Net.Http
open Xunit
open BuildStats.PackageServices

/// -------------------------------------
/// Helper functions
/// -------------------------------------

let httpClient = new HttpClient()
httpClient.DefaultRequestHeaders.Accept.Add(Headers.MediaTypeWithQualityHeaderValue("application/json"))

let runTask task =
    task
    |> Async.AwaitTask
    |> Async.RunSynchronously

let shouldEqual expected actual =
    if expected <> actual then sprintf "Expected: %s, Actual: %s" expected actual |> failwith

let shouldBeGreaterThan expected actual =
    if actual <= expected then sprintf "%i should have been greater than %i" expected actual |> failwith

let shouldBeTrue actual =
    if not actual then failwith "Value should have been true."
    ()

/// -------------------------------------
/// NuGet tests
/// -------------------------------------

[<Fact>]
let ``Lanem returns correct result``() =
    let package = NuGet.getPackageAsync httpClient "Lanem" false |> runTask

    package.Value.Name        |> shouldEqual "Lanem"
    package.Value.Downloads   |> shouldBeGreaterThan 800

[<Fact>]
let ``Guardo returns correct result``() =
    let package = NuGet.getPackageAsync httpClient "Guardo" false |> runTask

    package.Value.Name        |> shouldEqual "Guardo"
    package.Value.Downloads   |> shouldBeGreaterThan 49

[<Fact>]
let ``Moq returns correct result``() =
    let package = NuGet.getPackageAsync httpClient "Moq" false |> runTask

    package.Value.Name        |> shouldEqual "Moq"
    package.Value.Downloads   |> shouldBeGreaterThan 1874730

[<Fact>]
let ``NUnit returns correct result``() =
    let package = NuGet.getPackageAsync httpClient "NUnit" false |> runTask

    package.Value.Name        |> shouldEqual "NUnit"
    package.Value.Downloads   |> shouldBeGreaterThan 1283920

[<Fact>]
let ``NSubstitute returns correct result``() =
    let package = NuGet.getPackageAsync httpClient "NSubstitute" false |> runTask

    package.Value.Name        |> shouldEqual "NSubstitute"
    package.Value.Downloads   |> shouldBeGreaterThan 661870

[<Fact>]
let ``jQuery returns correct result``() =
    let package = NuGet.getPackageAsync httpClient "jQuery" false |> runTask

    package.Value.Name        |> shouldEqual "jQuery"
    package.Value.Downloads   |> shouldBeGreaterThan 4233340

[<Fact>]
let ``Microsoft.AspNet.Mvc returns correct result``() =
    let package = NuGet.getPackageAsync httpClient "Microsoft.AspNet.Mvc" false |> runTask

    package.Value.Name        |> shouldEqual "Microsoft.AspNet.Mvc"
    package.Value.Downloads   |> shouldBeGreaterThan 2956200

[<Fact>]
let ``EntityFramework returns correct result``() =
    let package = NuGet.getPackageAsync httpClient "EntityFramework" false |> runTask

    package.Value.Name        |> shouldEqual "EntityFramework"
    package.Value.Downloads   |> shouldBeGreaterThan 4496670

[<Fact>]
let ``NServiceBus.PostgreSQL PreRelease package returns correct result``() =
    let package = NuGet.getPackageAsync httpClient "NServiceBus.PostgreSQL" true |> runTask

    package.Value.Name        |> shouldEqual "NServiceBus.PostgreSQL"
    package.Value.Downloads   |> shouldBeGreaterThan 550

[<Fact>]
let ``Newtonsoft.Json returns correct result``() =
    let package = NuGet.getPackageAsync httpClient "Newtonsoft.Json" false |> runTask

    package.Value.Name        |> shouldEqual "Newtonsoft.Json"
    package.Value.Downloads   |> shouldBeGreaterThan 4998550

[<Fact>]
let ``Paket returns correct result``() =
    let package = NuGet.getPackageAsync httpClient "Paket" false |> runTask

    package.Value.Name        |> shouldEqual "Paket"
    package.Value.Downloads   |> shouldBeGreaterThan 127890

[<Fact>]
let ``Package written in lowercase returns correct result``() =
    let package = NuGet.getPackageAsync httpClient "newtonsoft.json" false |> runTask

    package.Value.Name        |> shouldEqual "Newtonsoft.Json"
    package.Value.Downloads   |> shouldBeGreaterThan 4998550

[<Fact>]
let ``Non existing package returns none``() =
    let package = NuGet.getPackageAsync httpClient "myPackage.which.does.not.exist" false |> runTask

    package.IsNone |> shouldBeTrue

/// -------------------------------------
/// MyGet tests
/// -------------------------------------

[<Fact>]
let ``NEventSocket returns correct result``() =
    let package = MyGet.getPackageFromOfficialFeedAsync httpClient ("neventsocket-prerelease", "NEventSocket") false |> runTask

    package.Value.Name        |> shouldEqual "NEventSocket"
    package.Value.Downloads   |> shouldBeGreaterThan 4

[<Fact>]
let ``MyGet Package written in lowercase returns correct result``() =
    let package = MyGet.getPackageFromOfficialFeedAsync httpClient ("neventsocket-prerelease", "neventsocket") false |> runTask

    package.Value.Name        |> shouldEqual "NEventSocket"
    package.Value.Downloads   |> shouldBeGreaterThan 4

[<Fact>]
let ``Non existing MyGet package returns none``() =
    let package = MyGet.getPackageFromOfficialFeedAsync httpClient ("not-found", "myPackage.which.does.not.exist") false |> runTask

    package.IsNone |> shouldBeTrue

/// -------------------------------------
/// MyGet Enterprise tests
/// -------------------------------------

[<Fact>]
let ``Microsoft.Bot.Builder returns correct result``() =
    let package =
        MyGet.getPackageFromEnterpriseFeedAsync
            httpClient
            ("botbuilder", "botbuilder-v4-dotnet-daily", "Microsoft.Bot.Builder")
            false
        |> runTask

    package.Value.Name        |> shouldEqual "Microsoft.Bot.Builder"