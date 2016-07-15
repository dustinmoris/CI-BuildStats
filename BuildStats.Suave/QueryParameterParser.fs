module QueryParameterParser

open System
open Suave

type QueryParameterValue<'T> =
    | Value of 'T
    | NotSet
    | ParsingError of string


let tryParseWith tryParseFunc = tryParseFunc >> function
    | true  , value -> Some value
    | false , _     -> None

let (|Bool |_|) = tryParseWith bool.TryParse
let (|Int32|_|) = tryParseWith Int32.TryParse


let getBool (key : string)
            (ctx : HttpContext) =
    match ctx.request.queryParam key with
    | Choice1Of2 x  ->
        match x with
        | Bool x    -> Value x
        | _         -> ParsingError <| sprintf "Could not parse query parameter \"%s\" into a Boolean value." key
    | _             -> NotSet

let getInt32 (key : string)
                (ctx : HttpContext) =
    match ctx.request.queryParam key with
    | Choice1Of2 value  ->
        match value with
        | Int32 x   -> Value x
        | _         -> ParsingError <| sprintf "Could not parse query parameter \"%s\" into an Int32 value." key
    | _             -> NotSet

let getString (key : string)
                (ctx : HttpContext) =
    match ctx.request.queryParam key with
    | Choice1Of2 value  -> Some value
    | _                 -> None