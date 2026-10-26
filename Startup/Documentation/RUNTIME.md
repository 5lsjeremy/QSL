# Runtime Architecture

## QslEngine

`QslEngine` is the primary gateway. It exposes `Parse(string)` and `Evaluate(string)`.

`Parse` creates the tokenizer and parser internally, returns the AST, and copies parser diagnostics into `QslParseResult`.

`Evaluate` records the starting provenance position, parses source, returns a parse failure when diagnostics exist, evaluates the AST through the current context, collects new error provenance, and returns `QslProcessingResult`.

## QslRuntimeRegistry

The registry stores:

- operators by name
- functions by name
- descriptors by name
- capabilities by name
- evaluators by `QslNodeKind`

`RegisterType` accepts supported QSL contracts. Evaluators use the explicit `RegisterEvaluator(QslNodeKind, IQslEvaluator)` path.

## QslRuntimeContext

The context owns its registry and supplies the evaluation environment.

Identifier resolution searches:

1. context variables
2. registered functions
3. registered descriptors
4. registered capabilities

Unresolved identifiers create provenance errors.

## Data Flow

```text
source
  -> QslEngine.Parse
  -> tokens
  -> AST
  -> QslRuntimeContext.Evaluate
  -> evaluator selected by node kind
  -> host state / extension
  -> value or provenance diagnostic
  -> QslProcessingResult
```

## Boundary Rule

Use `QslEngine` as the host gateway. Keep application state and domain semantics outside QSL.
