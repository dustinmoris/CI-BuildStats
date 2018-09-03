module BuildStats.Views

open Giraffe.GiraffeViewEngine
open BuildStats.Common

let minifiedCss =
    "Assets/site.css"
    |> StaticAssets.minifyCssFile

let cssHash = Hash.sha1 minifiedCss

let masterView (pageTitle : string)
               (content   : XmlNode list) =
    html [] [
        head [] [
            meta [ _charset "utf-8" ]
            meta [ _name "description"; _content "Little SVG widget to display AppVeyor, TravisCI or CircleCI build history charts and other SVG badges" ]
            meta [ _name "author"; _content "Dustin Moris Gorski, https://dusted.codes/" ]

            link [ attr "href" (sprintf "/site.css?v=%s" cssHash); attr "rel" "stylesheet" ]

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
            p [] [ rawText "Add a build history widget to your public GitHub repository:" ]
            img [ _src "/appveyor/chart/dustinmoris/dustedcodes?branch=master" ]

            h3 [] [ rawText "NuGet and MyGet Badges" ]
            p [] [ rawText "Display a badge for your NuGet or MyGet packages:" ]
            img [ _src "/nuget/nunit" ]

            h3 [] [ rawText "About" ]
            p [] [
                rawText "For more information please visit the "
                a [ _href "https://github.com/dustinmoris/CI-BuildStats" ] [ rawText "official GitHub repository" ]
                rawText "."
            ]
            p [] [
                rawText "BuildStats.info is provided by "
                a [ _href "https://dusted.codes/" ] [ rawText "Dustin Moris Gorski" ]
                rawText "."
            ]
        ]
    ] |> masterView "BuildStats.info"