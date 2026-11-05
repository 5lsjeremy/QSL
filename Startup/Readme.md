# QSL Runtime

**Runtime expressions and host-defined intent for .NET applications, games, and simulations.**

QSL is an embeddable runtime language from **TethraSolutions, LLC**. It parses and evaluates expressions against application-owned state, objects, functions, descriptors, capabilities, and runtime-attached vocabulary.

Your application owns the domain. QSL owns the language and evaluation runtime.

## Why QSL?

A host can expose its own vocabulary without teaching QSL the domain:

```qsl
health < 30 ? heal(20) : patrol()
```

QSL does not define `health`, `heal`, or `patrol`. The host does.

QSL can also bind an exact consumer-owned object and work with its native members:

```qsl
player.Health
player.Health = player.Health + action.Amount
```

Runtime state can be attached later without changing the consumer object's CLR type. Until that state is received through the runtime surface, the member is not part of QSL-visible vocabulary.

```text
action.Amount
    -> unresolved

ReceiveState(action, "Amount", 10)
    -> action.Amount is now QSL-visible
```

## Runtime Vocabulary

QSL supports two complementary forms of object state:

- **Native state**: public CLR properties or fields already exposed by the exact consumer-owned object.
- **Attached state**: runtime state explicitly received for that exact object and retained by `QslAttachedStateStore`.

Ordinary QSL member syntax is shared across both:

```qsl
player.Health
player.Curiosity
action.Amount
```

QSL does not assign domain meaning to those names. The host determines which consumer objects and resolved state values enter the runtime.

## Public Integration Surface

`QslEngine` is the primary public gateway. In addition to parsing and evaluation, it supports the runtime vocabulary lifecycle:

- `BindConsumer(QslConsumerBinding)`
- `UnbindConsumer(QslConsumerBinding)`
- `ReceiveState(object, string, object?)`
- `ReceiveState(QslStatePackage)`
- `UpdateState(object, string, object?)`
- `ReceiveDecay(QslDecayPackage)`
- `RemoveState(object, string)`
- `RemoveState(object)`
- `Clear()`
- `Parse(string)`
- `Evaluate(string)`

`QslConsumerBinding` supplies a QSL-visible name, a consumer-owned type classification, and the exact runtime object reference.

Packages carry already-resolved values for an exact target. QSL does not reproduce the upstream reasoning that produced those values.

## State Packages

`IQslPackage` and `QslPackageBase` define the common package shape:

```text
Target
Properties
QSL object identity / provenance
```

`QslStatePackage` batches approved state receipt. Existing attached state remains non-destructive on receipt.

`QslDecayPackage` carries already-resolved maintenance values. Nonzero values update existing attached state; numeric zero can retire existing state when `RemoveAtZero` is enabled. QSL does not calculate decay.

Packages inherit QSL identity and provenance so package creation and application can be traced independently from the per-property state lifecycle recorded by the runtime context.

## Processing Surface

`RegisterDefaultEvaluators()` installs support for:

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

Member access resolves supported special QSL values, native CLR properties/fields, and received attached state. Assignment can mutate writable native members or already-existing attached state.

## Core Architecture

```text
External host / integration
    -> consumer bindings
    -> resolved state / packages
    -> QslEngine
    -> QslRuntimeContext
    -> native + attached vocabulary
    -> tokenizer / parser / AST
    -> evaluator dispatch
    -> QslProcessingResult
```

The host owns semantic validation and meaning. QSL accepts the vocabulary made available through its supported runtime surface and computes against it.

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
```

See [QUICKSTART.md](QUICKSTART.md) for consumer binding and runtime-state examples.

## Diagnostics

`QslEngine.Evaluate` returns a `QslProcessingResult` containing `Value`, parser diagnostics, runtime diagnostics, any exception, and success/failure state. Parser diagnostics remain separate from provenance-backed runtime diagnostics.

Package lifecycle events and runtime-state lifecycle operations also emit provenance through the existing QSL provenance model.

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

- [Runtime Objects and State](Documentation/RUNTIME_OBJECTS.md)
- [Functions](Documentation/FUNCTIONS.md)
- [Descriptors](Documentation/DESCRIPTORS.md)
- [Capabilities](Documentation/CAPABILITIES.md)
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
