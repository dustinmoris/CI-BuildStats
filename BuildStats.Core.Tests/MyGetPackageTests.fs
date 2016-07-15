module MyGetPackageTests

open NUnit.Framework
open FsUnit
open PackageServices

[<Test>]
let ``NEventSocket returns correct result``() =
    let package = MyGet.getPackageAsync ("neventsocket-prerelease", "NEventSocket") false |> Async.RunSynchronously
        
    package.Value.Name        |> should equal "NEventSocket"
    package.Value.Version     |> should equal "2.0.0-build00240"
    package.Value.Downloads   |> should be (greaterThan 4)

[<Test>]
let ``Package written in lowercase returns correct result``() =
    let package = MyGet.getPackageAsync ("neventsocket-prerelease", "neventsocket") false |> Async.RunSynchronously
        
    package.Value.Name        |> should equal "NEventSocket"
    package.Value.Version     |> should equal "2.0.0-build00240"
    package.Value.Downloads   |> should be (greaterThan 4)

[<Test>]
let ``Non existing package returns none``() =
    let package = MyGet.getPackageAsync ("not-found", "myPackage.which.does.not.exist") false |> Async.RunSynchronously
        
    package.IsNone |> should equal true