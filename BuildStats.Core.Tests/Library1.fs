module PackageServicesTests

open NUnit.Framework
open FsUnit
open PackageServices
open RestClient

[<Test>]
let ``NuGet package Lanem returns correct result``() =    
    let package = getNuGetPackageAsync "lanem" false |> Async.RunSynchronously
        
    package.Name        |> should equal "Lanem"
    package.Version     |> should equal "2.0.0"
    package.Downloads   |> should be (greaterThan 900)

[<Test>]
let ``NuGet package Newtonsoft.Json returns correct result``() =    
    let package = getNuGetPackageAsync "newtonsoft.json" false |> Async.RunSynchronously
        
    package.Name        |> should equal "Newtonsoft.Json"
    package.Version     |> should equal "8.0.3"
    package.Downloads   |> should be (greaterThan 4998550)

[<Test>]
let ``NuGet package Paket returns correct result``() =    
    let package = getNuGetPackageAsync "paket" false |> Async.RunSynchronously
        
    package.Name        |> should equal "Paket"
    package.Version     |> should equal "3.0.0-alpha109"
    package.Downloads   |> should be (greaterThan 127890)