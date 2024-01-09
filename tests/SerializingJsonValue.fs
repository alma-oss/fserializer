module Serializer.SerializingJsonValue

open System.IO
open Expecto
open FSharp.Data
open Serializer.TestUtils

open Alma.Serializer

let expected fileName = $"Fixtures/Serializing/{fileName}" |> File.ReadAllText

let okOrFail = function
    | Ok ok -> ok
    | Error error -> failtestf "Fail on %A" error

type JsonValueTestCase = {
    Value: JsonValue
    Expected: string
    Description: string
}

let provideJsonValue =
    [
        {
            Description = "string"
            Value = JsonValue.String "some string value"
            Expected = "\"some string value\""
        }
        {
            Description = "int"
            Value = JsonValue.Number (decimal 66)
            Expected = "66"
        }
        {
            Description = "float as number"
            Value = JsonValue.Number (decimal 66.6)
            Expected = "66"
        }
        {
            Description = "float"
            Value = JsonValue.Float 66.6
            Expected = "66.6"
        }
        {
            Description = "bool - true"
            Value = JsonValue.Boolean true
            Expected = "true"
        }
        {
            Description = "bool - false"
            Value = JsonValue.Boolean false
            Expected = "false"
        }
        {
            Description = "null"
            Value = JsonValue.Null
            Expected = "null"
        }
        {
            Description = "array of different json values"
            Value = JsonValue.Array [|
                JsonValue.Null
                JsonValue.Boolean true
                JsonValue.String "string"
                JsonValue.Number (decimal 123)
                JsonValue.Float 66.6
                JsonValue.Array [| JsonValue.Number (decimal 1); JsonValue.Number (decimal 2); JsonValue.Number (decimal 3) |]
                JsonValue.Record [| "fst", JsonValue.String "one"; "snd", JsonValue.String "two" |]
            |]
            Expected = expected "json-value-array-of-different-values.json"
        }
        {
            Description = "DTO with null"
            Value = JsonValue.Record [|
                "id", JsonValue.Number (decimal 42)
                "name", JsonValue.String "Foo"
                "domain_data", JsonValue.Record [|
                    "type", JsonValue.String "with null"
                    "value", JsonValue.Null
                |]
            |]
            Expected = expected "json-value-dto-with-null.json"
        }
        {
            Description = "DTO with value"
            Value = JsonValue.Record [|
                "id", JsonValue.Number (decimal 42)
                "name", JsonValue.String "Foo"
                "domain_data", JsonValue.Record [|
                    "type", JsonValue.String "with value"
                    "value", JsonValue.String "value"
                |]
            |]
            Expected = expected "json-value-dto-with-value.json"
        }
        {
            Description = "DTO with int64 value"
            Value = JsonValue.Record [|
                "id", JsonValue.Number (1234567890 |> decimal)  // int
                "name", JsonValue.String "Foo"
                "domain_data", JsonValue.Record [|
                    "type", JsonValue.String "int64"
                    "value", JsonValue.Number (123456789011L |> decimal) // int64
                |]
            |]
            Expected = expected "json-value-dto-with-int64-value.json"
        }
    ]

let provideJsonValueIgnoringNulls =
    [
        {
            Description = "array of different json values"
            Value = JsonValue.Array [|
                JsonValue.Null
                JsonValue.Boolean true
                JsonValue.String "string"
                JsonValue.Number (decimal 123)
                JsonValue.Float 66.6
                JsonValue.Array [| JsonValue.Number (decimal 1); JsonValue.Number (decimal 2); JsonValue.Number (decimal 3) |]
                JsonValue.Record [| "fst", JsonValue.String "one"; "snd", JsonValue.String "two"; "null-should-be-ignored", JsonValue.Null |]
            |]
            Expected = expected "json-value-array-of-different-values.json"
        }
        {
            Description = "DTO with null"
            Value = JsonValue.Record [|
                "id", JsonValue.Number (decimal 42)
                "name", JsonValue.String "Foo"
                "domain_data", JsonValue.Record [|
                    "type", JsonValue.String "with ignored null"
                    "value", JsonValue.Null
                |]
            |]
            Expected = expected "json-value-dto-with-ignored-null.json"
        }
        {
            Description = "DTO with value"
            Value = JsonValue.Record [|
                "id", JsonValue.Number (decimal 42)
                "name", JsonValue.String "Foo"
                "domain_data", JsonValue.Record [|
                    "type", JsonValue.String "with value"
                    "value", JsonValue.String "value"
                |]
            |]
            Expected = expected "json-value-dto-with-value.json"
        }
        {
            Description = "DTO with int64 value"
            Value = JsonValue.Record [|
                "id", JsonValue.Number (1234567890 |> decimal)  // int
                "name", JsonValue.String "Foo"
                "domain_data", JsonValue.Record [|
                    "type", JsonValue.String "int64"
                    "value", JsonValue.Number (123456789011L |> decimal) // int64
                |]
            |]
            Expected = expected "json-value-dto-with-int64-value.json"
        }
    ]

[<Tests>]
let serializeJsonValueTest =
    testList "Serializer - serialize JsonValue" [
        testCase "DTO to json" <| fun _ ->
            provideJsonValue
            |> List.iter (fun ({ Value = event; Expected = expected; Description = description }) ->
                let serialized =
                    event
                    |> Serialize.JsonValue.toSerializableJson
                    |> Serialize.toJson

                Expect.hasLength (serialized.Split "\n") 1 "Serialized json should have a single line."

                serialized
                |> Actual
                |> assertJsonEquals id description (Expected expected)
            )

        testCase "DTO to json pretty" <| fun _ ->
            provideJsonValue
            |> List.iter (fun ({ Value = event; Expected = expected; Description = description }) ->
                let serialized =
                    event
                    |> Serialize.JsonValue.toSerializableJson
                    |> Serialize.toJsonPretty

                Expect.equal serialized expected description
            )

        testCase "DTO to json ignoring nulls" <| fun _ ->
            provideJsonValueIgnoringNulls
            |> List.iter (fun ({ Value = event; Expected = expected; Description = description }) ->
                let serialized =
                    event
                    |> Serialize.JsonValue.toSerializableJsonIgnoringNullsInRecord
                    |> Serialize.toJsonIgnoringNulls

                Expect.hasLength (serialized.Split "\n") 1 "Serialized json should have a single line."

                serialized
                |> Actual
                |> assertJsonEquals id description (Expected expected)
            )

        testCase "DTO to json pretty ignoring nulls" <| fun _ ->
            provideJsonValueIgnoringNulls
            |> List.iter (fun ({ Value = event; Expected = expected; Description = description }) ->
                let serialized =
                    event
                    |> Serialize.JsonValue.toSerializableJsonIgnoringNullsInRecord
                    |> Serialize.toJsonIgnoringNullsPretty

                Expect.equal serialized expected description
            )
    ]
