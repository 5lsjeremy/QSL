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

## Stability Guidance

Use `QslEngine`, processing results, runtime context, registry, and extension contracts for ordinary application integration.

Treat implementation classes as inspectable but not automatically promised as stable extension contracts. Public accessibility does not imply every implementation detail is a beginner API.

## Language Changes

Any language addition should include:

- token metadata decision
- tokenizer support when required
- parser/AST support
- evaluator or operator behavior
- provenance behavior
- certification coverage
- documentation and migration notes
