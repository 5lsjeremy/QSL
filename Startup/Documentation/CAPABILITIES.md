# Capabilities

A capability implements `IQslCapability : IQslRegistryItem` and provides:

```csharp
void Apply(QslRuntimeContext context);
```

Capabilities are useful for applying a coherent group of host-owned behavior to a runtime context.

## Example Domain

A `PhysicsCapability` might make related operations such as `applyForce` and `dampenVelocity` available while keeping `Vector` and `Velocity` consumer-owned.

## Lifecycle

```csharp
registry.RegisterType(physicsCapability);
physicsCapability.Apply(context);
```

## Descriptor vs Capability

Recommended convention:

- a descriptor exposes or initializes domain state
- a capability installs related behavior or functionality

QSL enforces the common application contract; the host owns the semantic distinction.
