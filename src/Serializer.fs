namespace Alma.Serializer

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

            // fsharplint:disable RedundantNewKeyword
            use stringWriter = new StringWriter()
            use writer = new JsonTextWriter(stringWriter)
            // fsharplint:enable
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

    [<RequireQualifiedAccess>]
    module JsonValue =
        open FSharp.Data
        open System.Collections.Generic

        /// Dictionary is used to maintain the order of added items - not sorting the keys like Map does.
        let private dictionaryOfArray array =
            array
            |> Array.fold
                (fun (acc: Dictionary<string, _>) (key, value) ->
                    acc.Add(key, value)
                    acc
                )
                (new Dictionary<string, _>())

        let rec toSerializableJson: JsonValue -> obj = function
            | JsonValue.String string -> string :> obj
            | JsonValue.Number number -> number |> int :> obj
            | JsonValue.Float float -> float :> obj
            | JsonValue.Boolean bool -> bool :> obj
            | JsonValue.Record properties ->
                properties
                |> Array.map (fun (key, value) -> key, value |> toSerializableJson)
                |> dictionaryOfArray
                :> obj
            | JsonValue.Array values ->
                values
                |> Array.map toSerializableJson
                :> obj
            | JsonValue.Null -> null

        let rec toSerializableJsonIgnoringNullsInRecord: JsonValue -> obj = function
            | JsonValue.String string -> string :> obj
            | JsonValue.Number number -> number |> int :> obj
            | JsonValue.Float float -> float :> obj
            | JsonValue.Boolean bool -> bool :> obj
            | JsonValue.Record properties ->
                // Json serializer ignores nulls in objects (records), but not in maps, dictionaries, etc. So we need to ignore them manually.
                properties
                |> Array.choose (function
                    | _, JsonValue.Null -> None
                    | key, value -> Some (key, value |> toSerializableJsonIgnoringNullsInRecord)
                )
                |> dictionaryOfArray
                :> obj
            | JsonValue.Array values ->
                values
                |> Array.map toSerializableJsonIgnoringNullsInRecord
                :> obj
            | JsonValue.Null -> null

    [<RequireQualifiedAccess>]
    module JsonElement =
        open System.Text.Json
        open FSharp.Data

        let private serializeAsJsonValue (serialize: JsonValue -> obj) (element: JsonElement) =
            match element.Deserialize() with
            | null -> null
            | element ->
                let rawJson = element.ToString()
                try
                    rawJson
                    |> JsonValue.Parse
                    |> serialize
                with
                | _ -> rawJson

        let toSerializableJson: JsonElement -> obj =
            serializeAsJsonValue JsonValue.toSerializableJson

        let toSerializableJsonIgnoringNullsInRecord: JsonElement -> obj =
            serializeAsJsonValue JsonValue.toSerializableJsonIgnoringNullsInRecord

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
