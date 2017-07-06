module BuildStats.Views

open System
open Giraffe.XmlViewEngine
open BuildStats.Models
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
    tag "svg" [ "xmlns", "http://www.w3.org/2000/svg"
                "style", "shape-rendering: geometricPrecision; image-rendering: optimizeQuality; fill-rule: evenodd; clip-rule: evenodd"
                "width", width.ToString()
                "height", height.ToString()
                "fill", "None" ]

let defaultG (fill : string) =
    g [ "font-family", "Helvetica,Arial,sans-serif"
        "font-size", "12"
        "fill", fill ]

let whiteStop (offset : int) (opacity : float) =
    stop [ "offset", (sprintf "%i%%" offset)
           "style", (sprintf "stop-color: rgb(255, 255, 255); stop-opacity: %.1f" opacity)]

let packageGradient =
    gradient [
        "id", "grad1"
        "x1", "0%"
        "y1", "0%"
        "x2", "0%"
        "y2", "100%" ]
        [
            whiteStop 0 0.3
            whiteStop 100 0.0 ]

let squareRect (x : int) (y : int) (width : int) (height : int) (fill : string) =
    rect [
        "x", x.ToString()
        "y", y.ToString()
        "height", height.ToString()
        "width", width.ToString()
        "stroke-width", "0"
        "fill", fill
    ]

let roundedRect (x : int) (y : int) (width : int)  (height : int)(fill : string) =
    rect [
        "x", x.ToString()
        "y", y.ToString()
        "height", height.ToString()
        "width", width.ToString()
        "rx", "2"
        "ry", "2"
        "stroke-width", "0"
        "fill", fill
    ]

let whiteText (x : int) (y : int) (value : string) =
    text [
        "x", x.ToString()
        "y", y.ToString()
        "fill", "#ffffff"
    ] [ rawText value ]

let packageView (model : PackageModel) = [
    defaultComment
    defaultSvg model.Width 20 [
        defaultG "#000000" [
            defs [] [ packageGradient ]
            roundedRect
                0 0
                (model.Width - 50) 20
                "#333333"
            squareRect
                model.FeedWidth 0
                (model.VersionWidth) 20
                "#00b359"
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
            whiteText model.Padding 14 model.FeedName
            whiteText (model.FeedWidth + model.Padding) 14 model.Version
            whiteText (model.FeedWidth + model.VersionWidth + model.Padding) 14 model.Downloads ] ] ]

let buildHistoryView (model : BuildHistoryModel) =
    defaultSvg model.Width model.Height [
        defaultG "#777777" [
            yield text [
                "x", "0"; "y", "12"; "font-weight", "bold"; "fill", "#000000"
            ] [ rawText model.Branch ]

            if model.ShowStats then
                yield text [ "x", "0"; "y", "27" ] [ rawText model.MaxBuild ]
                yield text [ "x", "0"; "y", "42" ] [ rawText model.MinBuild ]
                yield text [ "x", "0"; "y", "57" ] [ rawText model.AvgBuild ]

            yield!
                model.BuildBars
                |> List.map (fun b -> squareRect b.X b.Y 5 b.Height b.Colour)
        ]
    ]

let measureCharsView =
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
                        "x", x.ToString()
                        "y", "100"
                        "fill", "#000000"
                    ] [ encodedText (c.ToString()) ], x + w ) 0
                |> fst
        ]
    ]