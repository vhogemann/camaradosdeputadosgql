namespace Camara.RestAPI

module Serialization =
    open System.Text.Json
    open System.Text.Json.Serialization
    
    let options = JsonSerializerOptions()
    options.Converters.Add(JsonFSharpConverter())

    let deserialize (json: string) : 'T =
        JsonSerializer.Deserialize<'T>(json, options)

    // let obj:YourType = "{ .. jsonString .. }" |> deserialize