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
