# Changelog

<!-- There is always Unreleased section on the top. Subsections (Add, Changed, Fix, Removed) should be Add as needed. -->
## Unreleased
- Move repository

## 7.2.0 - 2025-10-08
- Add `SerializerOptions` type
- Add `Serialize.createSerializer` function

## 7.1.0 - 2025-03-17
- Update dependencies

## 7.0.0 - 2025-03-13
- [**BC**] Use net9.0

## 6.0.0 - 2024-01-09
- [**BC**] Use net8.0
- Fix package metadata
- Serialize numbers in json value to `int64`

## 5.0.0 - 2023-09-09
- [**BC**] Use `Alma` namespace

## 4.2.0 - 2023-08-19
- Add `Serialize.JsonElement` module

## 4.1.0 - 2023-08-10
- Update dependencies

## 4.0.0 - 2023-08-09
- [**BC**] Use net 7.0

## 3.3.0 - 2023-04-19
- Add `Serialize.toJsonIgnoringNullsPretty` function
- Add `Serialize.toJsonIgnoringNulls` function
- Add `Serialize.JsonValue.toSerializableJson` function

## 3.2.0 - 2023-01-19
- Add `Serialize.hash` function

## 3.1.0 - 2022-06-06
- Update dependencies
- Include `null` in serialized json
- Indent by `4` spaces in `Serialize.toJsonPretty`

## 3.0.0 - 2022-01-04
- [**BC**] Use net6.0

## 2.0.0 - 2020-11-20
- Use .netcore 5.0

## 1.1.0 - 2020-11-20
- Update dependencies

## 1.0.0 - 2020-04-23
- Add functions
    - `Serialize.toJson`
    - `Serialize.toJsonPretty`
    - `Serialize.dateTime`
    - `Serialize.dateTimeOffset`
    - `Serialize.stringOrNull`

- Initial implementation
