# QSL Runtime Quick Start

This guide creates a runtime, exposes host state, registers host behavior, evaluates QSL, and inspects the result.

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

## 2. Define Host State with a Descriptor

```csharp
using QSL.Abstractions.Runtime;
using QSL.Core.Base;
using QSL.Core.Runtime.Context;

public sealed class PlayerDescriptor : QslRegistryItem, IQslDescriptor
{
    public override string Name => "Player";
    public override string Type => "descriptor";

    public void Apply(QslRuntimeContext context)
    {
        context.SetVariable("health", 100);
        context.SetVariable("mana", 50);
    }
}
```

## 3. Define Host Behavior with a Function

The following illustrates the integration shape. Production functions should validate domain input and report failures through provenance.

```csharp
using System;
using QSL.Abstractions.Runtime;
using QSL.Core.Base;
using QSL.Core.Provenance;
using QSL.Core.Runtime.Context;

public sealed class HealFunction : QslRegistryItem, IQslFunction
{
    public override string Name => "heal";
    public override string Type => "function";

    public object? Invoke(object?[] args, QslRuntimeContext context)
    {
        if (args.Length != 1)
        {
            AddProvenance(
                QslProvenanceType.Error,
                QslProvenanceCategory.Evaluation,
                Name,
                "heal requires exactly one argument.");
            return null;
        }

        var amount = Convert.ToInt32(args[0]);
        var health = Convert.ToInt32(context.ResolveIdentifier("health") ?? 0);
        var updated = health + amount;

        context.SetVariable("health", updated);
        return updated;
    }
}
```

## 4. Register and Apply Host Extensions

```csharp
var player = new PlayerDescriptor();
var heal = new HealFunction();

registry.RegisterType(player);
registry.RegisterType(heal);

player.Apply(context);
```

Registration makes an extension available to the registry. Applying a descriptor or capability installs its context-specific state or behavior.

## 5. Evaluate

```csharp
var result = engine.Evaluate("heal(10)");
```

The descriptor initializes `health` to `100`; the function adds `10` and returns `110`.

## 6. Inspect the Result

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

## Mental Model

1. Create the registry, context, and engine.
2. Register host extensions.
3. Apply descriptors or capabilities to the target context.
4. Evaluate source through `QslEngine`.
5. Inspect the structured processing result.

The language belongs to QSL. The domain belongs to the host.
