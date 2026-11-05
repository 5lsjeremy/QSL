# API Reference Overview

## Primary Surface

### `QSL.Core.QslEngine`

Parsing and evaluation:

- `Parse(string)` -> `QslParseResult`
- `Evaluate(string)` -> `QslProcessingResult`

Consumer vocabulary lifecycle:

- `BindConsumer(QslConsumerBinding)`
- `UnbindConsumer(QslConsumerBinding)`
- `ReceiveState(object target, string propertyName, object? value)`
- `ReceiveState(QslStatePackage)` -> number of newly received properties
- `UpdateState(object target, string propertyName, object? value)`
- `ReceiveDecay(QslDecayPackage)` -> number of affected properties
- `RemoveState(object target, string propertyName)`
- `RemoveState(object target)`
- `Clear()`

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

## Runtime Context and State

### `QslRuntimeContext`

Owns the registry, evaluates AST nodes, resolves identifiers, receives/updates/removes attached state, and clears the active environment without clearing the registry.

### `QslAttachedStateStore`

Identity-keyed storage for runtime state attached to exact consumer-owned objects.

Core operations:

- `Introduce`
- `Contains`
- `TryGet`
- `TrySet`
- `Remove`
- `RemoveAll`
- `Clear`

### `QslConsumerBinding`

Carries:

- `Name`: QSL-visible identifier
- `Type`: consumer-owned classification
- `Value`: exact runtime object

## Packages

### `IQslPackage`

Common package contract for an exact `Target` and resolved `Properties`.

### `QslPackageBase`

Base QSL object for packages. Provides package identity, slug heritage, provenance, target, properties, and internal package-application provenance.

### `QslStatePackage`

Batches approved state receipt with the same non-destructive semantics as single-state receipt.

### `QslDecayPackage`

Carries already-resolved maintenance values. Nonzero values update existing attached state. Numeric zero can remove existing state when `RemoveAtZero` is enabled. Decay does not introduce missing attached state.

## Runtime Registry

### `QslRuntimeRegistry`

Holds operators, functions, descriptors, capabilities, and evaluators. Provides `RegisterDefaultEvaluators`, `RegisterType`, `RegisterEvaluator`, and `Evaluate`.

## Extension Contracts

- `IQslFunction`
- `IQslDescriptor`
- `IQslCapability`
- `IQslEvaluator`
- `IQslObject`
- `IQslRuntimeObject`
- `IQslRegistryItem`
- `IQslPackage`

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
