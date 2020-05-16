namespace BuildStats

[<RequireQualifiedAccess>]
module TextSize =
    let chars =
        dict [
            'a', 7
            'b', 7
            'c', 6
            'd', 7
            'e', 7
            'f', 3
            'g', 7
            'h', 7
            'i', 3
            'j', 3
            'k', 6
            'l', 3
            'm', 10
            'n', 7
            'o', 7
            'p', 7
            'q', 7
            'r', 4
            's', 6
            't', 3
            'u', 7
            'v', 6
            'w', 9
            'x', 6
            'y', 6
            'z', 6

            'A', 8
            'B', 8
            'C', 9
            'D', 9
            'E', 8
            'F', 8
            'G', 9
            'H', 9
            'I', 3
            'J', 6
            'K', 8
            'L', 7
            'M', 10
            'N', 9
            'O', 10
            'P', 8
            'Q', 10
            'R', 9
            'S', 8
            'T', 8
            'U', 9
            'V', 8
            'W', 12
            'X', 8
            'Y', 8
            'Z', 8

            '0', 7
            '1', 7
            '2', 7
            '3', 7
            '4', 7
            '5', 7
            '6', 7
            '7', 7
            '8', 7
            '9', 7

            '-', 4
            '_', 7
            '~', 7
            '.', 3
            '!', 4
            '*', 5
            ''', 3
            '(', 4
            ')', 4
            '[', 4
            ']', 4
            ';', 3
            ':', 3
            '@', 12
            '=', 7
            '+', 7
            '$', 7
            ',', 3
            '#', 7
            '?', 7
            '/', 4
            '&', 8

            '▾', 11
            ' ', 7
        ]

    let measureWidth (text : string) =
        text.ToCharArray()
        |> Array.fold (fun w c ->
            let charWidth =
                match chars.ContainsKey c with
                | true  -> chars.[c]
                | false -> 7
            w + charWidth) 0