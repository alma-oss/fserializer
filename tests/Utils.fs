module Serializer.TestUtils

open Expecto
open Alma.Serializer

type AssertJson =
    | Expected of string
    | Actual of string

let assertJsonEquals normalize description jsonA jsonB =
    let splitLines (string: string) =
        let normalizedJson =
            try Serialize.toJsonPretty(Newtonsoft.Json.JsonConvert.DeserializeObject(string))
            with e -> failtestf "Given string is not valid json: %A\nGiven string:\n%s\n" e string

        normalizedJson.Split("\n")

    let normalize serialized =
        serialized
        |> normalize
        |> splitLines
        |> Seq.map (fun s -> s.Trim())
        |> Seq.toList

    let description, actualLines, expectedLines =
        match (jsonA, jsonB) with
        | Expected expected, Actual actual
        | Actual actual, Expected expected ->
            let normalizedActual = actual |> normalize
            let normalizedExpected = expected |> normalize

            let data =
                normalizedExpected
                |> List.mapi (fun i expected ->
                    sprintf "Exp: %s\nAct: %s\n"
                        expected
                        (normalizedActual |> List.tryItem i |> Option.defaultValue "")
                )
                |> String.concat "\n"

            sprintf "%s.\nActual: %s\n---\n%s\n---\n" description actual data,
            normalizedActual,
            normalizedExpected
        | _ -> failtestf "You have to pass exactly 1 expected and 1 actual json (order does not matter)"

    expectedLines
    |> List.iteri (fun i expected ->
        Expect.equal actualLines.[i] expected description
    )
