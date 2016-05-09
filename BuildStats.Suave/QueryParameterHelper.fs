module QueryParameterHelper

open System
open Suave

let tryParseWith tryParseFunc = tryParseFunc >> function
        | true  , value -> Some value
        | false , _     -> None

let tryParseBool    = tryParseWith bool.TryParse
let tryParseInt32   = tryParseWith Int32.TryParse

let (|Bool|_|)  = tryParseBool
let (|Int32|_|) = tryParseInt32

type QueryParamValue<'T> =
    | Value of 'T
    | ParsingError

let getBoolFromUrlQuery (ctx : HttpContext) 
                        (key : string) 
                        (defaultValue : bool) =

    match ctx.request.queryParam key with
    | Choice1Of2 value  ->
        match value with
        | Bool x    -> Value x
        | _         -> ParsingError
    | _             -> Value defaultValue

let getInt32FromUrlQuery (ctx : HttpContext) 
                         (key : string) 
                         (defaultValue : int) =

    match ctx.request.queryParam key with
    | Choice1Of2 value  ->
        match value with
        | Int32 x   -> Value x
        | _         -> ParsingError
    | _             -> Value defaultValue