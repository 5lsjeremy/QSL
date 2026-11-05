# Language Reference

This reference describes the QSL language surface. Runtime binding and packages affect the vocabulary available to expressions; they do not add syntax.

## Literals

```qsl
10
3.14
true
false
'text'
"text"
```

## Identifiers and Member Access

```qsl
health
player.Health
player.Curiosity
action.Amount
```

A member may refer to supported special QSL state, a native public CLR property/field, or attached runtime state already received for the exact object. Mentioning an unknown member does not create it.

## Arithmetic

```qsl
+value
-value
left + right
left - right
left * right
left / right
left % right
```

## Logical and Comparison

```qsl
!active
left && right
left || right
left == right
left != right
left < right
left <= right
left > right
left >= right
```

## Null Coalescing

```qsl
value ?? fallback
```

## Assignment

```qsl
health = 100
player.Health = 75
player.Curiosity = player.Curiosity + 1
```

Assignment is right-associative. Member assignment can update writable native members and already-existing attached state. Assignment does not introduce missing attached state.

## Arrow

```qsl
left => right
```

Arrow evaluation processes left then right and returns the right result.

## Ternary

```qsl
condition ? whenTrue : whenFalse
```

## Function Calls

```qsl
heal(10)
function(a, b)
```

## Blocks

```qsl
{ first(); second(); }
```

Statements are separated by semicolons.

## Embedded Expressions and Interpolation

```qsl
$(health)
"Health: $(health)"
```

Single-quoted strings are literal strings. Double-quoted strings support embedded expressions.

## Precedence

From highest relevant binding to lowest:

1. unary `+`, `-`, `!`
2. `*`, `/`, `%`
3. `+`, `-`
4. comparisons
5. equality
6. logical AND
7. logical OR and ternary punctuation handling
8. null coalescing and assignment
9. arrow

Use parentheses when intent would otherwise be unclear.
