module BuildStats.IpAddressWhitelisting

open System
open System.Text
open System.Net
open System.Net.Http
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Builder
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe.Common

let rec private compareIPAddressWithCIDR (cidrBytes : byte array)
                                         (ipBytes   : byte array)
                                         (bitCount  : int)
                                         (index     : int) =
    match bitCount >= 8 with
    | true ->
        match cidrBytes.[index] = ipBytes.[index] with
        | false -> false
        | true  -> compareIPAddressWithCIDR
                        cidrBytes ipBytes (bitCount - 8) (index + 1)
    | false ->
        match bitCount > 0 with
        | false -> true
        | true  ->
            let mask = (byte)~~~(255 >>> bitCount)
            (cidrBytes.[index] &&& mask) = (ipBytes.[index] &&& mask)

type IPAddressWhitelistOption =
    | SingleAddress of IPAddress
    | CidrNotation  of IPAddress * int

    override this.ToString() =
        match this with
        | SingleAddress ip -> ip.ToString()
        | CidrNotation (ip, bitCount) ->
            sprintf "%s/%i" (ip.ToString()) bitCount

    static member FromString (str : string) =
        if String.IsNullOrEmpty str then
            raise (new ArgumentException("An IP address or CIDR notation cannot be null or empty."))

        let str' = str.Trim()
        match str'.Contains "/" with
        | false -> SingleAddress (IPAddress.Parse str')
        | true  ->
            let parts = str'.Split('/')

            if parts.Length <> 2 then
                raise (new ArgumentException(sprintf "Invalid CIDR notation: %s" str))

            let ipAddress = IPAddress.Parse parts.[0]
            let bitCount  = Convert.ToInt32(parts.[1], 10)

            if (bitCount < 0 || bitCount > 32) then
                raise (new ArgumentException(sprintf "Invalid CIDR notation: %s" str))

            CidrNotation (ipAddress, bitCount)

    member this.Contains (other : IPAddress) =
        if isNull other then nullArg "IP Address cannot be null."
        else
            match this with
            | SingleAddress ipAddress ->
                let validIp  = ipAddress.GetAddressBytes()
                let remoteIp =
                    match other.IsIPv4MappedToIPv6 with
                    | true  -> other.MapToIPv4().GetAddressBytes()
                    | false -> other.GetAddressBytes()

                match validIp.Length <> remoteIp.Length with
                | true  -> false
                | false -> validIp.Equals remoteIp

            | CidrNotation (ipAddress, bitCount) ->
                let cidrIp   = ipAddress.GetAddressBytes()
                let remoteIp =
                    match other.IsIPv4MappedToIPv6 with
                    | true  -> other.MapToIPv4().GetAddressBytes()
                    | false -> other.GetAddressBytes()

                match cidrIp.Length = remoteIp.Length with
                | true  -> compareIPAddressWithCIDR cidrIp remoteIp bitCount 0
                | false -> false


type IpAddressWhitelistMiddleware (next       : RequestDelegate,
                                   allowLocal : bool,
                                   whitelist  : IPAddressWhitelistOption list,
                                   logger     : ILogger<IpAddressWhitelistMiddleware>) =

    do  if isNull next then raise (ArgumentNullException("next"))
        let sb = new StringBuilder()
        sb.AppendLine "IP Whitelist enabled for:" |> ignore
        whitelist
        |> List.fold (fun (sb : StringBuilder) t -> sb.AppendLine (t.ToString())) sb
        |> (fun sb -> logger.LogInformation (sb.ToString()))

    member __.Invoke (ctx : HttpContext) =
        task {
            let localIpAddress  = ctx.Connection.LocalIpAddress
            let remoteIpAddress = ctx.Connection.RemoteIpAddress

            let isSet   = isNotNull remoteIpAddress && remoteIpAddress.ToString() <> "::1"
            let isLocal =
                match isSet with
                | true  -> remoteIpAddress.Equals localIpAddress
                | false -> IPAddress.IsLoopback remoteIpAddress

            let isAllowed =
                (isLocal && allowLocal) ||
                whitelist |> List.exists (fun x -> x.Contains remoteIpAddress)

            return!
                if isAllowed
                then next.Invoke ctx
                else
                    logger.LogWarning(sprintf "Request from '%s' has been denied." (remoteIpAddress.ToString()))
                    ctx.Response.StatusCode <- StatusCodes.Status403Forbidden
                    ctx.Response.WriteAsync "Access denied. Please use https://buildstats.info to access this API."
        }

type IApplicationBuilder with
    member this.UseIpAddressWhitelist (allowLocal : bool, whitelist : IPAddressWhitelistOption list) =
        this.UseMiddleware<IpAddressWhitelistMiddleware> (whitelist, allowLocal)

    member this.UseCloudflareIpAddressWhitelist (allowLocal       : bool,
                                                 ipv4WhitelistUrl : string option,
                                                 ipv6WhitelistUrl : string option) =
        let ipv4Url = defaultArg ipv4WhitelistUrl "https://www.cloudflare.com/ips-v4"
        let ipv6Url = defaultArg ipv6WhitelistUrl "https://www.cloudflare.com/ips-v6"
        let ipv4Whitelist =
            ((new HttpClient())
                .GetStringAsync ipv4Url)
                .Result
                .Split(Environment.NewLine)
            |> Array.filter (fun l -> not (String.IsNullOrEmpty l))
            |> Array.map IPAddressWhitelistOption.FromString
            |> Array.toList
        let ipv6Whitelist =
            ((new HttpClient())
                .GetStringAsync ipv6Url)
                .Result
                .Split(Environment.NewLine)
            |> Array.filter (fun l -> not (String.IsNullOrEmpty l))
            |> Array.map IPAddressWhitelistOption.FromString
            |> Array.toList
        this.UseIpAddressWhitelist(allowLocal, ipv4Whitelist @ ipv6Whitelist)