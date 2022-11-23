namespace Camara.RestAPI

open System.Text.Json
open System.Text.Json.Serialization

module Serialization =
    let options = JsonSerializerOptions()
    options.Converters.Add(JsonFSharpConverter())

    let deserialize (json: string) : 'T =
        JsonSerializer.Deserialize<'T>(json, options)
