# Functions

Functions expose host operations to QSL through `IQslFunction`.

```qsl
heal(10)
```

The host owns the meaning of `heal`.

## Registration

```csharp
registry.RegisterType(new HealFunction());
```

## Responsibilities

A function should:

- validate domain arguments
- use the supplied `QslRuntimeContext`
- return a meaningful host value or `null`
- record runtime failures through provenance

## Return Types

Function results cross the QSL boundary as `object?`, allowing host-owned values without adding a QSL literal kind for every domain type.
