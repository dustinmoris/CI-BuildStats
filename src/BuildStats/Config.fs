namespace BuildStats

[<RequireQualifiedAccess>]
module Config =
    open System
    open System.ComponentModel
    open System.Globalization

    let private strOption str =
        match String.IsNullOrEmpty str with
        | true  -> None
        | false -> Some str

    let private strSplitArray (str : string) =
        str.Split([| ' '; ','; ';' |], StringSplitOptions.RemoveEmptyEntries)

    let private tryConvertFromString<'T when 'T : struct> (cultureInfo : CultureInfo option) (value : string) =
        let culture = defaultArg cultureInfo CultureInfo.CurrentCulture
        let converter = TypeDescriptor.GetConverter (typeof<'T>)
        try Some (converter.ConvertFromString(null, culture, value) :?> 'T)
        with _ -> None

    let environmentVar key =
        Environment.GetEnvironmentVariable key
        |> strOption

    let environmentVarOrDefault key defaultValue =
        environmentVar key
        |> Option.defaultValue defaultValue

    let typedEnvironmentVar<'T when 'T : struct> culture key =
        Environment.GetEnvironmentVariable key
        |> strOption
        |> Option.bind (tryConvertFromString<'T> culture)

    let typedEnvironmentVarOrDefault<'T when 'T : struct> culture key defaultValue =
        typedEnvironmentVar<'T> culture key
        |> Option.defaultValue defaultValue

    let environmentVarList key =
        environmentVar key
        |> function
            | None   -> [||]
            | Some v -> strSplitArray v

    module CurrentCulture =
        let typedEnvironmentVar<'T when 'T : struct> key =
            Environment.GetEnvironmentVariable key
            |> strOption
            |> Option.bind (tryConvertFromString<'T> (Some CultureInfo.CurrentCulture))

        let typedEnvironmentVarOrDefault<'T when 'T : struct> (key : string) defaultValue =
            typedEnvironmentVar<'T> key
            |> Option.defaultValue defaultValue

    module InvariantCulture =
        let typedEnvironmentVar<'T when 'T : struct> key =
            Environment.GetEnvironmentVariable key
            |> strOption
            |> Option.bind (tryConvertFromString<'T> (Some CultureInfo.InvariantCulture))

        let typedEnvironmentVarOrDefault<'T when 'T : struct> (key : string) defaultValue =
            typedEnvironmentVar<'T> key
            |> Option.defaultValue defaultValue