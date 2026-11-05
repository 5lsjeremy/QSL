# Tokenization

`QslTokenizer` converts source text into `QslToken` values and always emits `EndOfFile`.

Tokenization is internal to `QslEngine`.

## Recognized Input

The tokenizer recognizes identifiers and `true` / `false`, integer and decimal numeric text, single-quoted strings, double-quoted strings, embedded expressions inside double-quoted strings, grouping and block punctuation, member access, commas and semicolons, and arithmetic, logical, comparison, equality, assignment, null-coalescing, and arrow symbols.

## Metadata

`QslTokenKind` is the canonical token inventory. Every enum member has `QslTokenAttribute` metadata. Metadata can define symbol, type, unary/binary support, precedence, right associativity, executor type, and evaluation strategy.

`QslKindExtensions` reflects and validates this metadata once, then exposes typed queries such as `Precedence`, `IsUnary`, `IsBinary`, `ExecutorType`, and `EvaluationStrategy`.

## Extension Warning

Adding syntax is not a single-file operation. New syntax may require tokenizer, token metadata, parser, AST, evaluator, operator, diagnostics, and certification coverage. Runtime state/package additions that do not change syntax should not be treated as language changes.
