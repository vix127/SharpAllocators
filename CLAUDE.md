# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**SharpAllocators** is a .NET library providing low-level memory allocation primitives with support for both managed and native memory. It targets **.NET 11.0** and uses unsafe code (`AllowUnsafeBlocks=true`).

## Architecture

The library consists of these core types in `src/`:

| Type | Purpose |
|------|---------|
| `IAllocator` | Interface for allocators with `Allocate`, `Free`, `Reallocate` methods |
| `IMemorySlice<TElement, TLength>` | Generic interface for memory slices with pointer and length |
| `MemorySlice<T>` | Readonly unsafe struct implementing `IMemorySlice<T, nuint>` |
| `LongSpan<T>` | Ref struct representing contiguous memory (managed, native, or stack) with bounds checking |
| `NativeAllocator` | `IAllocator` implementation using `System.Runtime.InteropServices.NativeMemory` |

Key design points:
- Uses `nuint` for sizes to support large allocations on 64-bit
- `LongSpan<T>` is a `ref struct` (stack-only) with indexer bounds checking
- All allocator methods are `[MethodImpl(MethodImplOptions.AggressiveInlining)]`
- Generic constraints use `where T : unmanaged` for type safety

## Build & Test Commands

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build --no-restore

# Run tests
dotnet test --no-build --verbosity normal

# Build + test in one command
dotnet test
```

## Development Notes

- **Target Framework**: net11.0 (configured in `src/SharpAllocators.csproj`)
- **Unsafe code**: Required — project has `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>`
- **Nullable**: Enabled (`<Nullable>enable</Nullable>`)
- **ImplicitUsings**: Disabled (`<ImplicitUsings>disable</ImplicitUsings>`)

## CI/CD

GitHub Actions workflow (`.github/workflows/dotnet.yml`) runs on push/PR to main:
1. Setup .NET 11.0.x
2. `dotnet restore`
3. `dotnet build --no-restore`
4. `dotnet test --no-build --verbosity normal`

## File Structure

```
SharpAllocators/
├── .github/workflows/dotnet.yml   # CI pipeline
├── SharpAllocators.slnx           # Solution file
├── README.md                      # Minimal readme
├── LICENSE                        # MIT License
└── src/
    ├── SharpAllocators.csproj     # Project config (net11.0, unsafe, nullable)
    ├── IAllocator.cs              # Allocator interface
    ├── IMemorySlice.cs            # Memory slice interface
    ├── LongSpan.cs                # Ref struct for memory spans
    ├── MemorySlice.cs             # Memory slice implementation
    └── NativeAllocator.cs         # NativeMemory-based allocator
```
## Engineering Guidelines

Source: project wiki (Engineering guidelines).

### Copyright & License

All source code files under `src/*.cs` **require** this exact header:

```csharp
// Copyright (c) 2026 Viktor Stojanović. All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.
```

### Coding Guidelines

#### Coding style - general

Use the default Visual Studio settings to format code.

- Use four spaces for indentation. Do **not** use tabs.
- Use `_camelCase` format for private fields.
- Do **not** use `this.` unless it is required.
- Write the visibility for every member, including default members (for example, `private string _foo;` instead of `string _foo;`).
- Put opening braces `{` on a new line.

#### Use C# type keywords in favor of .NET type names

When a type has a C# keyword, **use** the keyword instead of the .NET type name.

Correct:

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public T* Allocate<T>(long count) where T : unmanaged
{
    return (T*)NativeMemory.Alloc((nuint)(count * sizeof(T)));
}
```

Incorrect:

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public T* Allocate<T>(Int64 count) where T : unmanaged
{
    return (T*)NativeMemory.Alloc((UIntPtr)(count * sizeof(T)));
}
```

#### Usage of var keyword

**Use** the `var` keyword when the compiler allows it.

#### Documentation (doc) comments

**Write** doc comments for public APIs. You can also write doc comments for non-public APIs.

#### Primary constructors

Do **not** use primary constructors.
