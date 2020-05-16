namespace BuildStats

// -------------------------------------
// String helper functions
// -------------------------------------

[<RequireQualifiedAccess>]
module Str =
    open System

    let private ignoreCase = StringComparison.InvariantCultureIgnoreCase

    let equalsCi (str1 : string) str2 = str1.Equals(str2, ignoreCase)

    let toOption str =
        match str with
        | null | "" -> None
        | _         -> Some str

// -------------------------------------
// Serialization
// -------------------------------------

[<RequireQualifiedAccess>]
module Json =
    open Newtonsoft.Json

    let serialize   (x    : obj)    = JsonConvert.SerializeObject x
    let deserialize (json : string) = JsonConvert.DeserializeObject json

// -------------------------------------
// Cryptography
// -------------------------------------

[<RequireQualifiedAccess>]
module AES =
    open System
    open System.Text
    open System.Security.Cryptography
    open Microsoft.AspNetCore.Authentication

    let private ivLength = 16

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
    open System.Text
    open System.Security.Cryptography

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
module Css =
    open System.IO
    open System.Text
    open NUglify

    let private getErrorMsg (errors : UglifyError seq) =
        let msg =
            errors
            |> Seq.fold (fun (sb : StringBuilder) t ->
                sprintf "Error: %s, File: %s" t.Message t.File
                |> sb.AppendLine
            ) (new StringBuilder("Couldn't uglify content."))
        msg.ToString()

    let minify (css : string) =
        css
        |> Uglify.Css
        |> (fun res ->
            match res.HasErrors with
            | true  -> failwith (getErrorMsg res.Errors)
            | false -> res.Code)

    let getMinifiedContent (fileName : string) =
        fileName
        |> File.ReadAllText
        |> minify

    let getBundledContent (fileNames : string list) =
        let result =
            fileNames
            |> List.fold(
                fun (sb : StringBuilder) fileName ->
                    fileName
                    |> getMinifiedContent
                    |> sb.AppendLine
            ) (new StringBuilder())
        result.ToString()