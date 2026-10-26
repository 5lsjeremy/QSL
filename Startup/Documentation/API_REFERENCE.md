# API Reference Overview

## Primary Surface

### `QSL.Core.QslEngine`

- `Parse(string)` -> `QslParseResult`
- `Evaluate(string)` -> `QslProcessingResult`

### `QslParseResult`

- `Ast`
- `Diagnostics`
- `HasDiagnostics`

### `QslProcessingResult`

- `Ast`
- `Value`
- `ParserDiagnostics`
- `RuntimeDiagnostics`
- `Exception`
- `ParseFailed`
- `EvaluationFailed`
- `HasException`
- `Succeeded`

## Runtime

### `QslRuntimeRegistry`

Holds operators, functions, descriptors, capabilities, and evaluators. Provides `RegisterDefaultEvaluators`, `RegisterType`, `RegisterEvaluator`, and `Evaluate`.

### `QslRuntimeContext`

Owns a registry, evaluates nodes, and resolves identifiers.

## Extension Contracts

- `IQslFunction`
- `IQslDescriptor`
- `IQslCapability`
- `IQslEvaluator`
- `IQslObject`
- `IQslRuntimeObject`
- `IQslRegistryItem`

## Language Metadata

- `QslTokenKind`
- `QslTokenAttribute`
- `QslKindExtensions`
- `QslEvaluationStrategy`
- `QslNodeKind`

## Diagnostics

- `QslProvenanceBus`
- `QslProvenanceEntry`
- `QslProvenanceType`
- `QslProvenanceCategory`
