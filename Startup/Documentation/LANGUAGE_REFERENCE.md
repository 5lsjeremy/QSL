# Language Reference

This reference describes the certified QSL Runtime 1.0 language surface.

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
player.health
```

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
```

Assignment is right-associative.

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
