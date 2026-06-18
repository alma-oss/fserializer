# Examples — Alma.Serializer

---

## Basic: `toJson`

```fsharp
open Alma.Serializer

type DataRecord = { Id: int; Name: string }

let record = { Id = 1; Name = "example" }

let compact: string = record |> Serialize.toJson
// compact = """{"id":1,"name":"example"}"""
```

---

## Pretty output: `toJsonPretty`

```fsharp
open Alma.Serializer

type DataRecord = { Id: int; Name: string }

let record = { Id = 1; Name = "example" }

let pretty: string = record |> Serialize.toJsonPretty
// pretty =
// {
//     "id": 1,
//     "name": "example"
// }
```

---

## Ignoring nulls: `toJsonIgnoringNulls` and `toJsonIgnoringNullsPretty`

```fsharp
open Alma.Serializer

type ResponseDto = { Id: int; Tag: string }

let withNull  = { Id = 1; Tag = null }
let withValue = { Id = 2; Tag = "active" }

let compactNoNulls: string = withNull  |> Serialize.toJsonIgnoringNulls
// compactNoNulls = """{"id":1}"""

let prettyNoNulls: string = withValue |> Serialize.toJsonIgnoringNullsPretty
// prettyNoNulls =
// {
//     "id": 2,
//     "tag": "active"
// }
```

---

## `stringOrNull` helper

```fsharp
open Alma.Serializer

type RequestPayload = { Id: int; Label: string }

// Serialize.stringOrNull: string option -> string
let toPayload (id: int) (label: string option) : RequestPayload =
    { Id = id; Label = label |> Serialize.stringOrNull }

let payloadA = toPayload 1 (Some "active")  // Label = "active"
let payloadB = toPayload 2 None             // Label = null

let jsonA = payloadA |> Serialize.toJson              // """{"id":1,"label":"active"}"""
let jsonB = payloadB |> Serialize.toJsonIgnoringNulls // """{"id":2}"""
```

---

## DateTime formatting

```fsharp
open System
open Alma.Serializer

let formatted: string = Serialize.dateTime DateTime.UtcNow
// formatted = "2026-06-17T10:30:00.000Z"

let formattedOffset: string = Serialize.dateTimeOffset DateTimeOffset.UtcNow
// formattedOffset = "2026-06-17T10:30:00.000Z"
```

---

## Hash function

```fsharp
open Alma.Serializer

let digest: string = Serialize.hash "resource-identifier-42"
// digest = "a3f1..." (SHA-256, lowercase hex, no dashes, 64 chars)
```

---

## JsonValue pipeline: `toSerializableJson` → `toJson`

```fsharp
open FSharp.Data
open Alma.Serializer

let jsonValue =
    JsonValue.Record [|
        "id",   JsonValue.Number (decimal 1)
        "name", JsonValue.String "worker-a"
        "rate", JsonValue.Float  3.14        // Float, not Number — preserves decimal
    |]

let json: string =
    jsonValue
    |> Serialize.JsonValue.toSerializableJson  // JsonValue -> obj
    |> Serialize.toJson                        // obj -> string
// json = """{"id":1,"name":"worker-a","rate":3.14}"""
// Note: JsonValue.Number (decimal 1) serializes as int64 -> 1 (no fractional part).
```

---

## JsonValue ignoring nulls in records

```fsharp
open FSharp.Data
open Alma.Serializer

let jsonValue =
    JsonValue.Record [|
        "id",       JsonValue.Number (decimal 10)
        "label",    JsonValue.String "active"
        "optional", JsonValue.Null               // stripped from Record fields
    |]

let json: string =
    jsonValue
    |> Serialize.JsonValue.toSerializableJsonIgnoringNullsInRecord  // strips Null from Record fields
    |> Serialize.toJsonIgnoringNulls                                // also omits remaining null object props
// json = """{"id":10,"label":"active"}"""

// Note: JsonValue.Null items inside a JsonValue.Array are NOT stripped.
// Pre-filter the array before constructing JsonValue.Array if needed.
```

---

## JsonElement pipeline

```fsharp
open System.Text.Json
open Alma.Serializer

let rawJson = """{"service_name":"worker-b","enabled":true,"count":5}"""
let element: JsonElement = (JsonDocument.Parse rawJson).RootElement

let json: string =
    element
    |> Serialize.JsonElement.toSerializableJson
    |> Serialize.toJson
// json = """{"service_name":"worker-b","enabled":true,"count":5}"""

// For null-ignoring output:
let jsonNoNulls: string =
    element
    |> Serialize.JsonElement.toSerializableJsonIgnoringNullsInRecord
    |> Serialize.toJsonIgnoringNulls
```

---

## Custom serializer with `createSerializer`

```fsharp
open System.IO
open Alma.Serializer
open Newtonsoft.Json

type DataRecord = { Id: int; Name: string option }

// Build a reusable serializer: pretty output + nulls omitted
let serializer: JsonSerializer =
    Serialize.createSerializer [ Pretty; IgnoringNulls ]

// Use with any Newtonsoft.Json writer
let record = { Id = 7; Name = None }
let sb = System.Text.StringBuilder()
use sw = new StringWriter(sb)
serializer.Serialize(sw, record)
let json: string = sb.ToString()
// json =
// {
//     "id": 7
// }
```
