# Unity Integration

QSL is engine-neutral. Unity acts as a host for the same QSL Runtime assembly used by other .NET applications.

## Compatibility

QSL Runtime 1.0 targets .NET Standard 2.1. Use a Unity version and project API compatibility level that support .NET Standard 2.1.

In Unity, verify:

```text
Edit -> Project Settings -> Player -> Other Settings -> API Compatibility Level
```

Select `.NET Standard 2.1` when available and appropriate for the project.

## Managed Plug-in Installation

Place the supplied managed assemblies under a project plug-in folder, for example:

```text
Assets/
└── Plugins/
    └── QSL/
        ├── QSL.dll
        └── required dependency assemblies
```

Unity treats `.dll` files as plug-ins and lets you configure their platform compatibility in the Inspector.

QSL does not reference Unity APIs, so the core project does not require `UnityEngine.dll` to build.

## Dependency Note

QSL Runtime references `System.Text.Json` for provenance JSON export. The Unity distribution must provide or otherwise satisfy the compatible dependency graph validated for the supported Unity version.

Do not copy arbitrary dependency versions into an existing project without checking for conflicts.

## Minimal MonoBehaviour Host

```csharp
using QSL.Core;
using QSL.Core.Runtime;
using QSL.Core.Runtime.Context;
using UnityEngine;

public sealed class QslUnityHost : MonoBehaviour
{
    private QslEngine _engine;
    private QslRuntimeContext _context;

    private void Awake()
    {
        var registry = new QslRuntimeRegistry();
        registry.RegisterDefaultEvaluators();

        _context = new QslRuntimeContext(registry);
        _engine = new QslEngine(_context);
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

## Recommended Unity Boundary

Keep Unity types in Unity integration code.

Prefer this split:

```text
QSL Runtime
    no Unity dependency

Unity Adapter
    MonoBehaviours
    ScriptableObjects
    scene bindings
    Unity vectors and components
```

Convert Unity values to consumer-owned QSL-facing objects or expose them through functions, descriptors, and capabilities.

## Unity Lifecycle Guidance

- construct the registry/context/engine during a deliberate host lifecycle point such as `Awake`
- avoid rebuilding the runtime every frame
- route user-entered or authored source through `QslEngine`
- inspect `QslProcessingResult` rather than assuming evaluation succeeded
- decide deliberately whether contexts are global, scene-level, system-level, or entity-level
- keep QSL evaluation on a thread model compatible with the host operations it invokes

## IL2CPP and Build Validation

Editor success is not sufficient evidence of player-build compatibility. Validate supported targets using actual player builds, including any IL2CPP/AOT targets you intend to advertise.

Reflection-based host behavior and dependency stripping should be tested under the selected build backend.

## Packaging Recommendation

For a paid Asset Store release, package:

- QSL managed runtime and validated dependencies
- Unity adapter/sample code
- assembly definitions where used
- demo scene
- documentation
- dependency and compatibility disclosures

Do not include the CLI certification harness or internal audit reports in the Unity customer package.
