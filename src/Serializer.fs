namespace Lmc.Serializer

[<RequireQualifiedAccess>]
module Serialize =
    module private Json =
        open System.IO
        open Newtonsoft.Json
        open Newtonsoft.Json.Serialization

        let private options () =
            JsonSerializerSettings (
                ContractResolver =
                    DefaultContractResolver (
                        NamingStrategy = SnakeCaseNamingStrategy()
                    ),
                NullValueHandling = NullValueHandling.Include
            )

        let serialize obj =
            JsonConvert.SerializeObject (obj, options())

        let serializePretty obj =
            let options = options()
            options.Formatting <- Formatting.Indented

            use stringWriter = new StringWriter()
            use writer = new JsonTextWriter(stringWriter)
            writer.Indentation <- 4

            let serializer = JsonSerializer.Create(options)
            serializer.Serialize(writer, obj)

            stringWriter.ToString()

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

    module private Hash =
        open System.Security.Cryptography
        open System.Text

        let hashSHA256 (data: string) =
            let toLower (string: string) =
                string.ToLower()

            data
            |> Encoding.ASCII.GetBytes
            |> (SHA256.Create()).ComputeHash
            |> System.BitConverter.ToString
            |> Seq.filter ((<>) '-')
            |> String.Concat
            |> toLower

    let hash = Hash.hashSHA256
