# Runtime Architecture

## QslEngine

`QslEngine` is the primary host gateway. It encapsulates `QslRuntimeContext` and exposes parsing, evaluation, consumer binding, and runtime-state lifecycle operations.

Public integration operations include:

- `BindConsumer(QslConsumerBinding)`
- `UnbindConsumer(QslConsumerBinding)`
- `ReceiveState(object, string, object?)`
- `ReceiveState(QslStatePackage)`
- `UpdateState(object, string, object?)`
- `ReceiveDecay(QslDecayPackage)`
- `RemoveState(object, string)`
- `RemoveState(object)`
- `Clear()`
- `Parse(string)`
- `Evaluate(string)`

`Parse` creates the tokenizer and parser internally, returns the AST, and copies parser diagnostics into `QslParseResult`.

`Evaluate` records the starting provenance position, parses source, returns a parse failure when diagnostics exist, evaluates the AST through the current context, collects new error provenance, and returns `QslProcessingResult`.

## QslRuntimeRegistry

The registry stores operators, functions, descriptors, capabilities, and evaluators. `RegisterType` accepts supported QSL registry contracts; evaluators use the explicit `RegisterEvaluator(QslNodeKind, IQslEvaluator)` path.

## QslRuntimeContext

The context owns the registry and active runtime environment. It supports state receipt, state update, state removal, evaluation, identifier resolution, and context clearing.

Identifier resolution searches:

1. context variables
2. registered functions
3. registered descriptors
4. registered capabilities

Unresolved identifiers create provenance errors.

## Consumer Binding

`QslConsumerBinding` makes an exact arbitrary runtime object available under a QSL-visible name while retaining consumer-owned type classification.

```text
Name  -> QSL identifier
Type  -> consumer-owned classification
Value -> exact runtime object
```

The binding is not a wrapper that scripts must traverse. If `Name` is `player`, QSL evaluates `player.Health`, not `player.Value.Health`.

## Native and Attached State

Native public CLR properties and fields remain owned by the consumer object.

`QslAttachedStateStore` retains additional runtime state against exact object identity without requiring the consumer type to implement a QSL contract.

Attached members must be received before they can be read or updated. Assignment can update existing attached state but cannot introduce missing attached state.

Member resolution preserves native-object precedence before attached state.

## Package Foundation

`IQslPackage` and `QslPackageBase` share the common package contract:

- exact `Target`
- resolved `Properties`
- QSL object identity
- package provenance

`QslStatePackage` batches state receipt.

`QslDecayPackage` represents resolved maintenance values. QSL does not calculate decay. Existing nonzero state is updated; numeric zero can retire state when `RemoveAtZero` is enabled.

Package provenance records package creation/application. Runtime-context provenance records the individual state changes caused by applying a package.

## Data Flow

```text
host/integration
  -> QslConsumerBinding
  -> resolved state or package
  -> QslEngine
  -> QslRuntimeContext
  -> native / attached vocabulary
  -> Parse / AST
  -> evaluator dispatch
  -> consumer state mutation or returned value
  -> QslProcessingResult
```

## Boundary Rule

QSL receives resolved vocabulary and computes against it. Semantic validation, upstream policy, propagation, eligibility, decay calculations, and domain meaning remain outside QSL.
