module Serializer.SerializingJsonElement

open System.IO
open System.Text.Json
open Expecto
open Serializer.TestUtils

open Alma.Serializer

let expected fileName = $"Fixtures/Serializing/{fileName}" |> File.ReadAllText
let jsonElement (json: string) =
    try (json |> JsonDocument.Parse).RootElement
    with e -> failwithf "Failed to parse json: %s\nError: %s" json e.Message

let okOrFail = function
    | Ok ok -> ok
    | Error error -> failtestf "Fail on %A" error

type JsonElementTestCase = {
    Value: JsonElement
    Expected: string
    Description: string
}

let provideJsonElement =
    [
        {
            Description = "string"
            Value = jsonElement @"{""value"": ""some string value""}"
            Expected = @"{
    ""value"": ""some string value""
}"
        }
        {
            Description = "int"
            Value = jsonElement @"{""value"": 66}"
            Expected = @"{
    ""value"": 66
}"
        }
        {
            Description = "float as string"
            Value = jsonElement @"{""value"": ""66.6""}"
            Expected = @"{
    ""value"": ""66.6""
}"
        }
        {
            Description = "bool - true"
            Value = jsonElement @"{""value"": true}"
            Expected = @"{
    ""value"": true
}"
        }
        {
            Description = "bool - false"
            Value = jsonElement @"{""value"": false}"
            Expected = @"{
    ""value"": false
}"
        }
        {
            Description = "null"
            Value = jsonElement @"{""value"": null}"
            Expected = @"{
    ""value"": null
}"
        }
        {
            Description = "array of different json values"
            Value = jsonElement "[null, true, \"string\", 123, 66, [1, 2, 3], {\"fst\": \"one\", \"snd\": \"two\"}]"    // todo - cant handle float 66.6 in array
            Expected = expected "json-element-array-of-different-values.json"
        }
        {
            Description = "DTO with null"
            Value = jsonElement "{ \"id\": 42, \"name\": \"Foo\", \"domain_data\": { \"type\": \"with null\", \"value\": null } }"
            Expected = expected "json-value-dto-with-null.json"
        }
        {
            Description = "DTO with value"
            Value = jsonElement "{ \"id\": 42, \"name\": \"Foo\", \"domain_data\": { \"type\": \"with value\", \"value\": \"value\" } }"
            Expected = expected "json-value-dto-with-value.json"
        }
    ]

let provideJsonElementIgnoringNulls =
    [
        {
            Description = "array of different json values"
            Value = jsonElement "[null, true, \"string\", 123, 66, [1, 2, 3], {\"fst\": \"one\", \"snd\": \"two\", \"null-should-be-ignored\": null}]"      // todo - cant handle float 66.6 in array
            Expected = expected "json-element-array-of-different-values.json"
        }
        {
            Description = "DTO with null"
            Value = jsonElement "{ \"id\": 42, \"name\": \"Foo\", \"domain_data\": { \"type\": \"with ignored null\", \"value\": null } }"
            Expected = expected "json-value-dto-with-ignored-null.json"
        }
        {
            Description = "DTO with value"
            Value = jsonElement "{ \"id\": 42, \"name\": \"Foo\", \"domain_data\": { \"type\": \"with value\", \"value\": \"value\" } }"
            Expected = expected "json-value-dto-with-value.json"
        }
    ]

[<Tests>]
let serializeJsonElementTest =
    testList "Serializer - serialize JsonElement" [
        testCase "DTO to json" <| fun _ ->
            provideJsonElement
            |> List.iter (fun ({ Value = event; Expected = expected; Description = description }) ->
                let serialized =
                    event
                    |> Serialize.JsonElement.toSerializableJson
                    |> Serialize.toJson

                Expect.hasLength (serialized.Split "\n") 1 "Serialized json should have a single line."

                serialized
                |> Actual
                |> assertJsonEquals id description (Expected expected)
            )

        testCase "DTO to json pretty" <| fun _ ->
            provideJsonElement
            |> List.iter (fun ({ Value = event; Expected = expected; Description = description }) ->
                let serialized =
                    event
                    |> Serialize.JsonElement.toSerializableJson
                    |> Serialize.toJsonPretty

                Expect.equal serialized expected description
            )

        testCase "DTO to json ignoring nulls" <| fun _ ->
            provideJsonElementIgnoringNulls
            |> List.iter (fun ({ Value = event; Expected = expected; Description = description }) ->
                let serialized =
                    event
                    |> Serialize.JsonElement.toSerializableJsonIgnoringNullsInRecord
                    |> Serialize.toJsonIgnoringNulls

                Expect.hasLength (serialized.Split "\n") 1 "Serialized json should have a single line."

                serialized
                |> Actual
                |> assertJsonEquals id description (Expected expected)
            )

        testCase "DTO to json pretty ignoring nulls" <| fun _ ->
            provideJsonElementIgnoringNulls
            |> List.iter (fun ({ Value = event; Expected = expected; Description = description }) ->
                let serialized =
                    event
                    |> Serialize.JsonElement.toSerializableJsonIgnoringNullsInRecord
                    |> Serialize.toJsonIgnoringNullsPretty

                Expect.equal serialized expected description
            )
    ]
