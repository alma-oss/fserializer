# Preferred Patterns — Alma.Serializer

## Core Principles

- **Always `open Alma.Serializer`** before using any `Serialize.*` function.
- **snake_case is automatic.** `SnakeCaseNamingStrategy` is applied by default — never rename DTO properties manually to force snake_case output.
- **Nulls are included by default.** `toJson` / `toJsonPretty` emit null fields. Use the `IgnoringNulls` variants to suppress them.
- **`JsonValue.Number` is serialized as `int64`.** The decimal backing of `JsonValue.Number` is cast to `int64` on serialization — use `JsonValue.Float` for floating-point values.
- **`JsonValue.Record` preserves insertion order** via `Dictionary<string, _>` — unlike `Map`, key order is stable and matches the source array order.
- **`JsonElement` routes through `JsonValue.Parse` internally.** Floats in array position may lose precision; see `references/anti-patterns.md` for details.

## Recommended API Usage

### Plain .NET/F# records and DTOs

Use `Serialize.toJson` for compact output or `Serialize.toJsonPretty` for indented output (4 spaces). See `examples.md` → *Basic: toJson* and *Pretty output: toJsonPretty*.

### Optional string fields

Map `string option` to a JSON-compatible `string` / `null` with `Serialize.stringOrNull` before constructing the DTO passed to serialization. See `examples.md` → *stringOrNull helper*.

### `FSharp.Data.JsonValue` trees

Always call `Serialize.JsonValue.toSerializableJson` first, then pipe the result into `Serialize.toJson`. To strip null fields from `Record` nodes, use `Serialize.JsonValue.toSerializableJsonIgnoringNullsInRecord` instead, paired with `Serialize.toJsonIgnoringNulls`. See `examples.md` → *JsonValue pipeline* and *JsonValue ignoring nulls in records*.

The `IgnoringNulls` behavior for the `JsonValue` path applies **only to `Record` fields** — `null` items inside `JsonValue.Array` are not stripped by either variant. Pre-filter the array before building the `JsonValue` if array-level null removal is needed.

### `System.Text.Json.JsonElement` values

Use `Serialize.JsonElement.toSerializableJson` → `Serialize.toJson`. For null-ignoring output use `toSerializableJsonIgnoringNullsInRecord` → `Serialize.toJsonIgnoringNulls`. See `examples.md` → *JsonElement pipeline*.

### DateTime fields

Always format `DateTime` and `DateTimeOffset` values with `Serialize.dateTime` / `Serialize.dateTimeOffset` to produce `"yyyy-MM-dd'T'HH:mm:ss.fff'Z'"`. See `examples.md` → *DateTime formatting*.

### Custom serializer

When direct access to a `Newtonsoft.Json.JsonSerializer` instance is needed, use `Serialize.createSerializer [options]`. This returns a `JsonSerializer` pre-configured with the library defaults (SnakeCaseNamingStrategy, desired null handling, optional formatting). See `examples.md` → *Custom serializer with createSerializer*.

## Composition

The standard pipeline for `FSharp.Data.JsonValue` is: call `Serialize.JsonValue.toSerializableJson` to produce an `obj` tree, then pass the result to `Serialize.toJson` or `Serialize.toJsonPretty`. For null-ignoring output, replace the first step with `Serialize.JsonValue.toSerializableJsonIgnoringNullsInRecord` and the final step with `Serialize.toJsonIgnoringNulls` or `Serialize.toJsonIgnoringNullsPretty`.

## Naming Conventions

DTO property names may use PascalCase in F# — snake_case conversion is applied automatically. The mapping is `PascalCase` → `pascal_case` (e.g. `ServiceName` → `service_name`, `CreatedAt` → `created_at`). Design DTOs with the expected snake_case output key in mind.

## Testing Recommendations

- Use Expecto test style consistent with the library's own tests.
- To compare JSON output while ignoring formatting differences, normalize both sides: deserialize with `Newtonsoft.Json.JsonConvert.DeserializeObject`, re-serialize with `Serialize.toJsonPretty`, then compare line-by-line (pattern from `tests/Utils.fs`).
- Always have an explicit test case for the null-present and null-absent branches when testing `IgnoringNulls` paths.
