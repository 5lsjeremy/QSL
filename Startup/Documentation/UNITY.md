# Unity Integration

QSL is engine-neutral. Unity acts as a host for the same QSL Runtime assembly used by other .NET applications.

## Compatibility

QSL Runtime 1.0 targets .NET Standard 2.1. Use a Unity version and project API compatibility level that support .NET Standard 2.1.

## Minimal MonoBehaviour Host

```csharp
using QSL.Core;
using QSL.Core.Runtime;
using QSL.Core.Runtime.Context;
using UnityEngine;

public sealed class QslUnityHost : MonoBehaviour
{
    private QslEngine _engine;

    private void Awake()
    {
        var registry = new QslRuntimeRegistry();
        registry.RegisterDefaultEvaluators();

        var context = new QslRuntimeContext(registry);
        _engine = new QslEngine(context);
    }

    public void Execute(string source)
    {
        var result = _engine.Evaluate(source);

        if (result.Succeeded)
        {
            Debug.Log(result.Value);
            return;
        }

        foreach (var diagnostic in result.ParserDiagnostics)
            Debug.LogError($"QSL parser: {diagnostic}");

        foreach (var diagnostic in result.RuntimeDiagnostics)
            Debug.LogError($"QSL runtime: {diagnostic.Message}");

        if (result.Exception != null)
            Debug.LogException(result.Exception);
    }
}
```

## Consumer Objects and Runtime State

Keep Unity types and domain semantics in Unity/host integration code. Bind consumer-owned objects and supply resolved runtime state through `QslEngine` rather than requiring those objects to implement QSL contracts.

`QslConsumerBinding` can expose an exact object under a QSL-visible identifier. Runtime-attached state can then be received individually or through packages.

## Recommended Unity Boundary

```text
QSL Runtime
    no Unity dependency

Unity Adapter
    MonoBehaviours
    ScriptableObjects
    scene bindings
    Unity vectors and components
    consumer binding / resolved state preparation
```

## Unity Lifecycle Guidance

- construct the registry/context/engine during a deliberate host lifecycle point such as `Awake`
- avoid rebuilding the runtime every frame
- bind and retire consumer vocabulary deliberately
- route user-entered or authored source through `QslEngine`
- inspect `QslProcessingResult` rather than assuming evaluation succeeded
- decide deliberately whether runtime contexts are global, scene-level, system-level, or entity-level
- keep QSL evaluation on a thread model compatible with host operations

## Build Validation

Editor success is not sufficient evidence of player-build compatibility. Validate supported targets using actual player builds, including any IL2CPP/AOT targets you intend to advertise. Reflection-based host behavior and dependency stripping should be tested under the selected build backend.
