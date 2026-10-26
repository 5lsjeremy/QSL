# Processing Guide

`RegisterDefaultEvaluators()` installs the standard processing set.

| Node kind | Purpose | Example |
|---|---|---|
| Literal | Produces a literal value | `10`, `true`, `'text'` |
| Identifier | Resolves the active context | `health` |
| Unary | Applies a unary operation | `!active`, `-value` |
| Binary | Applies binary strategy/operator behavior | `health + 10` |
| Call | Invokes a registered host function | `heal(10)` |
| Embedded | Evaluates an embedded expression | `$(health)` |
| InterpolatedString | Combines string parts and embedded results | `"Health: $(health)"` |
| Block | Evaluates ordered statements | `{ heal(10); patrol(); }` |
| Error | Carries parser-produced error behavior | parser-generated |
| MemberAccess | Reads a member from a supported runtime value | `player.health` |
| Ternary | Selects between expressions | `active ? run() : wait()` |

## Composition

```qsl
health < 30 ? heal(20) : patrol()
```

This can involve identifier, literal, binary, ternary, and call evaluation in one expression.

## Trust Boundary

The supported processing behavior is part of the QSL product contract. Individual evaluator implementations remain runtime implementation details, even though managed assemblies can be inspected with ordinary .NET tooling.
