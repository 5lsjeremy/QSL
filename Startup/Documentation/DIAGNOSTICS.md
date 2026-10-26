# Diagnostics and Provenance

QSL separates parse failures from runtime evaluation failures.

## Parser Diagnostics

Parser diagnostics are strings collected by the parser and returned on `QslParseResult` or `QslProcessingResult.ParserDiagnostics`.

## Runtime Diagnostics

Runtime and evaluator errors are recorded as provenance entries. `QslEngine.Evaluate` returns error entries created during the current evaluation in `RuntimeDiagnostics`.

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

Types include:

- Info, Warning, Error
- Parse, Tokenize, Evaluate, Execute
- DescriptorApply, CapabilityApply
- Semantic, System, Merge, Context
- BlockCreate, BlockProcess

Categories include:

- QslSyntax
- QslRuntime
- System
- Evaluation
- Capability
