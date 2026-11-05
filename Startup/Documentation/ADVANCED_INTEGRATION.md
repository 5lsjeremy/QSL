# Advanced Integration

## Public Advanced Surfaces

Advanced tooling can inspect or interact with:

- AST nodes and `QslNodeKind`
- `IQslEvaluator`
- `IQslOperator`
- `QslTokenKind` metadata
- `QslEvaluationStrategy`
- registry evaluator mappings
- parser results
- provenance entries
- consumer bindings
- runtime-attached state
- state and maintenance packages

## Integration Boundary

Prefer `QslEngine` as the public integration gateway. Consumer bindings and package/state operations should flow through its public surface rather than external integrations manipulating `QslRuntimeContext` storage directly.

`QslConsumerBinding` carries the QSL-visible name, consumer-owned classification, and exact runtime object.

`IQslPackage`/`QslPackageBase` provide a common target/property envelope for resolved values. Concrete package types may express different upstream intent, but QSL continues to receive resolved values rather than reproducing upstream policy calculations.

## Package Extension Guidance

A future package should preserve these rules:

- target an exact runtime object
- carry resolved values/instructions
- avoid embedding domain meaning in QSL Core
- use package identity/provenance for package lifecycle
- route actual state mutations through the existing runtime-state lifecycle where appropriate
- avoid turning the package into an evaluator, registry item, or policy calculator unless a separate language/runtime requirement truly demands it

## Stability Guidance

Use `QslEngine`, processing results, runtime context, registry, and documented extension contracts for application integration.

Treat implementation classes as inspectable but not automatically promised as stable extension contracts. Public accessibility does not imply every implementation detail is a beginner API.

## Language Changes

Runtime packages and state hydration do not by themselves require language changes.

Any actual language addition should include token metadata decision, tokenizer support when required, parser/AST support, evaluator or operator behavior, provenance behavior, certification coverage, and documentation/migration notes.
