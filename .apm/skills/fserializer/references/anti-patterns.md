# Anti-Patterns — Alma.Serializer

## Mistake: Using `JsonValue.Number` for floating-point values

**Why:** `JsonValue.Number` has a `decimal` backing but is serialized as `int64` — any fractional part is truncated (`3.14` becomes `3`).

**Fix:** Use `JsonValue.Float` for floating-point values. Reserve `JsonValue.Number` for integer values.

---

## Mistake: Piping `JsonValue` directly into `Serialize.toJson`

**Why:** `JsonValue` is an F# discriminated union. `Newtonsoft.Json` will serialize the DU tag and fields structure, not the JSON data it represents.

**Fix:** Always call `Serialize.JsonValue.toSerializableJson` (or the `IgnoringNulls` variant) first to convert to an `obj` tree, then pipe into `Serialize.toJson`.

---

## Mistake: Using `toSerializableJson` + `toJsonIgnoringNulls` to omit null fields from a `JsonValue.Record`

**Why:** `toSerializableJson` converts `JsonValue.Record` to a `Dictionary<string, obj>`. Newtonsoft's `NullValueHandling.Ignore` applies to class properties via reflection, not to Dictionary entries — null values in the Dictionary are still emitted.

**Fix:** Use `Serialize.JsonValue.toSerializableJsonIgnoringNullsInRecord` instead of `toSerializableJson`. It manually strips `JsonValue.Null` entries from Record nodes before they become Dictionary entries. Pair with `Serialize.toJsonIgnoringNulls` for full coverage.

---

## Mistake: Expecting null items inside `JsonValue.Array` to be stripped

**Why:** Both `toSerializableJson` and `toSerializableJsonIgnoringNullsInRecord` pass `null` array items through as-is. The `IgnoringNulls` behavior is scoped to `Record` fields only.

**Fix:** Filter the array before building the `JsonValue`: remove `JsonValue.Null` items from the sequence before passing it to `JsonValue.Array`.

---

## Mistake: Configuring `Newtonsoft.Json` directly instead of using `Serialize.createSerializer`

**Why:** Creating `JsonSerializerSettings` or `JsonSerializer` directly bypasses the required `SnakeCaseNamingStrategy` and the library's null-handling defaults — the output format will diverge from the rest of the codebase.

**Fix:** Use `Serialize.createSerializer [Pretty; IgnoringNulls]` (or whichever options apply) to get a properly configured `JsonSerializer`. See `examples.md` → *Custom serializer with createSerializer*.

---

## Mistake: Expecting camelCase output

**Why:** The library hardcodes `SnakeCaseNamingStrategy` — there is no opt-in camelCase mode.

**Fix:** Design DTOs knowing all property names will appear as snake_case in the output. Do not add manual underscore transforms to property names.

---

## Mistake: Using `Serialize.JsonElement` for JSON input that contains floats in arrays

**Why:** The `JsonElement` path parses the raw JSON via `FSharp.Data.JsonValue.Parse`, which does not reliably preserve floating-point values in array position (marked as a known TODO in the source).

**Fix:** If float precision in arrays is required, either process the raw JSON string separately or avoid the `JsonElement` path for that data shape.
