module Serializer.Serializing

open System.IO
open Expecto
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
    ]
