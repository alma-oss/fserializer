F-Serializer
============

[![NuGet](https://img.shields.io/nuget/v/Alma.Serializer.svg)](https://www.nuget.org/packages/Alma.Serializer)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Alma.Serializer.svg)](https://www.nuget.org/packages/Alma.Serializer)
[![Tests](https://github.com/alma-oss/fserializer/actions/workflows/tests.yaml/badge.svg)](https://github.com/alma-oss/fserializer/actions/workflows/tests.yaml)

> Library for common serializations.

---

## Install

Add following into `paket.references`
```
Alma.Serializer
```

## Release
1. Increment version in `Serializer.fsproj`
2. Update `CHANGELOG.md`
3. Commit new version and tag it

## Development
### Requirements
- [dotnet core](https://dotnet.microsoft.com/learn/dotnet/hello-world-tutorial)

### Build
```bash
./build.sh build
```

### Tests
```bash
./build.sh -t tests
```
