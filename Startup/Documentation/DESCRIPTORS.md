# Descriptors

A descriptor implements `IQslDescriptor : IQslRegistryItem` and provides:

```csharp
void Apply(QslRuntimeContext context);
```

Descriptors are appropriate for exposing or initializing host domain state.

## Example

```csharp
public void Apply(QslRuntimeContext context)
{
    context.SetVariable("health", 100);
    context.SetVariable("mana", 50);
}
```

## Lifecycle

```csharp
registry.RegisterType(playerDescriptor);
playerDescriptor.Apply(context);
```

Registration makes the descriptor resolvable through the registry. Application places its state into the selected context.

## Boundary

A descriptor does not make domain values part of the QSL language. It makes host-owned data available to an evaluation environment.
