# Parsing

Parsing is internal to `QslEngine`.

```text
source -> QslTokenizer -> QslParser -> IQslNode AST
```

Consumers normally call:

```csharp
QslParseResult result = engine.Parse(source);
```

## Supported Structures

The parser constructs AST nodes for:

- numeric, boolean, and string literals
- identifiers
- unary and binary expressions
- grouped expressions
- function calls
- member access
- ternary expressions
- blocks
- embedded expressions
- interpolated strings
- error nodes

## Precedence and Associativity

Binary parsing reads precedence and right-associativity from token metadata through `QslKindExtensions`.

## Diagnostics

`QslParser.Errors` is the parser diagnostic collection. It is intentionally distinct from runtime provenance.

`QslEngine.Parse` copies these diagnostics into `QslParseResult`.
