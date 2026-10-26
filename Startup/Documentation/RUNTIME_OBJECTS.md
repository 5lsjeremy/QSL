# Host-Owned Runtime Objects

QSL transports evaluated values as `object?`, allowing hosts to use their own runtime types.

Examples include:

- Vector
- Velocity
- Entity
- Inventory
- Resource
- Market
- WorldNode

These are examples, not QSL primitives.

## Member Access

QSL supports member access syntax:

```qsl
player.health
soil.moisture
```

The concrete access behavior depends on the member-access evaluator and the supplied runtime value.

## Design Rule

Do not add a language literal kind solely to transport a consumer type. Keep domain objects in the host unless the type is truly a language primitive.
