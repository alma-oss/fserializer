namespace Lmc.Serializer

[<RequireQualifiedAccess>]
module Serialize =
    module private Json =
        open Newtonsoft.Json
        open Newtonsoft.Json.Serialization

        let private options () =
            JsonSerializerSettings (
                ContractResolver =
                    DefaultContractResolver (
                        NamingStrategy = SnakeCaseNamingStrategy()
                    )
            )

        let serialize obj =
            JsonConvert.SerializeObject (obj, options())

        let serializePretty obj =
            let options = options()
            options.Formatting <- Formatting.Indented

            JsonConvert.SerializeObject (obj, options)

    let toJsonPretty: obj -> string = Json.serializePretty
    let toJson: obj -> string = Json.serialize

    open System

    let dateTime (dateTime: DateTime) =
        dateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'")

    let dateTimeOffset (dateTime: DateTimeOffset) =
        dateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'")

    type StringOrNull = string option -> string

    let stringOrNull: StringOrNull = function
        | Some string -> string
        | _ -> null
