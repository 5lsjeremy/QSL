# Processing Guide

`RegisterDefaultEvaluators()` installs the standard processing set.

| Node kind | Purpose | Example |
|---|---|---|
| Literal | Produces a literal value | `10`, `true`, `'text'` |
| Identifier | Resolves the active context | `health`, `player` |
| Unary | Applies a unary operation | `!active`, `-value` |
| Binary | Applies binary strategy/operator behavior | `health + 10` |
| Call | Invokes a registered host function | `heal(10)` |
| Embedded | Evaluates an embedded expression | `$(health)` |
| InterpolatedString | Combines string parts and embedded results | `"Health: $(health)"` |
| Block | Evaluates ordered statements | `{ heal(10); patrol(); }` |
| Error | Carries parser-produced error behavior | parser-generated |
| MemberAccess | Reads supported native or attached object state | `player.Health` |
| Ternary | Selects between expressions | `active ? run() : wait()` |

## Member Access

For ordinary consumer-owned objects, member access can read native public CLR properties/fields and runtime-attached state previously received for the exact target object.

Native members retain precedence over attached members.

Attached state does not exist merely because source mentions a member name. The state must first be received through the runtime state surface.

## Assignment

Assignment to a member resolves a writable native property, a field, or already-existing attached state.

Assignment does not introduce missing attached state.

This allows native and attached values to compose naturally:

```qsl
player.Health = player.Health - (action.Amount * (action.SpaceDustAppetite / 100))
```

QSL does not need domain-specific understanding of `Amount` or `SpaceDustAppetite`; it evaluates them once the host has made them valid runtime vocabulary.

## Packages and Resolved Values

Packages are runtime integration objects, not language syntax.

`QslStatePackage` batches state receipt.

`QslDecayPackage` applies already-resolved maintenance values to existing attached state. QSL does not calculate decay or other upstream policies represented by future package types.

## Trust Boundary

The supported processing behavior is part of the QSL product contract. Hosts remain responsible for deciding which consumer objects, state, functions, capabilities, descriptors, and other vocabulary are supplied to QSL.
