module BuildStats.SVGs

open System
open Giraffe.GiraffeViewEngine
open BuildStats.ViewModels
open BuildStats.TextSize

let svg      = tag     "svg"
let g        = tag     "g"
let defs     = tag     "defs"
let gradient = tag     "linearGradient"
let text     = tag     "text"
let rect     = voidTag "rect"
let stop     = voidTag "stop"

let defaultComment =
    let nl = Environment.NewLine
    sprintf "%sThis SVG badge is provided by Dustin Moris Gorski (https://dusted.codes/).%sAll source code is open source and hosted on GitHub (https://github.com/dustinmoris/CI-BuildStats/).%s%s" nl nl nl nl |> comment

let defaultSvg (width : int) (height : int) =
    tag "svg" [ attr "xmlns" "http://www.w3.org/2000/svg"
                attr "style" "shape-rendering: geometricPrecision; image-rendering: optimizeQuality; fill-rule: evenodd; clip-rule: evenodd"
                attr "width" (width.ToString())
                attr "height" (height.ToString())
                attr "fill" "None" ]

let defaultG (fill : string) =
    g [ attr "font-family" "Helvetica,Arial,sans-serif"
        attr "font-size" "12"
        attr "fill" fill ]

let whiteStop (offset : int) (opacity : float) =
    stop [ attr "offset" (sprintf "%i%%" offset)
           attr "style" (sprintf "stop-color: rgb(240, 240, 240); stop-opacity: %.1f" opacity)]

let packageGradient =
    gradient [
        attr "id" "grad1"
        attr "x1" "0%"
        attr "y1" "0%"
        attr "x2" "0%"
        attr "y2" "100%" ]
        [
            whiteStop 0 0.15
            whiteStop 100 0.0 ]

let squareRect (x : int) (y : int) (width : int) (height : int) (fill : string) =
    rect [
        attr "x" (x.ToString())
        attr "y" (y.ToString())
        attr "height" (height.ToString())
        attr "width" (width.ToString())
        attr "stroke-width" "0"
        attr "fill" fill
    ]

let roundedRect (x : int) (y : int) (width : int)  (height : int)(fill : string) =
    rect [
        attr "x" (x.ToString())
        attr "y" (y.ToString())
        attr "height" (height.ToString())
        attr "width" (width.ToString())
        attr "rx" "2.5"
        attr "ry" "2.5"
        attr "stroke-width" "0"
        attr "fill" fill
    ]

let colouredText (colour : string) (x : int) (y : int) (value : string) =
    text [
        attr "x" (x.ToString())
        attr "y" (y.ToString())
        attr "fill" colour
    ] [ rawText value ]

let whiteText = colouredText "#ffffff"
let blackText = colouredText "#777777"

let packageSVG (model : PackageModel) = [
    defaultComment
    defaultSvg model.Width 20 [
        defaultG "#000000" [
            defs [] [ packageGradient ]
            roundedRect
                0 0
                (model.Width - 50) 20
                "#444444"
            squareRect
                model.FeedWidth 0
                (model.VersionWidth) 20
                "#43ba1b"
            squareRect
                (model.FeedWidth + model.VersionWidth) 0
                (model.DownloadsWidth - 10) 20
                "#483C32"
            roundedRect
                (model.FeedWidth + model.VersionWidth) 0
                (model.DownloadsWidth) 20
                "#483C32"
            roundedRect
                0 0
                model.Width 20
                "url(#grad1)"

            blackText (model.FeedWidth + model.Padding) 15 model.Version

            whiteText model.Padding 14 model.FeedName
            whiteText (model.FeedWidth + model.Padding) 14 model.Version
            whiteText (model.FeedWidth + model.VersionWidth + model.Padding) 14 model.Downloads ] ] ]

let buildHistorySVG (model : BuildHistoryModel) =
    defaultSvg model.Width model.Height [
        defaultG "#777777" [
            yield text [
                attr "x" "0"; attr "y" "12"; attr "font-weight" "bold"; attr "fill" "#000000"
            ] [ rawText model.Branch ]

            if model.ShowStats then
                yield text [ attr "x" "0"; attr "y" "27" ] [ rawText model.MaxBuild ]
                yield text [ attr "x" "0"; attr "y" "42" ] [ rawText model.MinBuild ]
                yield text [ attr "x" "0"; attr "y" "57" ] [ rawText model.AvgBuild ]

            yield!
                model.BuildBars
                |> List.map (fun b -> squareRect b.X b.Y 5 b.Height b.Colour)
        ]
    ]

let measureCharsSVG =
    let isEven x = (x % 2) = 0
    defaultSvg 800 800 [
        defaultG "#000000" [
            yield!
                chars
                |> Seq.map (fun kv -> kv.Key, kv.Value)
                |> Seq.mapFold (fun (x, i) (c, w) ->
                    match isEven i with
                    | true  -> squareRect x 0 w 300 "#ff0000", (x + w, i + 1)
                    | false -> squareRect x 0 w 300 "#00ff00", (x + w, i + 1)) (0, 0)
                |> fst

            yield!
                chars
                |> Seq.map (fun kv -> kv.Key, kv.Value)
                |> Seq.mapFold (fun x (c, w) ->
                    text [
                        attr "x" (x.ToString())
                        attr "y" "100"
                        attr "fill" "#000000"
                    ] [ encodedText (c.ToString()) ], x + w ) 0
                |> fst
        ]
    ]