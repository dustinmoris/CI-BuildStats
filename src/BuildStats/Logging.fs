namespace BuildStats

[<RequireQualifiedAccess>]
module Logging =
    open System.Text
    open System.Collections.Generic
    open Logfella

    let outputEnvironmentSummary (summary : IDictionary<string, IDictionary<string, string>>) =
        let categories = summary.Keys |> Seq.toList
        let keyLength =
            categories
            |> List.fold(
                fun (len : int) (category : string) ->
                    summary.[category].Keys
                    |> Seq.toList
                    |> List.map(fun k -> k.Length)
                    |> List.sortByDescending (fun l -> l)
                    |> List.head
                    |> max len
            ) 0
        let output =
            (categories
            |> List.fold(
                fun (sb : StringBuilder) (category : string) ->
                    summary.[category]
                    |> Seq.fold(
                        fun (sb : StringBuilder) (kvp) ->
                            let key = kvp.Key.PadLeft(keyLength, ' ')
                            let value = kvp.Value
                            sprintf "%s : %s" key value
                            |> sb.AppendLine
                    ) (sb.AppendLine("")
                         .AppendLine((sprintf "%s :" (category.ToUpper())).PadLeft(keyLength + 2, ' '))
                         .AppendLine("-----".PadRight(keyLength + 2, '-')))
            ) (StringBuilder()
                .AppendLine("")
                .AppendLine("")
                .AppendLine("..:: Environment Summary ::..")))
                .ToString()
        Log.Notice(
            output,
            ("categoryName", "startupInfo" :> obj))


[<AutoOpen>]
module LoggingExtensions =
    open System
    open Microsoft.AspNetCore.Hosting

    let private secretMask = "******"

    type String with
        member this.ToSecret() =
            match this with
            | str when String.IsNullOrEmpty str -> ""
            | str when str.Length <= 10 -> secretMask
            | str -> str.Substring(0, str.Length / 2) + secretMask

    type Option<'T> with
        member this.ToSecret() =
            match this with
            | None     -> ""
            | Some obj -> obj.ToString().ToSecret()

    type IWebHostBuilder with
        member this.ConfigureSentry (sentryDsn : string option,
                                     environmentName : string,
                                     appVersion : string) =

            match sentryDsn with
            | None -> this
            | Some dsn ->
                this.UseSentry(
                    fun sentry ->
                        sentry.Debug            <- false
                        sentry.Environment      <- environmentName
                        sentry.Release          <- appVersion
                        sentry.AttachStacktrace <- true
                        sentry.Dsn              <- dsn)