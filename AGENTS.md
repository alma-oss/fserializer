# AGENTS.md — Alma.Serializer

This repo ships Agent Skill for the `Alma.Serializer` library. Compatible agents discover it automatically; see `.agents/skills/fserializer/SKILL.md`

## Project Purpose

F# library providing common serialization utilities. Wraps `Newtonsoft.Json` and `FSharp.Data` for JSON serialization/deserialization with F#-friendly APIs. Published as NuGet package `Alma.Serializer`.

## Tech Stack

- **Language:** F# (.NET 10)
- **Framework:** .NET SDK library
- **Package management:** Paket
- **Build system:** FAKE (F# Make) via `build.sh`
- **Testing:** Expecto (via `YoloDev.Expecto.TestSdk`)
- **Linting:** fsharplint
- **CI/CD:** GitHub Actions
- **Key dependencies:** `FSharp.Core ~> 10.0`, `FSharp.Data ~> 6.0`, `Newtonsoft.Json ~> 13.0`

## Commands

```bash
# Install dependencies
dotnet tool restore && dotnet paket install

# Build
./build.sh build

# Run tests
./build.sh -t tests

# Lint
dotnet fsharplint lint Serializer.fsproj
```

## Project Structure

```
fserializer/
├── Serializer.fsproj           # Main project (PackageId: Alma.Serializer, v9.0.0)
├── AssemblyInfo.fs             # Auto-generated
├── src/
│   └── Serializer.fs           # Core serialization logic
├── tests/
│   └── tests.fsproj            # Expecto test project
├── build/
│   └── ...
├── build.sh
├── paket.dependencies
├── paket.references            # FSharp.Core, FSharp.Data, Newtonsoft.Json
├── global.json                 # .NET SDK 10.0.0
├── fsharplint.json
├── CHANGELOG.md
└── .github/workflows/
    ├── tests.yaml
    ├── pr-check.yaml
    └── publish.yaml
```

## Architecture

Pure library — single source file (`Serializer.fs`) providing JSON serialization/deserialization. Uses both `FSharp.Data` for F# type providers and `Newtonsoft.Json` for general JSON handling.

## Build System (FAKE)

Standard library target chain: `Clean → AssemblyInfo → Build → Lint → Tests → Release → Publish`

## CI/CD

- **tests.yaml** — runs on PRs and nightly
- **pr-check.yaml** — blocks fixup commits, runs ShellCheck
- **publish.yaml** — publishes to NuGet on semver tags

## Release Process

1. Increment `<Version>` in `Serializer.fsproj`
2. Update `CHANGELOG.md`
3. Commit, tag with version, push

## Conventions

- Uses `Newtonsoft.Json` (not `System.Text.Json`) — maintain consistency
- `FSharp.Data` for type-provider-based JSON parsing
- Compile order in `.fsproj` matters

## Pitfalls

- **Dual JSON libraries** — both `FSharp.Data` and `Newtonsoft.Json` are used; be consistent with the existing patterns
- **Paket, not NuGet CLI** — use `dotnet paket install`
- **Test framework** — uses Expecto, not xUnit/NUnit
