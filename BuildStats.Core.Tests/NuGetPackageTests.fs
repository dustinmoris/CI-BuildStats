module NuGetPackageTests

open NUnit.Framework
open FsUnit
open PackageServices

[<Test>]
let ``Lanem returns correct result``() =
    let package = NuGet.getPackageAsync "Lanem" false |> Async.RunSynchronously
        
    package.Value.Name        |> should equal "Lanem"
    package.Value.Version     |> should equal "2.0.0"
    package.Value.Downloads   |> should be (greaterThan 900)

[<Test>]
let ``Guardo returns correct result``() =
    let package = NuGet.getPackageAsync "Guardo" false |> Async.RunSynchronously
        
    package.Value.Name        |> should equal "Guardo"
    package.Value.Version     |> should equal "0.0.1"
    package.Value.Downloads   |> should be (greaterThan 49)

[<Test>]
let ``Moq returns correct result``() =
    let package = NuGet.getPackageAsync "Moq" false |> Async.RunSynchronously
        
    package.Value.Name        |> should equal "Moq"
    package.Value.Version     |> should equal "4.2.1510.2205"
    package.Value.Downloads   |> should be (greaterThan 1874730)

[<Test>]
let ``NUnit returns correct result``() =
    let package = NuGet.getPackageAsync "NUnit" false |> Async.RunSynchronously
        
    package.Value.Name        |> should equal "NUnit"
    package.Value.Version     |> should equal "3.2.1"
    package.Value.Downloads   |> should be (greaterThan 1283920)

[<Test>]
let ``NSubstitute returns correct result``() =
    let package = NuGet.getPackageAsync "NSubstitute" false |> Async.RunSynchronously
        
    package.Value.Name        |> should equal "NSubstitute"
    package.Value.Version     |> should equal "1.10.0"
    package.Value.Downloads   |> should be (greaterThan 661870)

[<Test>]
let ``jQuery returns correct result``() =
    let package = NuGet.getPackageAsync "jQuery" false |> Async.RunSynchronously
        
    package.Value.Name        |> should equal "jQuery"
    package.Value.Version     |> should equal "2.2.3"
    package.Value.Downloads   |> should be (greaterThan 4233340)

[<Test>]
let ``Microsoft.AspNet.Mvc returns correct result``() =
    let package = NuGet.getPackageAsync "Microsoft.AspNet.Mvc" false |> Async.RunSynchronously
        
    package.Value.Name        |> should equal "Microsoft.AspNet.Mvc"
    package.Value.Version     |> should equal "5.2.3"
    package.Value.Downloads   |> should be (greaterThan 2956200)

[<Test>]
let ``EntityFramework returns correct result``() =
    let package = NuGet.getPackageAsync "EntityFramework" false |> Async.RunSynchronously
        
    package.Value.Name        |> should equal "EntityFramework"
    package.Value.Version     |> should equal "6.1.3"
    package.Value.Downloads   |> should be (greaterThan 4496670)

[<Test>]
let ``NServiceBus.PostgreSQL PreRelease package returns correct result``() =
    let package = NuGet.getPackageAsync "NServiceBus.PostgreSQL" true |> Async.RunSynchronously
        
    package.Value.Name        |> should equal "NServiceBus.PostgreSQL"
    package.Value.Version     |> should equal "1.0.0-CI00021"
    package.Value.Downloads   |> should be (greaterThan 550)

[<Test>]
let ``Newtonsoft.Json returns correct result``() =
    let package = NuGet.getPackageAsync "Newtonsoft.Json" false |> Async.RunSynchronously
        
    package.Value.Name        |> should equal "Newtonsoft.Json"
    package.Value.Version     |> should equal "8.0.3"
    package.Value.Downloads   |> should be (greaterThan 4998550)

[<Test>]
let ``Paket returns correct result``() =
    let package = NuGet.getPackageAsync "Paket" false |> Async.RunSynchronously
        
    package.Value.Name        |> should equal "Paket"
    package.Value.Version     |> should equal "2.65.2"
    package.Value.Downloads   |> should be (greaterThan 127890)

[<Test>]
let ``Package written in lowercase returns correct result``() =
    let package = NuGet.getPackageAsync "newtonsoft.json" false |> Async.RunSynchronously
        
    package.Value.Name        |> should equal "Newtonsoft.Json"
    package.Value.Version     |> should equal "8.0.3"
    package.Value.Downloads   |> should be (greaterThan 4998550)

[<Test>]
let ``Non existing package returns none``() =
    let package = NuGet.getPackageAsync "myPackage.which.does.not.exist" false |> Async.RunSynchronously
        
    package.IsNone |> should equal true