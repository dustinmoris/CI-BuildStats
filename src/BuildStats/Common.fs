module BuildStats.Common

open System
open System.IO
open System.Text
open System.Text.RegularExpressions
open System.Net
open System.Net.Http
open System.Security.Cryptography
open Newtonsoft.Json
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Authentication
open FSharp.Control.Tasks.V2.ContextInsensitive
open Polly
open Polly.Extensions.Http
open Giraffe

// -------------------------------------
// String helper functions
// -------------------------------------

[<RequireQualifiedAccess>]
module Str =

    let equals (name1 : string)
                (name2 : string) =
        name1.Equals(name2, StringComparison.CurrentCultureIgnoreCase)

    let toOption str =
        match str with
        | null | "" -> None
        | _         -> Some str

// -------------------------------------
// Serialization
// -------------------------------------

[<RequireQualifiedAccess>]
module Json =

    let serialize (x : obj) =
        JsonConvert.SerializeObject x

    let deserialize (json : string) =
        JsonConvert.DeserializeObject json

// -------------------------------------
// Http
// -------------------------------------

[<RequireQualifiedAccess>]
module Http =

    let getJson (httpClient : HttpClient) (url : string) =
        task {
            let! response = httpClient.GetAsync url
            match response.StatusCode with
            | HttpStatusCode.OK -> return! response.Content.ReadAsStringAsync()
            | _ -> return ""
        }

    let sendRequest (httpClient : HttpClient) (request : HttpRequestMessage) =
        task {
            let! response = httpClient.SendAsync request
            match response.StatusCode with
            | HttpStatusCode.OK -> return! response.Content.ReadAsStringAsync()
            | _ -> return ""
        }

// -------------------------------------
// Cryptography
// -------------------------------------

[<RequireQualifiedAccess>]
module AES =

    let private devKey = Guid.NewGuid().ToString()
    let private ivLength = 16

    let key =
        Environment.GetEnvironmentVariable "CRYPTO_KEY"
        |> Str.toOption
        |> function
            | Some k -> k
            | None   -> devKey

    let private mergeIVandCipher (iv : byte array) (cipher : byte array) =
        let result = Array.init (iv.Length + cipher.Length) (fun _ -> Byte.MinValue)
        Buffer.BlockCopy(iv, 0, result, 0, iv.Length)
        Buffer.BlockCopy(cipher, 0, result, iv.Length, cipher.Length)
        result

    let private splitIntoIVandCipher (buffer : byte array) =
        buffer.[0..ivLength - 1], buffer.[ivLength..buffer.Length - 1]

    let private createRandomBytes (count : int) =
        let result = Array.init count (fun _ -> Byte.MinValue)
        let rnd = RandomNumberGenerator.Create()
        rnd.GetBytes result
        result

    let encrypt (key : byte array) (iv : byte array) (plain : byte array) =
        use aes = Aes.Create(Key = key, IV = iv)
        use encryptor = aes.CreateEncryptor(key, iv)
        encryptor.TransformFinalBlock(plain, 0, plain.Length)

    let decrypt (key : byte array) (iv : byte array) (cipher : byte array) =
        use aes = Aes.Create(Key = key, IV = iv)
        use decryptor = aes.CreateDecryptor(key, iv)
        decryptor.TransformFinalBlock(cipher, 0, cipher.Length)

    let encryptToUrlEncodedString (key : string) (plainText : string) =
        let key'  = Encoding.UTF8.GetBytes key
        let iv    = createRandomBytes ivLength
        let plain = Encoding.UTF8.GetBytes plainText
        encrypt key' iv plain
        |> mergeIVandCipher iv
        |> Base64UrlTextEncoder.Encode

    let decryptUrlEncodedString (key : string) (urlEncodedCipherText : string) =
        urlEncodedCipherText
        |> Base64UrlTextEncoder.Decode
        |> splitIntoIVandCipher
        ||> decrypt (Encoding.UTF8.GetBytes key)
        |> Encoding.UTF8.GetString

[<RequireQualifiedAccess>]
module Hash =

    let md5 (str : string) =
        str
        |> Encoding.UTF8.GetBytes
        |> MD5.Create().ComputeHash
        |> Array.map (fun b -> b.ToString "x2")
        |> String.concat ""

    let sha1 (str : string) =
        str
        |> Encoding.UTF8.GetBytes
        |> SHA1.Create().ComputeHash
        |> Array.map (fun b -> b.ToString "x2")
        |> String.concat ""

// -------------------------------------
// CSS
// -------------------------------------

[<RequireQualifiedAccess>]
module StaticAssets =

    let minifyCss (css : string) =
        let css = css.Replace(Environment.NewLine, "")
        let css = (Regex("\\s*{\\s*")).Replace(css, "{")
        let css = (Regex("\\s*}\\s*")).Replace(css, "}")
        let css = (Regex("\\s*:\\s*")).Replace(css, ":")
        let css = (Regex("\\s*,\\s*")).Replace(css, ",")
        let css = (Regex("\\s*;\\s*")).Replace(css, ";")
        let css = (Regex("\\s*>\\s*")).Replace(css, ">")
        let css = (Regex(";}")).Replace(css, "}")
        css

    let minifyCssFile (fileName : string) =
        fileName
        |> File.ReadAllText
        |> minifyCss

    let bundleCssFiles (fileNames : string list) =
        let result =
            fileNames
            |> List.fold(
                fun (sb : StringBuilder) fileName ->
                    fileName
                    |> minifyCssFile
                    |> sb.AppendLine
            ) (new StringBuilder())
        result.ToString()

// -------------------------------------
// HTTP Client
// -------------------------------------

[<RequireQualifiedAccess>]
module HttpClientConfig =
    let defaultClientName = "DefaultHttpClient"

    let transientHttpErrorPolicy =
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                [
                    TimeSpan.FromSeconds(1.0)
                    TimeSpan.FromSeconds(3.0)
                    TimeSpan.FromSeconds(5.0)
                ])

    let tooManyRequestsPolicy =
        Policy
            .Handle<HttpRequestException>()
            .OrResult(
                fun (msg : HttpResponseMessage) ->
                    msg.StatusCode.Equals StatusCodes.Status429TooManyRequests)
            .CircuitBreakerAsync(2, TimeSpan.FromSeconds 30.0)