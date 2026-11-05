# Host-Owned Runtime Objects and Attached State

QSL transports evaluated values as `object?`, allowing hosts to use their own runtime types without turning those types into QSL primitives.

Examples include Vector, Velocity, Entity, Inventory, Resource, Market, WorldNode, actions, stimuli, or any other consumer-owned runtime object.

## Consumer Binding

Use `QslConsumerBinding` to associate a QSL-visible identifier with an exact consumer-owned object:

```csharp
var binding = new QslConsumerBinding(
    "player",
    "Player",
    player);

engine.BindConsumer(binding);
```

The `Type` string is consumer-provided classification. QSL does not assign semantic meaning to it.

## Native Member Access

Public CLR properties and fields can participate directly in ordinary member syntax:

```qsl
player.Health
soil.Moisture
```

Writable native members can also be assignment targets.

## Attached Runtime State

A host can extend the QSL-visible state of an exact runtime object without changing its CLR type:

```csharp
engine.ReceiveState(
    player,
    "Curiosity",
    7.0);
```

After receipt:

```qsl
player.Curiosity
player.Curiosity = player.Curiosity + 1
```

Before receipt, `player.Curiosity` is unresolved unless `Curiosity` is already a native member.

Attached state is keyed to the exact runtime object, not to its CLR type. Two instances of the same class can therefore carry different attached vocabulary and values.

## State Lifecycle

The public engine surface supports:

```text
ReceiveState -> introduce missing attached state
UpdateState  -> update existing attached state only
RemoveState  -> retire one or all attached members
Clear        -> clear active runtime variables, metadata, and attached state
```

`UnbindConsumer` removes the consumer identifier only if it still points to the exact bound object and also retires attached state associated with that object.

## Packages

`QslStatePackage` batches resolved state receipt for one target.

`QslDecayPackage` carries already-resolved maintenance values for existing attached state. It does not instruct QSL to calculate decay. Numeric zero can remove existing state when the package enables `RemoveAtZero`.

Packages derive from `QslPackageBase`, carry QSL identity/provenance, and expose the shared `IQslPackage` target/property contract.

## Design Rule

Do not add a language literal kind solely to transport a consumer type. Do not require a consumer object to implement QSL contracts merely to participate in member access or attached state.

The host owns domain meaning. QSL owns evaluation over the vocabulary made available to it.
