module Serializers

open Newtonsoft.Json

let serializeJson obj =
    JsonConvert.SerializeObject(obj)

let deserializeJson(json : string) =
    JsonConvert.DeserializeObject(json)