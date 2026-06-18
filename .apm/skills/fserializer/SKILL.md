---
name: fserializer
description: >-
  Use whenever writing or reviewing F# code that calls Serialize.toJson, Serialize.toJsonPretty, Serialize.toJsonIgnoringNulls, Serialize.toJsonIgnoringNullsPretty, Serialize.JsonValue.toSerializableJson, Serialize.JsonValue.toSerializableJsonIgnoringNullsInRecord, Serialize.JsonElement.toSerializableJson, Serialize.JsonElement.toSerializableJsonIgnoringNullsInRecord, Serialize.createSerializer, Serialize.hash, Serialize.dateTime, Serialize.dateTimeOffset, or Serialize.stringOrNull. Trigger also on open Alma.Serializer, NuGet package Alma.Serializer, SerializerOptions, or any composition of FSharp.Data.JsonValue or System.Text.Json.JsonElement with Newtonsoft.Json output.
---

# F-Serializer

Library: [alma-oss/fserializer](https://github.com/alma-oss/fserializer)
NuGet: `Alma.Serializer`

## Purpose

`Alma.Serializer` is an F# library (NuGet: `Alma.Serializer`) providing JSON serialization utilities built on `Newtonsoft.Json` and `FSharp.Data`. It produces snake_case JSON by default, offers null-handling variants, and bridges `FSharp.Data.JsonValue` and `System.Text.Json.JsonElement` trees into the Newtonsoft serialization pipeline. Utility helpers for datetime formatting, `string option` → null mapping, and SHA-256 hashing are also included.

## When to Use

- Serializing any F# record, DU, or DTO to a JSON string.
- Bridging `FSharp.Data.JsonValue` or `System.Text.Json.JsonElement` values into Newtonsoft-formatted JSON output.
- Formatting `DateTime` / `DateTimeOffset` values to the standard `"yyyy-MM-dd'T'HH:mm:ss.fff'Z'"` string.
- Mapping `string option` to a `string` / `null` value for JSON-compatible serialization.
- Producing a SHA-256 hex digest of a string.
- Building a reusable `Newtonsoft.Json.JsonSerializer` with the library's default settings.

## When NOT to Use

- When `System.Text.Json` output is required — this library wraps Newtonsoft.Json only.
- When deserialization is needed — no deserialization API is provided.
- When camelCase output is required — all property names are unconditionally snake_case.

## Main Concepts

- `Serialize` module — top-level entry point for all operations; access via `open Alma.Serializer`.
- `SerializerOptions` — DU with cases `Pretty` and `IgnoringNulls`; passed as a list to `createSerializer`.
- `Serialize.toJson` / `toJsonPretty` — serialize any .NET object to compact or 4-space-indented JSON; includes null fields.
- `Serialize.toJsonIgnoringNulls` / `toJsonIgnoringNullsPretty` — same as the above pair but null fields are omitted from output.
- `Serialize.createSerializer` — builds a `Newtonsoft.Json.JsonSerializer` from a `SerializerOptions list` using the library defaults (SnakeCaseNamingStrategy applied).
- `Serialize.JsonValue` submodule — bridges `FSharp.Data.JsonValue` trees into a serializable `obj` for use with `Serialize.toJson` and variants.
- `Serialize.JsonValue.toSerializableJson` — converts a `JsonValue` to an `obj` tree; `JsonValue.Number` (decimal) is cast to `int64`; `JsonValue.Record` becomes an insertion-ordered `Dictionary`.
- `Serialize.JsonValue.toSerializableJsonIgnoringNullsInRecord` — like `toSerializableJson` but strips `JsonValue.Null` fields from `Record` nodes before serialization; array items are unaffected.
- `Serialize.JsonElement` submodule — bridges `System.Text.Json.JsonElement` values; parses internally via `FSharp.Data.JsonValue.Parse`.
- `Serialize.JsonElement.toSerializableJson` / `toSerializableJsonIgnoringNullsInRecord` — same semantics as the `JsonValue` equivalents.
- `Serialize.dateTime` / `dateTimeOffset` — format a `DateTime` / `DateTimeOffset` to `"yyyy-MM-dd'T'HH:mm:ss.fff'Z'"`.
- `Serialize.stringOrNull` — unwrap `string option` to `string` or `null`; type alias `StringOrNull = string option -> string`.
- `Serialize.hash` — SHA-256 of a string as a lowercase hex string (no dashes).

## Related Libraries

- `Newtonsoft.Json ~> 13.0` — underlying JSON serializer; `SnakeCaseNamingStrategy` is applied automatically by the library.
- `FSharp.Data ~> 6.0` — provides the `JsonValue` type consumed by `Serialize.JsonValue`.

## Keywords

Alma.Serializer, Serialize.toJson, Serialize.toJsonPretty, Serialize.toJsonIgnoringNulls, Serialize.toJsonIgnoringNullsPretty, Serialize.JsonValue, Serialize.JsonElement, Serialize.createSerializer, Serialize.hash, Serialize.dateTime, Serialize.dateTimeOffset, Serialize.stringOrNull, SerializerOptions, Pretty, IgnoringNulls, snake_case, null handling, NullValueHandling, FSharp.Data JsonValue, System.Text.Json JsonElement, Newtonsoft.Json, F# JSON serialization, open Alma.Serializer

## Reference Files

For composition principles and recommended API usage patterns, read `references/preferred-patterns.md`.
For known pitfalls and incorrect assumptions about the API, read `references/anti-patterns.md`.
For worked code examples ordered by complexity, read `references/examples.md`.
