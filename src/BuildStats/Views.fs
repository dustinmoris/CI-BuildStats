module BuildStats.Views

open Giraffe.GiraffeViewEngine
open BuildStats.Common

let minifiedCss =
    "Assets/site.css"
    |> Css.getMinifiedContent

let cssPath = "/site.css"
let cssHash = Hash.md5 minifiedCss

let masterView (pageTitle : string)
               (content   : XmlNode list) =
    html [] [
        head [] [
            meta [ _charset "utf-8" ]
            meta [ _name "description"; _content "Little SVG widget to display AppVeyor, TravisCI, CircleCI or Azure Pipelines build history charts and other SVG badges" ]
            meta [ _name "author"; _content "Dustin Moris Gorski, https://dusted.codes/" ]

            link [ attr "href" (sprintf "%s?v=%s" cssPath cssHash); attr "rel" "stylesheet" ]

            title [] [ encodedText pageTitle ]
        ]
        body [] content
    ]

let indexView =
    [
        main [] [
            h1 [] [ rawText "BuildStats.info" ]
            h2 [] [ rawText "SVG widget to display build history charts and other badges" ]

            h3 [] [ rawText "Build History Chart" ]
            p [] [ rawText "Add a build history widget to your public Git repository:" ]
            img [ _src "/appveyor/chart/dustinmoris/dustedcodes?branch=master" ]
            p [ _style "margin-top: 2em;" ] [ rawText "Build history charts are currently supported for:" ]
            ul [] [
                li [] [ a [ _href "https://www.appveyor.com/"; _target "_blank" ] [ rawText "AppVeyor" ] ]
                li [] [ a [ _href "https://travis-ci.org/"; _target "_blank" ] [ rawText "TravisCI" ] ]
                li [] [ a [ _href "https://circleci.com/"; _target "_blank" ] [ rawText "CircleCI" ] ]
                li [] [ a [ _href "https://azure.microsoft.com/en-us/services/devops/pipelines/"; _target "_blank" ] [ rawText "Azure Pipelines" ] ]
            ]

            h3 [] [ rawText "NuGet and MyGet Badges" ]
            p [] [ rawText "Display the most beautiful badge for your NuGet or MyGet package:" ]
            img [ _src "/nuget/nunit" ]

            h3 [] [ rawText "About" ]
            p [] [
                rawText "For more information please visit the "
                a [ _href "https://github.com/dustinmoris/CI-BuildStats" ] [ rawText "official GitHub repository" ]
                rawText "."
            ]
            h3 [] [ rawText "Support" ]
            p [] [
                rawText "If you've got value from any of the content which I have created, then I would very much appreciate your support by buying me a coffee."
                a [
                    _href "https://www.buymeacoffee.com/dustinmoris"
                    _target "_blank"
                ] [
                    img [
                        _src "https://www.buymeacoffee.com/assets/img/custom_images/yellow_img.png"
                        _alt "Buy Me A Coffee"
                        _style "height: auto !important;width: auto !important;"
                    ]
                ]
            ]
            p [ _class "footer" ] [
                rawText "BuildStats.info is provided by "
                a [ _href "https://dusted.codes/" ] [ rawText "Dustin Moris Gorski" ]
                rawText "."
            ]
        ]
    ] |> masterView "BuildStats.info"

let createApiTokenView =
    [
        main [] [
            h1 [] [ rawText "BuildStats.info" ]
            h2 [] [ rawText "SVG widget to display build history charts and other badges" ]

            form [ _method "POST" ] [
                input [ _id "plaintext"; _name "plaintext"; _type "text" ]
                input [ _type "submit" ]
            ]
        ]
    ] |> masterView "BuildStats.info"

let visualTestsView =
    [
        main [] [
            h1 [] [ rawText "BuildStats.info" ]

            h2 [] [ rawText "Build History Chart" ]

            h3 [] [ rawText "AppVeyor" ]
            table [] [
                tr [] [
                    th [] [ rawText "Basic" ]
                    td [] [ img [ _src "/appveyor/chart/CharliePoole/nunit" ] ]
                ]
                tr [] [
                    th [] [ rawText "Branch Filter" ]
                    td [] [ img [ _src "/appveyor/chart/CharliePoole/nunit?branch=master" ] ]
                ]
                tr [] [
                    th [] [ rawText "Reduced BuildCount" ]
                    td [] [ img [ _src "/appveyor/chart/CharliePoole/nunit?buildCount=10" ] ]
                ]
                tr [] [
                    th [] [ rawText "Increased BuildCount" ]
                    td [] [ img [ _src "/appveyor/chart/CharliePoole/nunit?buildCount=40" ] ]
                ]
                tr [] [
                    th [] [ rawText "Exclude PullRequests" ]
                    td [] [ img [ _src "/appveyor/chart/CharliePoole/nunit?includeBuildsFromPullRequest=false" ] ]
                ]
                tr [] [
                    th [] [ rawText "Hide Stats" ]
                    td [] [ img [ _src "/appveyor/chart/CharliePoole/nunit?showStats=false" ] ]
                ]
            ]

            h3 [] [ rawText "TravisCI" ]
            table [] [
                tr [] [
                    th [] [ rawText "Basic" ]
                    td [] [ img [ _src "/travisci/chart/nunit/nunit" ] ]
                ]
                tr [] [
                    th [] [ rawText "Branch Filter" ]
                    td [] [ img [ _src "/travisci/chart/nunit/nunit?branch=master" ] ]
                ]
                tr [] [
                    th [] [ rawText "Reduced BuildCount" ]
                    td [] [ img [ _src "/travisci/chart/nunit/nunit?buildCount=10" ] ]
                ]
                tr [] [
                    th [] [ rawText "Increased BuildCount" ]
                    td [] [ img [ _src "/travisci/chart/nunit/nunit?buildCount=40" ] ]
                ]
                tr [] [
                    th [] [ rawText "Exclude PullRequests" ]
                    td [] [ img [ _src "/travisci/chart/nunit/nunit?includeBuildsFromPullRequest=false" ] ]
                ]
                tr [] [
                    th [] [ rawText "Hide Stats" ]
                    td [] [ img [ _src "/travisci/chart/nunit/nunit?showStats=false" ] ]
                ]
                tr [] [
                    th [] [ rawText "Travis-CI.com" ]
                    td [] [ img [ _src "/travisci/chart/martincostello/sqllocaldb?branch=master&includeBuildsFromPullRequest=false" ] ]
                ]
            ]

            h3 [] [ rawText "CircleCI" ]
            table [] [
                tr [] [
                    th [] [ rawText "Basic" ]
                    td [] [ img [ _src "/circleci/chart/spotify/helios" ] ]
                ]
                tr [] [
                    th [] [ rawText "Branch Filter" ]
                    td [] [ img [ _src "/circleci/chart/spotify/helios?branch=master" ] ]
                ]
                tr [] [
                    th [] [ rawText "Reduced BuildCount" ]
                    td [] [ img [ _src "/circleci/chart/spotify/helios?buildCount=10" ] ]
                ]
                tr [] [
                    th [] [ rawText "Increased BuildCount" ]
                    td [] [ img [ _src "/circleci/chart/spotify/helios?buildCount=40" ] ]
                ]
                tr [] [
                    th [] [ rawText "Exclude PullRequests" ]
                    td [] [ img [ _src "/circleci/chart/spotify/helios?includeBuildsFromPullRequest=false" ] ]
                ]
                tr [] [
                    th [] [ rawText "Hide Stats" ]
                    td [] [ img [ _src "/circleci/chart/spotify/helios?showStats=false" ] ]
                ]
            ]

            h3 [] [ rawText "Azure Pipelines" ]
            table [] [
                tr [] [
                    th [] [ rawText "Basic" ]
                    td [] [ img [ _src "/azurepipelines/chart/github/Desktop/3" ] ]
                ]
                tr [] [
                    th [] [ rawText "Branch Filter" ]
                    td [] [ img [ _src "/azurepipelines/chart/github/Desktop/3?branch=master" ] ]
                ]
                tr [] [
                    th [] [ rawText "Build Definition Filter" ]
                    td [] [ img [ _src "/azurepipelines/chart/dnceng/public/59?branch=master" ] ]
                ]
                tr [] [
                    th [] [ rawText "Reduced BuildCount" ]
                    td [] [ img [ _src "/azurepipelines/chart/github/Desktop/3?buildCount=10" ] ]
                ]
                tr [] [
                    th [] [ rawText "Increased BuildCount" ]
                    td [] [ img [ _src "/azurepipelines/chart/github/Desktop/3?buildCount=40" ] ]
                ]
                tr [] [
                    th [] [ rawText "Exclude PullRequests" ]
                    td [] [ img [ _src "/azurepipelines/chart/github/Desktop/3?includeBuildsFromPullRequest=false" ] ]
                ]
                tr [] [
                    th [] [ rawText "Hide Stats" ]
                    td [] [ img [ _src "/azurepipelines/chart/github/Desktop/3?showStats=false" ] ]
                ]
                tr [] [
                    th [] [ rawText "Branch and Exclude PullRequests" ]
                    td [] [ img [ _src "/azurepipelines/chart/martincostello/sqllocaldb/66?branch=master&includeBuildsFromPullRequest=false" ] ]
                ]
            ]

            h2 [] [ rawText "Package Badges" ]

            h3 [] [ rawText "NuGet" ]
            table [] [
                tr [] [
                    th [] [ rawText "Lanem" ]
                    td [] [ img [ _src "/nuget/lanem" ] ]
                ]
                tr [] [
                    th [] [ rawText "Guardo" ]
                    td [] [ img [ _src "/nuget/guardo" ] ]
                ]
                tr [] [
                    th [] [ rawText "Newtonsoft.Json" ]
                    td [] [ img [ _src "/nuget/Newtonsoft.Json" ] ]
                ]
                tr [] [
                    th [] [ rawText "Moq" ]
                    td [] [ img [ _src "/nuget/moq" ] ]
                ]
                tr [] [
                    th [] [ rawText "Nunit" ]
                    td [] [ img [ _src "/nuget/nunit" ] ]
                ]
                tr [] [
                    th [] [ rawText "NSubstitute" ]
                    td [] [ img [ _src "/nuget/nsubstitute" ] ]
                ]
                tr [] [
                    th [] [ rawText "jQuery" ]
                    td [] [ img [ _src "/nuget/jQuery" ] ]
                ]
                tr [] [
                    th [] [ rawText "ASP.NET MVC" ]
                    td [] [ img [ _src "/nuget/microsoft.aspnet.mvc" ] ]
                ]
                tr [] [
                    th [] [ rawText "EntityFramework" ]
                    td [] [ img [ _src "/nuget/entityframework" ] ]
                ]
                tr [] [
                    th [] [ rawText "NServiceBus.PostgreSQL" ]
                    td [] [ img [ _src "/nuget/NServiceBus.PostgreSQL?includePreReleases=true" ] ]
                ]
            ]

            h3 [] [ rawText "MyGet" ]
            table [] [
                tr [] [
                    th [] [ rawText "neventsocket-prerelease/NEventSocket" ]
                    td [] [ img [ _src "/myget/neventsocket-prerelease/NEventSocket" ] ]
                ]
            ]

            h3 [] [ rawText "MyGet Enterprise" ]
            table [] [
                tr [] [
                    th [] [ rawText "botbuilder/botbuilder-v4-dotnet-daily/Microsoft.Bot.Builder" ]
                    td [] [ img [ _src "/myget/botbuilder/botbuilder-v4-dotnet-daily/Microsoft.Bot.Builder" ] ]
                ]
                tr [] [
                    th [] [ rawText "botbuilder/botbuilder-v4-dotnet-daily/Microsoft.Bot.Configuration" ]
                    td [] [ img [ _src "/myget/botbuilder/botbuilder-v4-dotnet-daily/Microsoft.Bot.Configuration" ] ]
                ]
            ]

            p [] [
                rawText "BuildStats.info is provided by "
                a [ _href "https://dusted.codes/" ] [ rawText "Dustin Moris Gorski" ]
                rawText "."
            ]
        ]
    ] |> masterView "BuildStats.info"
