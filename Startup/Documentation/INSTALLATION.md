# Installation

## Runtime Requirement

QSL Runtime 1.0 targets .NET Standard 2.1 and references `System.Text.Json` 8.0.5 for provenance export.

## .NET Host

Reference the supplied QSL runtime package or assembly and confirm the host can resolve:

```csharp
using QSL.Core;
using QSL.Core.Runtime;
using QSL.Core.Runtime.Context;
```

Then follow `QUICKSTART.md`.

## Distribution Boundary

The commercial runtime package should contain the QSL runtime, generated XML documentation, examples, product documentation, license, and release notes.

The CLI certification harness, development audit scripts, generated audit reports, IDE metadata, `bin`, and `obj` are not runtime dependencies.

## Unity

See `UNITY.md` for managed plug-in setup and a MonoBehaviour host example.
