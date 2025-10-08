namespace Alma.Serializer

[<RequireQualifiedAccess>]
module Serialize =
    type SerializerOptions =
        | Pretty
        | IgnoringNulls

    module private Json =
        open System.IO
        open Newtonsoft.Json
        open Newtonsoft.Json.Serialization

        [<RequireQualifiedAccess>]
        module Options =
            let create () =
                JsonSerializerSettings (
                    ContractResolver =
                        DefaultContractResolver (
                            NamingStrategy = SnakeCaseNamingStrategy()
                        ),
                    NullValueHandling = NullValueHandling.Include
                )

            let map f (options: JsonSerializerSettings) =
                f options

            let withIgnoringNulls = map (fun opts ->
                    opts.NullValueHandling <- NullValueHandling.Ignore
                    opts
                )

            let withPretty = map (fun opts ->
                opts.Formatting <- Formatting.Indented
                opts
            )

        let createSerializer options =
            options
            |> List.fold (fun opts -> function
                | Pretty -> Options.withPretty opts
                | IgnoringNulls -> Options.withIgnoringNulls opts
            ) (Options.create())
            |> JsonSerializer.Create

        let private serializeWithSerializer (serializer: JsonSerializer) indentation (obj: obj) =
            use stringWriter = new StringWriter()

            use writer = new JsonTextWriter(stringWriter)
            match indentation with
            | Some indent -> writer.Indentation <- indent
            | None -> ()

            serializer.Serialize(writer, obj)
            stringWriter.ToString()

        let private serializeWith options = serializeWithSerializer (createSerializer options) None
        let private serializePrettyWith options = serializeWithSerializer (createSerializer (Pretty :: options)) (Some 4)

        let serialize = serializeWith []
        let serializePretty = serializePrettyWith []

        let serializeIgnoringNulls = serializeWith [ IgnoringNulls ]
        let serializeIgnoringNullsPretty = serializePrettyWith [ IgnoringNulls ]

    let createSerializer = Json.createSerializer

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
            | JsonValue.Number number -> number |> int64 :> obj
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
            | JsonValue.Number number -> number |> int64 :> obj
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
