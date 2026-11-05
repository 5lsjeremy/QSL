# QSL Runtime Quick Start

This guide creates a runtime, binds an ordinary consumer-owned object, supplies runtime state, evaluates QSL, and inspects the result.

## 1. Create the Runtime

```csharp
using QSL.Core;
using QSL.Core.Runtime;
using QSL.Core.Runtime.Context;

var registry = new QslRuntimeRegistry();
registry.RegisterDefaultEvaluators();

var context = new QslRuntimeContext(registry);
var engine = new QslEngine(context);
```

## 2. Bind a Consumer-Owned Object

The object does not implement a QSL contract:

```csharp
public sealed class Player
{
    public double Health { get; set; } = 50.0;
}

var player = new Player();

var binding = new QslConsumerBinding(
    "player",
    "Player",
    player);

engine.BindConsumer(binding);
```

`Name` is the QSL-visible identifier, `Type` is consumer-owned classification, and `Value` is the exact runtime object.

QSL can immediately read native public members:

```qsl
player.Health
```

## 3. Receive Runtime State

Attached state is not invented by member access or assignment. It becomes available only after it is received through the runtime state surface.

```csharp
engine.ReceiveState(
    player,
    "Curiosity",
    7.0);
```

Now QSL can evaluate:

```qsl
player.Curiosity
```

and can update that already-existing attached state:

```qsl
player.Curiosity = player.Curiosity + 3
```

## 4. Receive a State Package

Multiple already-approved values can be batched:

```csharp
var package = new QslStatePackage(
    player,
    new Dictionary<string, object?>
    {
        ["Interest"] = 5.0,
        ["Suspicion"] = 2.0
    });

var received = engine.ReceiveState(package);
```

Package receipt uses the same non-destructive semantics as single-state receipt. The returned count is the number of newly received properties.

## 5. Apply Resolved Maintenance Values

A decay package contains values that have already been resolved upstream. QSL does not calculate the decay.

```csharp
var decay = new QslDecayPackage(
    player,
    new Dictionary<string, object?>
    {
        ["Interest"] = 2.5,
        ["Suspicion"] = 0.0
    });

var affected = engine.ReceiveDecay(decay);
```

With `RemoveAtZero` enabled, `Interest` is updated to `2.5` and `Suspicion` is removed. A decay package cannot introduce missing attached state.

## 6. Evaluate

```csharp
var result = engine.Evaluate(
    "player.Health = player.Health - player.Interest");
```

## 7. Inspect the Result

```csharp
if (result.Succeeded)
{
    Console.WriteLine($"Result: {result.Value}");
}
else
{
    foreach (var diagnostic in result.ParserDiagnostics)
        Console.WriteLine($"Parser: {diagnostic}");

    foreach (var diagnostic in result.RuntimeDiagnostics)
        Console.WriteLine($"Runtime: {diagnostic.Message}");

    if (result.Exception != null)
        Console.WriteLine($"Exception: {result.Exception.Message}");
}
```

## 8. Retire Runtime Vocabulary

Remove one attached member:

```csharp
engine.RemoveState(
    player,
    "Curiosity");
```

Remove all attached state for the exact object:

```csharp
engine.RemoveState(player);
```

Unbind the exact consumer object and retire its attached state:

```csharp
engine.UnbindConsumer(binding);
```

Or clear active runtime variables, metadata, and attached state while preserving the configured registry:

```csharp
engine.Clear();
```

## Mental Model

1. Create the registry, context, and engine.
2. Bind exact consumer-owned objects through `QslEngine`.
3. Receive already-approved/resolved runtime state, individually or in packages.
4. Evaluate ordinary QSL against native and attached vocabulary.
5. Update, remove, decay, unbind, or clear state through the public engine lifecycle.
6. Inspect structured processing results and provenance.

The language belongs to QSL. The domain, semantic meaning, validation, and upstream calculation belong to the host.
