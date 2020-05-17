namespace BuildStats

[<RequireQualifiedAccess>]
module SVGs =
    open System
    open Giraffe.GiraffeViewEngine

    let private svg      = tag     "svg"
    let private g        = tag     "g"
    let private defs     = tag     "defs"
    let private gradient = tag     "linearGradient"
    let private text     = tag     "text"
    let private path     = tag     "path"
    let private mask     = tag     "mask"
    let private rect     = voidTag "rect"
    let private stop     = voidTag "stop"

    let nuget =
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

    let rust =
        svg [
            _width "144px"
            _height "144px"
            attr "viewBox" "16 16 800 800"
            attr "xmlns" "http://www.w3.org/2000/svg"
        ] [
            path [ attr "d" "m71.05 23.68c-26.06 0-47.27 21.22-47.27 47.27s21.22 47.27 47.27 47.27 47.27-21.22 47.27-47.27-21.22-47.27-47.27-47.27zm-.07 4.2a3.1 3.11 0 0 1 3.02 3.11 3.11 3.11 0 0 1 -6.22 0 3.11 3.11 0 0 1 3.2-3.11zm7.12 5.12a38.27 38.27 0 0 1 26.2 18.66l-3.67 8.28c-.63 1.43.02 3.11 1.44 3.75l7.06 3.13a38.27 38.27 0 0 1 .08 6.64h-3.93c-.39 0-.55.26-.55.64v1.8c0 4.24-2.39 5.17-4.49 5.4-2 .23-4.21-.84-4.49-2.06-1.18-6.63-3.14-8.04-6.24-10.49 3.85-2.44 7.85-6.05 7.85-10.87 0-5.21-3.57-8.49-6-10.1-3.42-2.25-7.2-2.7-8.22-2.7h-40.6a38.27 38.27 0 0 1 21.41-12.08l4.79 5.02c1.08 1.13 2.87 1.18 4 .09zm-44.2 23.02a3.11 3.11 0 0 1 3.02 3.11 3.11 3.11 0 0 1 -6.22 0 3.11 3.11 0 0 1 3.2-3.11zm74.15.14a3.11 3.11 0 0 1 3.02 3.11 3.11 3.11 0 0 1 -6.22 0 3.11 3.11 0 0 1 3.2-3.11zm-68.29.5h5.42v24.44h-10.94a38.27 38.27 0 0 1 -1.24-14.61l6.7-2.98c1.43-.64 2.08-2.31 1.44-3.74zm22.62.26h12.91c.67 0 4.71.77 4.71 3.8 0 2.51-3.1 3.41-5.65 3.41h-11.98zm0 17.56h9.89c.9 0 4.83.26 6.08 5.28.39 1.54 1.26 6.56 1.85 8.17.59 1.8 2.98 5.4 5.53 5.4h16.14a38.27 38.27 0 0 1 -3.54 4.1l-6.57-1.41c-1.53-.33-3.04.65-3.37 2.18l-1.56 7.28a38.27 38.27 0 0 1 -31.91-.15l-1.56-7.28c-.33-1.53-1.83-2.51-3.36-2.18l-6.43 1.38a38.27 38.27 0 0 1 -3.32-3.92h31.27c.35 0 .59-.06.59-.39v-11.06c0-.32-.24-.39-.59-.39h-9.15zm-14.43 25.33a3.11 3.11 0 0 1 3.02 3.11 3.11 3.11 0 0 1 -6.22 0 3.11 3.11 0 0 1 3.2-3.11zm46.05.14a3.11 3.11 0 0 1 3.02 3.11 3.11 3.11 0 0 1 -6.22 0 3.11 3.11 0 0 1 3.2-3.11z"; attr "fill" "#eeeeee"; attr "fill-rule" "evenodd" ] []
            //path [ attr "d" "m115.68 70.95a44.63 44.63 0 0 1 -44.63 44.63 44.63 44.63 0 0 1 -44.63-44.63 44.63 44.63 0 0 1 44.63-44.63 44.63 44.63 0 0 1 44.63 44.63zm-.84-4.31 6.96 4.31-6.96 4.31 5.98 5.59-7.66 2.87 4.78 6.65-8.09 1.32 3.4 7.46-8.19-.29 1.88 7.98-7.98-1.88.29 8.19-7.46-3.4-1.32 8.09-6.65-4.78-2.87 7.66-5.59-5.98-4.31 6.96-4.31-6.96-5.59 5.98-2.87-7.66-6.65 4.78-1.32-8.09-7.46 3.4.29-8.19-7.98 1.88 1.88-7.98-8.19.29 3.4-7.46-8.09-1.32 4.78-6.65-7.66-2.87 5.98-5.59-6.96-4.31 6.96-4.31-5.98-5.59 7.66-2.87-4.78-6.65 8.09-1.32-3.4-7.46 8.19.29-1.88-7.98 7.98 1.88-.29-8.19 7.46 3.4 1.32-8.09 6.65 4.78 2.87-7.66 5.59 5.98 4.31-6.96 4.31 6.96 5.59-5.98 2.87 7.66 6.65-4.78 1.32 8.09 7.46-3.4-.29 8.19 7.98-1.88-1.88 7.98 8.19-.29-3.4 7.46 8.09 1.32-4.78 6.65 7.66 2.87z"; attr "fill-rule" "evenodd"; attr "stroke" "#eeeeee"; attr "stroke-linecap" "round"; attr "stroke-linejoin" "round"; attr "stroke-width" "3" ] []
            voidTag "circle" [ attr "cx" "71"; attr "cy" "71"; attr "r" "50"; attr "stroke" "#505050"; attr "stroke-width" "10"; attr "fill" "transparent" ]
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

    let package (logoSvg : XmlNode) (model : PackageModel) =
        let logoWidth = 16
        let downloadColour = "#007ec6" // #483C32
        [
            defaultComment
            defaultSvg (model.Width + logoWidth) 20 [
                defaultG "#000000" [
                    defs [] [ packageGradient ]
                    roundedRect
                        0 0
                        (max 62 (logoWidth + model.Width - (min 50 model.FeedWidth))) 20
                        "#444444"
                    match model.DownloadsWidth with
                    | 0 ->
                        squareRect
                            (logoWidth + model.FeedWidth) 0
                            (model.VersionWidth / 2) 20
                            "#43ba1b"
                        roundedRect
                            (logoWidth + model.FeedWidth) 0
                            (model.VersionWidth) 20
                            "#43ba1b"
                    | _ ->
                        squareRect
                            (logoWidth + model.FeedWidth) 0
                            (model.VersionWidth) 20
                            "#43ba1b"
                    squareRect
                        (logoWidth + model.FeedWidth + model.VersionWidth) 0
                        (model.DownloadsWidth - 10) 20
                        downloadColour
                    roundedRect
                        (logoWidth + model.FeedWidth + model.VersionWidth) 0
                        (model.DownloadsWidth) 20
                        downloadColour
                    roundedRect
                        0 0
                        (logoWidth + model.Width) 20
                        "url(#grad1)"

                    match model.VersionWidth, model.DownloadsWidth with
                    | 0, 0 ->
                        whiteText (logoWidth + model.Padding) 14 model.FeedName
                    | 0, _ ->
                        whiteText (logoWidth + model.Padding) 14 model.FeedName
                        whiteText (logoWidth + model.FeedWidth + model.VersionWidth + model.Padding) 14 model.Downloads
                    | _, 0 ->
                        blackText (logoWidth + model.FeedWidth + model.Padding) 15 model.Version
                        whiteText (logoWidth + model.Padding) 14 model.FeedName
                        whiteText (logoWidth + model.FeedWidth + model.Padding) 14 model.Version
                    | _, _ ->
                        blackText (logoWidth + model.FeedWidth + model.Padding) 15 model.Version
                        whiteText (logoWidth + model.Padding) 14 model.FeedName
                        whiteText (logoWidth + model.FeedWidth + model.Padding) 14 model.Version
                        whiteText (logoWidth + model.FeedWidth + model.VersionWidth + model.Padding) 14 model.Downloads ]
                g [] [
                    logoSvg
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
                    TextSize.chars
                    |> Seq.map (fun kv -> kv.Key, kv.Value)
                    |> Seq.mapFold (fun (x, i) (c, w) ->
                        match isEven i with
                        | true  -> squareRect x 0 w 300 "#ff0000", (x + w, i + 1)
                        | false -> squareRect x 0 w 300 "#00ff00", (x + w, i + 1)) (0, 0)
                    |> fst

                yield!
                    TextSize.chars
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