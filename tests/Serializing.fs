module Serializer.Serializing

open System.IO
open Expecto
open FSharp.Data
open Serializer.TestUtils

open Lmc.Serializer

let expected fileName = $"Fixtures/Serializing/{fileName}" |> File.ReadAllText

let okOrFail = function
    | Ok ok -> ok
    | Error error -> failtestf "Fail on %A" error

module Domain =
    type Id = Id of int

    type Event = {
        Id: Id
        Name: string
        DomainData: DomainData
    }

    and DomainData = {
        Type: Type
        Value: string option
    }

    and Type =
        | WithValue
        | WithNull
        | WithIgnoredNull

    type Dto = {
        Id: int
        Name: string
        DomainData: DomainDataDto
    }

    and DomainDataDto = {
        Type: string
        Value: string
    }

    let toDto ({ Id = Id id } as event: Event): Dto =
        {
            Id = id
            Name = event.Name
            DomainData = {
                Type =
                    match event.DomainData.Type with
                    | WithValue -> "with value"
                    | WithNull -> "with null"
                    | WithIgnoredNull -> "with ignored null"
                Value = event.DomainData.Value |> Serialize.stringOrNull
            }
        }

type ProvidedData = {
    Event: Domain.Event
    Expected: string
    Description: string
}

let provideDtos =
    [
        {
            Description = "DTO with null"
            Event = {
                Id = Domain.Id 42
                Name = "Foo"
                DomainData = {
                    Type = Domain.Type.WithNull
                    Value = None
                }
            }
            Expected = expected "dto-with-null.json"
        }
        {
            Description = "DTO with value"
            Event = {
                Id = Domain.Id 42
                Name = "Foo"
                DomainData = {
                    Type = Domain.Type.WithValue
                    Value = Some "value"
                }
            }
            Expected = expected "dto-with-value.json"
        }
    ]

let provideDtosIgnoringNulls =
    [
        {
            Description = "DTO with null"
            Event = {
                Id = Domain.Id 42
                Name = "Foo"
                DomainData = {
                    Type = Domain.Type.WithIgnoredNull
                    Value = None
                }
            }
            Expected = expected "dto-with-ignored-null.json"
        }
        {
            Description = "DTO with value"
            Event = {
                Id = Domain.Id 42
                Name = "Foo"
                DomainData = {
                    Type = Domain.Type.WithValue
                    Value = Some "value"
                }
            }
            Expected = expected "dto-with-value.json"
        }
    ]

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
    ]

[<Tests>]
let serializeTest =
    testList "Serializer - serialize" [
        testCase "DTO to json" <| fun _ ->
            provideDtos
            |> List.iter (fun ({ Event = event; Expected = expected; Description = description }) ->
                let serialized =
                    event
                    |> Domain.toDto
                    |> Serialize.toJson

                Expect.hasLength (serialized.Split "\n") 1 "Serialized json should have a single line."

                serialized
                |> Actual
                |> assertJsonEquals id description (Expected expected)
            )

        testCase "DTO to json pretty" <| fun _ ->
            provideDtos
            |> List.iter (fun ({ Event = event; Expected = expected; Description = description }) ->
                let serialized =
                    event
                    |> Domain.toDto
                    |> Serialize.toJsonPretty

                Expect.equal serialized expected description
            )

        testCase "DTO to json ignoring nulls" <| fun _ ->
            provideDtosIgnoringNulls
            |> List.iter (fun ({ Event = event; Expected = expected; Description = description }) ->
                let serialized =
                    event
                    |> Domain.toDto
                    |> Serialize.toJsonIgnoringNulls

                Expect.hasLength (serialized.Split "\n") 1 "Serialized json should have a single line."

                serialized
                |> Actual
                |> assertJsonEquals id description (Expected expected)
            )

        testCase "DTO to json pretty ignoring nulls" <| fun _ ->
            provideDtosIgnoringNulls
            |> List.iter (fun ({ Event = event; Expected = expected; Description = description }) ->
                let serialized =
                    event
                    |> Domain.toDto
                    |> Serialize.toJsonIgnoringNullsPretty

                Expect.equal serialized expected description
            )

        testCase "JsonValue DTO to json" <| fun _ ->
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

        testCase "JsonValue DTO to json pretty" <| fun _ ->
            provideJsonValue
            |> List.iter (fun ({ Value = event; Expected = expected; Description = description }) ->
                let serialized =
                    event
                    |> Serialize.JsonValue.toSerializableJson
                    |> Serialize.toJsonPretty

                Expect.equal serialized expected description
            )

        testCase "JsonValue DTO to json ignoring nulls" <| fun _ ->
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

        testCase "JsonValue DTO to json pretty ignoring nulls" <| fun _ ->
            provideJsonValueIgnoringNulls
            |> List.iter (fun ({ Value = event; Expected = expected; Description = description }) ->
                let serialized =
                    event
                    |> Serialize.JsonValue.toSerializableJsonIgnoringNullsInRecord
                    |> Serialize.toJsonIgnoringNullsPretty

                Expect.equal serialized expected description
            )
    ]
