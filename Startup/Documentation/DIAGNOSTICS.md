# Diagnostics and Provenance

QSL separates parse failures from runtime evaluation failures and uses provenance for runtime/context history.

## Parser Diagnostics

Parser diagnostics are strings collected by the parser and returned on `QslParseResult` or `QslProcessingResult.ParserDiagnostics`.

## Runtime Diagnostics

Runtime and evaluator errors are recorded as provenance entries. `QslEngine.Evaluate` returns error entries created during the current evaluation in `RuntimeDiagnostics`.

## Runtime-State Provenance

`QslRuntimeContext` records successful attached-state lifecycle operations such as state receipt, update, removal, and context clearing.

## Package Provenance

Packages derive from `QslBaseObject` through `QslPackageBase`. A package therefore has its own `Id`, `Slug`, and provenance history.

Package provenance records package lifecycle events such as creation and completed application, including the target type, property count, and affected count.

Individual property mutations remain recorded by `QslRuntimeContext`, so package-level provenance and per-state provenance remain distinct rather than duplicating every mutation onto the package.

## Processing Result

Inspect:

- `ParseFailed`
- `EvaluationFailed`
- `HasException`
- `Succeeded`
- `Value`
- `ParserDiagnostics`
- `RuntimeDiagnostics`
- `Exception`

## Provenance Export

`QslProvenanceBus.Export` serializes selected provenance entries to JSON. This is why QSL Runtime references `System.Text.Json`.

## Current Provenance Values

Types include Info, Warning, Error, Parse, Tokenize, Evaluate, Execute, DescriptorApply, CapabilityApply, Semantic, System, Merge, Context, BlockCreate, and BlockProcess.

Categories include QslSyntax, QslRuntime, System, Evaluation, and Capability.
