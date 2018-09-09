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

type IPAddressRange =
    | SingleAddress of IPAddress
    | CIDR          of IPAddress * int

    override this.ToString() =
        match this with
        | SingleAddress ip    -> ip.ToString()
        | CIDR (ip, bitCount) -> sprintf "%s/%i" (ip.ToString()) bitCount

    static member FromString (str : string) =
        if String.IsNullOrEmpty str then
            raise (new ArgumentException("An IP address or CIDR cannot be null or an empty string."))

        let str' = str.Trim()
        match str'.Contains "/" with
        | false -> SingleAddress (IPAddress.Parse str')
        | true  ->
            let parts = str'.Split('/')

            if parts.Length <> 2 then
                raise (
                    new ArgumentException(
                        sprintf "Invalid CIDR: %s" str'))

            let ip   = IPAddress.Parse parts.[0]
            let bitCount = Int32.Parse parts.[1]

            if (bitCount < 0 || bitCount > 32) then
                raise (new ArgumentException(sprintf "Invalid CIDR: %s" str'))

            CIDR (ip, bitCount)

    member this.Contains (other : IPAddress) =
        match this with
        | SingleAddress ipAddress -> ipAddress.Equals other
        | CIDR (ipAddress, bitCount) ->
            let ip      = BitConverter.ToInt32(ipAddress.GetAddressBytes(), 0)
            let ip2     = BitConverter.ToInt32(other.GetAddressBytes(), 0)
            let netmask = IPAddress.HostToNetworkOrder(-1 <<< (32 - bitCount))
            (ip &&& netmask) = (ip2 &&& netmask)


type IpAddressWhitelistMiddleware (next      : RequestDelegate,
                                   whitelist : IPAddressRange list,
                                   logger    : ILogger<IpAddressWhitelistMiddleware>) =

    do  if isNull next then raise (ArgumentNullException("next"))
        let sb = new StringBuilder()
        sb.AppendLine "IP Whitelist enabled for:" |> ignore
        whitelist
        |> List.fold (fun (sb : StringBuilder) t -> sb.AppendLine (t.ToString())) sb
        |> (fun sb -> logger.LogInformation (sb.ToString()))

    member __.Invoke (ctx : HttpContext) =
        task {
            let ipAddress = ctx.Connection.RemoteIpAddress
            let isAllowed =
                whitelist
                |> List.exists (fun range -> range.Contains ipAddress)

            return!
                if isAllowed
                then next.Invoke ctx
                else
                    logger.LogInformation(sprintf "Forbidden request from %s" (ipAddress.ToString()))
                    ctx.Response.StatusCode <- StatusCodes.Status403Forbidden
                    ctx.Response.WriteAsync "You are not authorized to access this endpoint."
        }

type IApplicationBuilder with
    member this.UseIpAddressWhitelist (whitelist : IPAddressRange list) =
        this.UseMiddleware<IpAddressWhitelistMiddleware> whitelist

    member this.UseCloudflareIpAddressWhitelist (whitelistUrl : string option) =
        let url = defaultArg whitelistUrl "https://www.cloudflare.com/ips-v4"
        ((new HttpClient())
            .GetStringAsync url)
            .Result
            .Split(Environment.NewLine)
        |> Array.filter (fun l -> not (String.IsNullOrEmpty l))
        |> Array.map IPAddressRange.FromString
        |> Array.toList
        |> this.UseIpAddressWhitelist