# QSL + Wilds Milestone

**Wilds integration pass, followed by QSL consumer finalization**  
**Working checkpoint: November 2026**

## Current Gate

QSL certification is green: the existing semantic baseline plus the new runtime-state certification. QSL is frozen while the Wilds pass is performed.

---

## 1. Milestone Objective

Prove that Wilds can transport and process the newly certified QSL runtime vocabulary without weakening the separation between the two products.

After the Wilds pass, return to QSL only for consumer-facing API and packaging finalization, not another architecture rewrite.

---

## 2. Gold-Standard Invariant

Keep the Space Dust expression permanently in certification because it demonstrates native consumer state and arbitrary attached runtime vocabulary composing inside ordinary QSL while mutating the exact consumer-owned object:

```qsl
player.Health = player.Health -
    (action.Amount *
    (action.SpaceDustAppetite / 100))
```

This must remain a canonical certification example.

---

# Milestone A: Wilds Pass

**Primary goal:** Certify Wilds as a producer and transport layer for arbitrary consumer-owned objects and QSL runtime packages, while QSL remains the evaluator.

## A1. Audit the Wilds.Qsl integration boundary

- Start with the current `WildsEngine.Qsl` surface.
- Identify the exact request/envelope route into `QslEngine`.
- Preserve the existing:

```text
Pulse -> WorkUnit -> Moment -> Timeline -> Epoch
```

No redesign of the certified Wilds temporal model should be required merely to support the new QSL capabilities.

## A2. Exact package transport

Send a QSL state-package object through Wilds and prove that the exact runtime object/reference survives the processing route rather than being flattened into game-specific fields.

**Gate:** Exact package identity is observable at the QSL handoff.

## A3. Bind consumer-owned objects through the bridge

Carry arbitrary consumer objects such as `player` and `action` through the Wilds request so QSL can bind them without Wilds requiring built-in knowledge of their domain semantics.

Wilds should transport and coordinate the objects. It should not need to understand their application-specific meaning.

## A4. Apply runtime vocabulary package

Use the transported package to attach consumer vocabulary such as:

```text
Amount
SpaceDustAppetite
```

to the appropriate consumer-owned runtime object.

## A5. Space Dust end-to-end proof

Execute the canonical expression through the complete Wilds -> QSL route:

```qsl
player.Health = player.Health -
    (action.Amount *
    (action.SpaceDustAppetite / 100))
```

Starting state:

```text
player.Health = 50

action.Amount = 10
action.SpaceDustAppetite = 87
```

Expected result:

```text
player.Health = 41.3
```

**Gate:** The exact consumer object is mutated and the result is represented honestly in Wilds temporal history.

## A6. Failure honesty

Verify that:

- invalid QSL cannot appear successful;
- rejected state operations cannot appear successful;
- failed WorkUnits cannot produce successful Moments;
- cancelled work cannot become successful temporal history.

## A7. Package provenance continuity

Confirm that package identity/provenance and QSL processing diagnostics remain available at the boundary without inventing a parallel Wilds diagnostic model.

Wilds should preserve what QSL reports rather than reinterpret QSL's internal processing semantics.

## A8. Wilds regression certification

Run the existing Wilds certification suites unchanged after the new integration cases are in place.

**Gate:**

```text
Existing Wilds certification      GREEN
New QSL package transport tests   GREEN
Space Dust end-to-end test        GREEN
```

---

# Milestone B: QSL Consumer Finalization

**Primary goal:** Turn the certified QSL runtime into a clean standalone consumer product surface while keeping its semantics stable.

## B1. First-class consumer function registration

Promote consumer function registration to the `QslEngine` public surface rather than requiring ordinary consumers to understand registry construction.

Candidate surface:

```csharp
engine.RegisterFunction(...);
```

Define deliberately:

- registration;
- duplicate-name behavior;
- replacement policy;
- removal/unregistration;
- ownership and lifetime.

Do not change the underlying `IQslFunction` model merely to create the convenience API.

## B2. Consumer function extension certification

Add focused certification proving that a consumer-owned `IQslFunction` can:

- be registered through the public engine API;
- become callable through normal QSL syntax;
- receive its arguments;
- access the runtime context where appropriate;
- return a value;
- participate in ordinary QSL composition;
- report invalid invocation honestly.

This should become the behavioral counterpart to the Space Dust state test.

## B3. Consumer API coherence audit

Review the public consumer story as one surface:

```text
BindConsumer
UnbindConsumer
ReceiveState
UpdateState
RemoveState
Clear
RegisterFunction
Parse
Evaluate
```

The consumer should not have to reach into runtime implementation machinery for ordinary integration tasks.

## B4. Capability and descriptor exposure decision

Explicitly decide whether capabilities and descriptors:

1. remain advanced registry extensions, or
2. receive matching `QslEngine` convenience APIs.

Do not broaden the public engine surface merely for symmetry. Add APIs only where they improve the real consumer boundary.

## B5. Consumer lifecycle rules

Document and certify compatibility-sensitive lifecycle behavior:

- consumer replacement;
- unbinding;
- attached-state retirement;
- `Clear()` behavior;
- package lifetime;
- function lifetime;
- duplicate function registration;
- function replacement/removal;
- runtime ownership expectations.

Once QSL ships independently, these behaviors become product promises.

## B6. Stable QslProcessingResult contract

Document `QslProcessingResult` as the validated processing product returned by `QslEngine`.

The consumer-facing contract should clearly describe:

- returned value;
- overall success/failure;
- parse failure;
- evaluation failure;
- parser diagnostics;
- runtime/provenance diagnostics;
- AST exposure where intentionally public;
- exception handling.

QSL validates its own processing. It does not claim that consumer-supplied domain facts are semantically true.

## B7. Standalone consumer integration sample

Create one small, host-neutral example that requires no Wilds dependency.

It should demonstrate:

1. ordinary consumer-owned runtime objects;
2. consumer bindings;
3. dynamic attached state;
4. a state package;
5. a consumer-defined function;
6. QSL evaluation;
7. mutation of the original consumer object;
8. processing-result inspection.

This sample should make the standalone QSL product boundary obvious.

## B8. Golden examples

Keep two permanent examples in documentation and certification.

### Dynamic vocabulary: Space Dust

```qsl
player.Health = player.Health -
    (action.Amount *
    (action.SpaceDustAppetite / 100))
```

This proves that QSL does not need compile-time knowledge of future consumer vocabulary.

### Consumer behavior

Add one equally durable consumer-function example proving that consumers can supply reusable application behavior without modifying the QSL language.

Together, these express the long-term model:

```text
Consumer supplies nouns
    objects + attached runtime vocabulary

Consumer supplies verbs
    reusable functions + capabilities

QSL supplies grammar and execution
```

## B9. Release packaging

Finalize the standalone QSL product package:

- supported TFMs/platforms;
- package contents;
- public namespaces;
- semantic/versioning policy;
- installation path;
- licensing text;
- API documentation;
- sample project;
- packaged-artifact smoke tests.

## B10. Release-mode certification

Run the complete certification against the release artifact, not only against a source-tree Debug build.

**Gate:**

```text
Existing semantic certification   GREEN
Runtime-state certification       GREEN
Consumer-function certification   GREEN
Packaged artifact smoke test      GREEN
```

---

# 5. Product Boundary to Preserve

## QSL

QSL consumes:

- bound consumer-owned objects;
- native object state;
- attached runtime state;
- state/decay packages;
- consumer-defined functions;
- capabilities/descriptors where registered;
- QSL source.

QSL owns:

- parsing;
- language semantics;
- evaluation;
- composition;
- allowed consumer mutation;
- processing results;
- QSL diagnostics and provenance.

QSL does **not** need to understand what application-specific concepts such as `SpaceDustAppetite` mean.

## Wilds

Wilds may:

- receive consumer requests;
- resolve simulation context;
- decompose requests into work;
- produce or transport runtime vocabulary/packages;
- route QSL processing;
- preserve arbitrary runtime identity;
- record honest temporal outcomes.

Wilds should not make QSL vocabulary game-specific.

## Combined model

```text
Consumer / World
       |
       v
     Wilds
resolve / coordinate / transport
       |
       v
      QSL
parse / evaluate / mutate / validate processing
       |
       v
Consumer-owned runtime + QslProcessingResult
```

QSL must remain useful without Wilds.

Wilds must remain conceptually broader than QSL.

Their integration should make each stronger without turning either one into an implementation detail of the other.

---

# 6. Execution Order

1. Freeze QSL at the current certified checkpoint.
2. Perform the Wilds transport/integration pass.
3. Certify QSL state-package transport through Wilds.
4. Certify the Space Dust path through Wilds.
5. Run the existing Wilds suites unchanged.
6. Return to QSL for consumer API finalization only.
7. Add first-class consumer-function registration.
8. Add consumer-function certification.
9. Complete the public consumer API/lifecycle audit.
10. Package and certify the standalone QSL release artifact.

---

# 7. Definition of Done

This milestone closes when:

- Wilds can carry arbitrary QSL-facing consumer/runtime packages through its certified processing and temporal route;
- exact consumer/runtime identity is preserved where required;
- the canonical Space Dust expression succeeds end-to-end through Wilds and QSL;
- failures remain failures throughout the complete route;
- QSL exposes a coherent standalone consumer API;
- consumer-defined reusable functions have a first-class public registration path;
- runtime state and consumer-function extension paths are certified;
- the release artifact itself passes certification/smoke testing;
- neither QSL nor Wilds requires built-in domain knowledge of future consumer vocabulary.

## Permanent rule

> **Never delete the Space Dust test.**

It is the compact proof that QSL can compute over a concept that did not exist when QSL was compiled, combine it with native consumer state, and mutate the exact consumer-owned object without changing the language.
