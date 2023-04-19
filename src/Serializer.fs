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

        let serializeWith (options: JsonSerializerSettings) obj =
            JsonConvert.SerializeObject (obj, options)

        let serialize obj =
            obj |> serializeWith (options())

        let private serializePrettyWith (options: JsonSerializerSettings) obj =
            options.Formatting <- Formatting.Indented

            use stringWriter = new StringWriter()
            use writer = new JsonTextWriter(stringWriter)
            writer.Indentation <- 4

            let serializer = JsonSerializer.Create(options)
            serializer.Serialize(writer, obj)

            stringWriter.ToString()

        let serializePretty obj =
            obj |> serializePrettyWith (options())

        let private optionsIgnoringNulls () =
            let options = options ()
            options.NullValueHandling <- NullValueHandling.Ignore
            options

        let serializeIgnoringNulls obj =
            obj |> serializeWith (optionsIgnoringNulls())

        let serializeIgnoringNullsPretty obj =
            obj |> serializePrettyWith (optionsIgnoringNulls())

    let toJsonPretty: obj -> string = Json.serializePretty
    let toJson: obj -> string = Json.serialize

    let toJsonIgnoringNullsPretty: obj -> string = Json.serializeIgnoringNullsPretty
    let toJsonIgnoringNulls: obj -> string = Json.serializeIgnoringNulls

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
