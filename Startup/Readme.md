# QSL Runtime

**Runtime expressions and host-defined intent for .NET applications, games, and simulations.**

QSL is an embeddable runtime language from **TethraSolutions, LLC**. It parses and evaluates expressions against application-owned state, objects, functions, descriptors, and capabilities.

Your application owns the domain. QSL owns the language and evaluation runtime.

## Why QSL?

A host can expose its own vocabulary:

```qsl
health < 30 ? heal(20) : patrol()
```

QSL does not define `health`, `heal`, or `patrol`. The host does.

Another host can expose a different domain without modifying QSL:

```qsl
soil.moisture < 0.25 ? irrigate(field) : wait()
```

## Processing Surface

`RegisterDefaultEvaluators()` installs support for these AST structures:

- Literal
- Identifier
- Unary
- Binary
- Call
- Embedded expression
- Interpolated string
- Block
- Error
- Member access
- Ternary

Examples:

```qsl
10
health
!active
health + regeneration
heal(10)
player.health
health < 30 ? heal(20) : patrol()
{ heal(10); patrol(); }
"Health: $(health)"
```

See [PROCESSING.md](Documentation/PROCESSING.md) for composition details.

## Core Architecture

```text
QSL Source
    -> QslEngine
    -> tokenizer and parser
    -> AST
    -> QslRuntimeContext
    -> evaluator dispatch
    -> host state and behavior
    -> QslProcessingResult
```

- `QslEngine` is the public parsing and evaluation gateway.
- `QslRuntimeRegistry` defines the evaluators and host extensions available to QSL.
- `QslRuntimeContext` provides the state and resolution environment for an evaluation.
- Descriptors and capabilities apply host-owned data or behavior to a context.
- Functions expose host-owned operations.
- Evaluators interpret AST nodes against the current context.

## Quick Start

```csharp
using QSL.Core;
using QSL.Core.Runtime;
using QSL.Core.Runtime.Context;

var registry = new QslRuntimeRegistry();
registry.RegisterDefaultEvaluators();

var context = new QslRuntimeContext(registry);
var engine = new QslEngine(context);

var result = engine.Evaluate("10 + 20 * 3");

if (result.Succeeded)
    Console.WriteLine(result.Value);
```

See [QUICKSTART.md](Documentation/QUICKSTART.md) for a host extension example.

## Host Extension Surfaces

### Functions

Functions expose operations such as:

```qsl
heal(10)
```

### Descriptors

Descriptors implement `IQslDescriptor` and apply host-owned state to a `QslRuntimeContext`.

```csharp
public void Apply(QslRuntimeContext context)
{
    context.SetVariable("health", 100);
}
```

### Capabilities

Capabilities implement `IQslCapability` and apply a coherent group of host-owned behavior to a context. A host might provide a `PhysicsCapability`, economy capability, navigation capability, or another domain-specific subsystem.

### Evaluators

Evaluators implement `IQslEvaluator`:

```csharp
object? Evaluate(IQslNode node, QslRuntimeContext context);
```

Most consumers use the standard evaluator set. Advanced integrations can register an evaluator for a specific `QslNodeKind`.

## Diagnostics

`QslEngine.Evaluate` returns a `QslProcessingResult` containing:

- `Value`
- `ParserDiagnostics`
- `RuntimeDiagnostics`
- `Exception`
- `Succeeded`

Parser diagnostics remain separate from runtime diagnostics. Runtime evaluation errors are collected through provenance.

## Requirements

- .NET Standard 2.1
- `System.Text.Json` 8.0.5

## Documentation

### Start Here

- [Installation](Documentation/INSTALLATION.md)
- [Quick Start](QUICKSTART.md)
- [Runtime Architecture](Documentation/RUNTIME.md)
- [Processing Guide](Documentation/PROCESSING.md)

### Host Integration

- [Functions](Documentation/FUNCTIONS.md)
- [Descriptors](Documentation/DESCRIPTORS.md)
- [Capabilities](Documentation/CAPABILITIES.md)
- [Runtime Objects](Documentation/RUNTIME_OBJECTS.md)
- [Diagnostics and Provenance](Documentation/DIAGNOSTICS.md)

### Language and Advanced Integration

- [Language Reference](Documentation/LANGUAGE_REFERENCE.md)
- [Evaluators](Documentation/EVALUATORS.md)
- [Parsing](Documentation/PARSING.md)
- [Tokenization](Documentation/TOKENIZATION.md)
- [Advanced Integration](Documentation/ADVANCED_INTEGRATION.md)
- [API Reference](Documentation/API_REFERENCE.md)

### Hosts and Product Information

- [Unity Integration](Documentation/UNITY.md)
- [Support](Documentation/SUPPORT.md)
- [Security](Documentation/SECURITY.md)
- [Changelog](Documentation/CHANGELOG.md)
- [QSL Runtime 1.0 Release Notes](Documentation/RELEASE_NOTES_1.0.md)

## License

QSL Runtime is proprietary software. Use and distribution are governed by the commercial license supplied with the product.

Copyright © 2026 TethraSolutions, LLC. All rights reserved.
