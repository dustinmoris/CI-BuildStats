module BuildStats.Tests.PackageTests

open System
open Xunit
open BuildStats.PackageServices

/// -------------------------------------
/// Helper functions
/// -------------------------------------

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
    let package = NuGet.getPackageAsync "Lanem" false |> Async.RunSynchronously
        
    package.Value.Name        |> shouldEqual "Lanem"
    package.Value.Version     |> shouldEqual "2.0.0"
    package.Value.Downloads   |> shouldBeGreaterThan 800

[<Fact>]
let ``Guardo returns correct result``() =
    let package = NuGet.getPackageAsync "Guardo" false |> Async.RunSynchronously
        
    package.Value.Name        |> shouldEqual "Guardo"
    package.Value.Version     |> shouldEqual "1.0.0"
    package.Value.Downloads   |> shouldBeGreaterThan 49

[<Fact>]
let ``Moq returns correct result``() =
    let package = NuGet.getPackageAsync "Moq" false |> Async.RunSynchronously
        
    package.Value.Name        |> shouldEqual "Moq"
    package.Value.Version     |> shouldEqual "4.7.63"
    package.Value.Downloads   |> shouldBeGreaterThan 1874730

[<Fact>]
let ``NUnit returns correct result``() =
    let package = NuGet.getPackageAsync "NUnit" false |> Async.RunSynchronously
        
    package.Value.Name        |> shouldEqual "NUnit"
    package.Value.Version     |> shouldEqual "3.7.1"
    package.Value.Downloads   |> shouldBeGreaterThan 1283920

[<Fact>]
let ``NSubstitute returns correct result``() =
    let package = NuGet.getPackageAsync "NSubstitute" false |> Async.RunSynchronously
        
    package.Value.Name        |> shouldEqual "NSubstitute"
    package.Value.Version     |> shouldEqual "2.0.3"
    package.Value.Downloads   |> shouldBeGreaterThan 661870

[<Fact>]
let ``jQuery returns correct result``() =
    let package = NuGet.getPackageAsync "jQuery" false |> Async.RunSynchronously
        
    package.Value.Name        |> shouldEqual "jQuery"
    package.Value.Version     |> shouldEqual "3.1.1"
    package.Value.Downloads   |> shouldBeGreaterThan 4233340

[<Fact>]
let ``Microsoft.AspNet.Mvc returns correct result``() =
    let package = NuGet.getPackageAsync "Microsoft.AspNet.Mvc" false |> Async.RunSynchronously
        
    package.Value.Name        |> shouldEqual "Microsoft.AspNet.Mvc"
    package.Value.Version     |> shouldEqual "5.2.3"
    package.Value.Downloads   |> shouldBeGreaterThan 2956200

[<Fact>]
let ``EntityFramework returns correct result``() =
    let package = NuGet.getPackageAsync "EntityFramework" false |> Async.RunSynchronously
        
    package.Value.Name        |> shouldEqual "EntityFramework"
    package.Value.Version     |> shouldEqual "6.1.3"
    package.Value.Downloads   |> shouldBeGreaterThan 4496670

[<Fact>]
let ``NServiceBus.PostgreSQL PreRelease package returns correct result``() =
    let package = NuGet.getPackageAsync "NServiceBus.PostgreSQL" true |> Async.RunSynchronously
        
    package.Value.Name        |> shouldEqual "NServiceBus.PostgreSQL"
    package.Value.Version     |> shouldEqual "1.0.0-CI00021"
    package.Value.Downloads   |> shouldBeGreaterThan 550

[<Fact>]
let ``Newtonsoft.Json returns correct result``() =
    let package = NuGet.getPackageAsync "Newtonsoft.Json" false |> Async.RunSynchronously
        
    package.Value.Name        |> shouldEqual "Newtonsoft.Json"
    package.Value.Version     |> shouldEqual "10.0.3"
    package.Value.Downloads   |> shouldBeGreaterThan 4998550

[<Fact>]
let ``Paket returns correct result``() =
    let package = NuGet.getPackageAsync "Paket" false |> Async.RunSynchronously
        
    package.Value.Name        |> shouldEqual "Paket"
    package.Value.Version     |> shouldEqual "5.4.0"
    package.Value.Downloads   |> shouldBeGreaterThan 127890

[<Fact>]
let ``Package written in lowercase returns correct result``() =
    let package = NuGet.getPackageAsync "newtonsoft.json" false |> Async.RunSynchronously
        
    package.Value.Name        |> shouldEqual "Newtonsoft.Json"
    package.Value.Version     |> shouldEqual "10.0.3"
    package.Value.Downloads   |> shouldBeGreaterThan 4998550

[<Fact>]
let ``Non existing package returns none``() =
    let package = NuGet.getPackageAsync "myPackage.which.does.not.exist" false |> Async.RunSynchronously
        
    package.IsNone |> shouldBeTrue

/// -------------------------------------
/// MyGet tests
/// -------------------------------------

[<Fact>]
let ``NEventSocket returns correct result``() =
    let package = MyGet.getPackageAsync ("neventsocket-prerelease", "NEventSocket") false |> Async.RunSynchronously
        
    package.Value.Name        |> shouldEqual "NEventSocket"
    package.Value.Version     |> shouldEqual "2.1.0-build00282"
    package.Value.Downloads   |> shouldBeGreaterThan 4

[<Fact>]
let ``MyGet Package written in lowercase returns correct result``() =
    let package = MyGet.getPackageAsync ("neventsocket-prerelease", "neventsocket") false |> Async.RunSynchronously
        
    package.Value.Name        |> shouldEqual "NEventSocket"
    package.Value.Version     |> shouldEqual "2.1.0-build00282"
    package.Value.Downloads   |> shouldBeGreaterThan 4

[<Fact>]
let ``Non existing MyGet package returns none``() =
    let package = MyGet.getPackageAsync ("not-found", "myPackage.which.does.not.exist") false |> Async.RunSynchronously
        
    package.IsNone |> shouldBeTrue