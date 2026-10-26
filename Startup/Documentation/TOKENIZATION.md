# Tokenization

`QslTokenizer` converts source text into `QslToken` values and always emits `EndOfFile`.

Tokenization is internal to `QslEngine`.

## Recognized Input

The tokenizer recognizes:

- identifiers and `true` / `false`
- integer and decimal numeric text
- single-quoted strings
- double-quoted strings
- embedded expressions inside double-quoted strings
- grouping and block punctuation
- member access
- commas and semicolons
- arithmetic, logical, comparison, equality, assignment, null-coalescing, and arrow symbols

## Metadata

`QslTokenKind` is the canonical token inventory. Every enum member has `QslTokenAttribute` metadata.

Metadata can define:

- symbol
- type
- unary support
- binary support
- binary precedence
- unary precedence
- right associativity
- executor type
- evaluation strategy

`QslKindExtensions` reflects and validates this metadata once, then exposes typed queries such as `Precedence`, `IsUnary`, `IsBinary`, `ExecutorType`, and `EvaluationStrategy`.

## Extension Warning

Adding syntax is not a single-file operation. New syntax may require tokenizer, token metadata, parser, AST, evaluator, operator, diagnostics, and certification coverage. Treat language changes as versioned QSL changes.
