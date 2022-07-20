namespace BuildStats

open Microsoft.AspNetCore.Http

[<RequireQualifiedAccess>]
module Network =
    open System
    open System.Net
    open Microsoft.AspNetCore.HttpOverrides

    let tryParseIPAddress (str : string) =
        match IPAddress.TryParse str with
        | true, ipAddress -> Some ipAddress
        | false, _        -> None

    let tryParseNetworkAddress (str : string) =
        let ipAddr, cidrLen =
            match str.Split('/', StringSplitOptions.RemoveEmptyEntries) with
            | arr when arr.Length = 2 -> arr.[0], Some arr.[1]
            | arr -> arr.[0], None

        match IPAddress.TryParse ipAddr with
        | false, _        -> None
        | true, ipAddress ->
            let cidrMask =
                match cidrLen with
                | None     -> None
                | Some len ->
                    match Int32.TryParse len with
                    | true, mask -> Some mask
                    | false, _   -> None
            match cidrMask with
            | Some mask -> Some (IPNetwork(ipAddress, mask))
            | None      -> Some (IPNetwork(ipAddress, 32))

[<AutoOpen>]
module NetworkExtensions =
    open System
    open System.Net
    open System.Collections.Generic
    open System.Threading.Tasks
    open Microsoft.AspNetCore.Builder
    open Microsoft.AspNetCore.HttpOverrides
    open Microsoft.Extensions.DependencyInjection

    type IPNetwork with
        member this.ToPrettyString() =
            sprintf "%s/%s"
                (this.Prefix.ToString())
                (this.PrefixLength.ToString())

    type IPAddress with
        member this.ToPrettyString() =
            this.MapToIPv4().ToString()

    type IEnumerable<'T> with
        member this.ToPrettyString() =
            this
            |> Seq.map (fun t ->
                match box t with
                | :? IPAddress as ip -> ip.ToPrettyString()
                | :? IPNetwork as nw -> nw.ToPrettyString()
                | _ -> t.ToString())
            |> String.concat ", "

    type IServiceCollection with
        member this.AddProxies (proxyCount    : int,
                                proxyNetworks : IPNetwork[],
                                proxies       : IPAddress[]) =

            this.Configure<ForwardedHeadersOptions>(
                fun (cfg : ForwardedHeadersOptions) ->
                    proxyNetworks
                    |> Array.iter cfg.KnownNetworks.Add
                    proxies
                    |> Array.iter cfg.KnownProxies.Add
                    cfg.RequireHeaderSymmetry <- false
                    cfg.ForwardLimit          <- Nullable<int> proxyCount
                    cfg.ForwardedHeaders      <- ForwardedHeaders.All)

    type IApplicationBuilder with
        member this.UseHttpsRedirection (isEnabled : bool, domainName : string) =
            match isEnabled with
            | true ->
                this.Use(
                    Func<HttpContext, RequestDelegate, Task>(
                        fun (ctx : HttpContext) (next : RequestDelegate) ->
                            let host = ctx.Request.Host.Host
                            // Only HTTPS redirect for the chosen domain:
                            let mustUseHttps =
                                host = domainName
                                || host.EndsWith ("." + domainName)
                            // Otherwise prevent the HTTP redirection middleware
                            // to redirect by force setting the scheme to https:
                            if not mustUseHttps then
                                ctx.Request.Scheme  <- "https"
                                ctx.Request.IsHttps <- true
                            next.Invoke(ctx)))
                    .UseHttpsRedirection()
            | false -> this
