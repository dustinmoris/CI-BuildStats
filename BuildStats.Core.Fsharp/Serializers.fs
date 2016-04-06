namespace BuildStats.Core.Fsharp

open Newtonsoft.Json

type ISerializer =
    abstract member Deserialize<'T> : string -> 'T
    abstract member Deserialize : string -> obj

type JsonSerializer() =
    interface ISerializer with
        member this.Deserialize<'T> (content : string) =
            JsonConvert.DeserializeObject<'T>(content)
        member this.Deserialize (content : string) =
            JsonConvert.DeserializeObject(content)