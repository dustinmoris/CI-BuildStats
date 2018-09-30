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
let path     = tag     "path"
let mask     = tag     "mask"
let rect     = voidTag "rect"
let stop     = voidTag "stop"


let nuGetSvg =
    svg [
        attr "x" "3"
        attr "y" "2"
        _width "14px"
        _height "14px"
        attr "viewBox" "0 0 512 512"
        attr "xmlns" "http://www.w3.org/2000/svg"
        attr "xmlns:xlink" "http://www.w3.org/1999/xlink"
    ] [
        g [
            attr "stroke" "none"
            attr "stroke-width" "1"
            attr "fill" "none"
            attr "fill-rule" "evenodd"
        ] [
            defs [] [
                tag "polygon" [
                    _id "path-1"
                    attr "points" "0 46.021103 0 3.7002935 84.6521577 3.7002935 84.6521577 88.3419125 0 88.3419125"
                ] []
            ]
            g [ attr "transform" "translate(0.000000, 6.000000)" ] [
                path [
                    attr "d" "M374.424959,454.856991 C327.675805,454.856991 289.772801,416.950177 289.772801,370.196324 C289.772801,323.463635 327.675805,285.535656 374.424959,285.535656 C421.174113,285.535656 459.077116,323.463635 459.077116,370.196324 C459.077116,416.950177 421.174113,454.856991 374.424959,454.856991 M205.565067,260.814741 C176.33891,260.814741 152.657469,237.109754 152.657469,207.901824 C152.657469,178.672728 176.33891,154.988907 205.565067,154.988907 C234.791225,154.988907 258.472666,178.672728 258.472666,207.901824 C258.472666,237.109754 234.791225,260.814741 205.565067,260.814741 M378.170817,95.6417786 L236.886365,95.6417786 C164.889705,95.6417786 106.479717,154.057639 106.479717,226.082702 L106.479717,367.360191 C106.479717,439.40642 164.889705,497.77995 236.886365,497.77995 L378.170817,497.77995 C450.209803,497.77995 508.577466,439.40642 508.577466,367.360191 L508.577466,226.082702 C508.577466,154.057639 450.209803,95.6417786 378.170817,95.6417786"
                    attr "id" "Fill-12"
                    attr "fill" "#eeeeee"
                    attr "fill-rule" "evenodd"
                ] [
                    mask [
                        _id "mask-2"
                        attr "fill" "white"
                    ] [
                        tag "use" [ attr "xlink:href" "#path-1" ] []
                    ]
                ]
                path [
                    attr "d" "M84.6521577,46.0115787 C84.6521577,69.3990881 65.6900744,88.3419125 42.3260788,88.3419125 C18.9409203,88.3419125 0,69.3990881 0,46.0115787 C0,22.6452344 18.9409203,3.68124485 42.3260788,3.68124485 C65.6900744,3.68124485 84.6521577,22.6452344 84.6521577,46.0115787"
                    _id "Fill-14"
                    attr "fill" "#eeeeee"
                    attr "fill-rule" "evenodd"
                    attr "mask" "url(#mask-2)"
                ] []
            ]
        ]
    ]


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

let packageSVG (model : PackageModel) =
    let nugetLogoWidth = 16
    [
        defaultComment
        defaultSvg (model.Width + nugetLogoWidth) 20 [
            defaultG "#000000" [
                defs [] [ packageGradient ]
                roundedRect
                    0 0
                    (nugetLogoWidth + model.Width - 50) 20
                    "#444444"
                squareRect
                    (nugetLogoWidth + model.FeedWidth) 0
                    (model.VersionWidth) 20
                    "#43ba1b"
                squareRect
                    (nugetLogoWidth + model.FeedWidth + model.VersionWidth) 0
                    (model.DownloadsWidth - 10) 20
                    "#483C32"
                roundedRect
                    (nugetLogoWidth + model.FeedWidth + model.VersionWidth) 0
                    (model.DownloadsWidth) 20
                    "#483C32"
                roundedRect
                    0 0
                    (nugetLogoWidth + model.Width) 20
                    "url(#grad1)"

                blackText (nugetLogoWidth + model.FeedWidth + model.Padding) 15 model.Version

                whiteText (nugetLogoWidth + model.Padding) 14 model.FeedName
                whiteText (nugetLogoWidth + model.FeedWidth + model.Padding) 14 model.Version
                whiteText (nugetLogoWidth + model.FeedWidth + model.VersionWidth + model.Padding) 14 model.Downloads ]
            g [] [
                nuGetSvg
            ] ]
        ]

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