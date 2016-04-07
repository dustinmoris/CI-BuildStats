namespace BuildStats.Core.Fsharp

open Newtonsoft.Json

type ISerializer =
    abstract member Deserialize : string -> obj

type JsonSerializer() =
    interface ISerializer with
        member this.Deserialize (content : string) =
            JsonConvert.DeserializeObject(content)