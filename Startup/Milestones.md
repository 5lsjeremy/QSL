# QSL + Wilds Milestone

**Wilds integration pass, followed by QSL consumer finalization**  
**Working checkpoint: November 2026**

## Current Gate

QSL remains green at its certified semantic/runtime checkpoint. The Wilds integration pass is now substantially complete.

Current certified integration status:

- [x] Wilds/QSL integration boundary audited.
- [x] `WildsQslRequest` request envelope established.
- [x] Consumer-owned objects bind through the Wilds/QSL bridge.
- [x] `QslStatePackage` attached runtime vocabulary is transported and consumed through the bridge.
- [x] Native consumer state and attached runtime vocabulary compose in ordinary QSL.
- [x] The canonical Space Dust expression mutates the exact consumer-owned object from `50` to `41.3`.
- [x] Wilds owns the active Pulse/WorkUnit lifecycle around delegated QSL processing.
- [x] The QSL result returns to Wilds and resolves into Moment, Timeline, and Epoch history.
- [x] Existing QSL core certification remains green at **156/156**.
- [x] Full Release Wilds certification remains green at **50/50**.
- [ ] Add one focused package provenance/diagnostic continuity certification before formally closing Milestone A.

The integration pass established an important product boundary: **Wilds is the exposed/front-door surface for the combined Wilds/QSL route. QSL performs delegated computation inside a Wilds-owned request lifecycle.**

---

## 1. Milestone Objective

Prove that Wilds can transport and process the newly certified QSL runtime vocabulary without weakening the separation between the two products.

That objective is now substantially proven. Wilds can carry consumer-owned objects, QSL state packages, and QSL source through its certified request lifecycle while remaining independent of consumer domain semantics and QSL implementation details.

After Milestone A closes, return to QSL only for standalone consumer/product finalization and packaging, not another architecture rewrite.

---

## 2. Gold-Standard Invariant

Keep the Space Dust expression permanently in certification because it demonstrates native consumer state and arbitrary attached runtime vocabulary composing inside ordinary QSL while mutating the exact consumer-owned object:

```qsl
player.Health = player.Health -
    (action.Amount *
    (action.SpaceDustAppetite / 100))
```

Certified starting state:

```text
player.Health = 50
action.Amount = 10
action.SpaceDustAppetite = 87
```

Certified result:

```text
player.Health = 41.3
```

This remains a canonical/golden certification example.

---

# Milestone A: Wilds Pass

**Primary goal:** Certify Wilds as the exposed coordination and transport surface for arbitrary consumer-owned objects and QSL runtime packages, while QSL remains the delegated evaluator.

## A1. Audit the Wilds.Qsl integration boundary - COMPLETE

- [x] Audited the current `WildsEngine.Qsl` surface.
- [x] Identified the exact request-envelope route into `QslEngine`.
- [x] Established `WildsQslRequest` as the integration envelope carrying source, consumer bindings, and state packages.
- [x] Preserved the existing temporal route without redesign:

```text
Pulse -> WorkUnit -> Moment -> Timeline -> Epoch
```

The certified execution order is more precisely:

```text
Consumer request
       |
       v
     Wilds
       |
       v
     Pulse
       |
       v
   WorkUnit
       |
       v
QSL-backed delegated work
       |
       v
   QSL result
       |
       v
WorkUnit completion
       |
       v
    Moment
       |
       v
   Timeline
       |
       v
     Epoch
```

Wilds establishes the Pulse and WorkUnit before QSL evaluates anything. QSL returns the delegated computation result to Wilds. Wilds then completes the WorkUnit and finalizes the terminal Moment.

`WildsQslExecutor` is integration machinery behind the Wilds-facing boundary, not the intended consumer front door.

## A2. Exact package transport - COMPLETE

- [x] A `QslStatePackage` is carried inside `WildsQslRequest` through Wilds.
- [x] The package continues to target the exact consumer-owned runtime object.
- [x] Consumer/runtime objects are not flattened into Wilds-specific gameplay fields.
- [x] Wilds Core remains ignorant of package vocabulary and application-specific meaning.

**Gate:** PASSED. Exact package target identity survives to the QSL integration handoff.

## A3. Bind consumer-owned objects through the bridge - COMPLETE

- [x] Arbitrary consumer objects such as `player` and `action` travel through the Wilds request.
- [x] QSL binds the exact supplied objects through `QslConsumerBinding`.
- [x] Wilds does not require built-in knowledge of `Player`, `Action`, or their meanings.

Wilds transports and coordinates the objects. QSL evaluates against them. The consumer retains ownership of their domain semantics.

## A4. Apply runtime vocabulary package - COMPLETE

The certified route composes native and attached runtime vocabulary on the same consumer object.

Certified example:

```text
action.Amount = 10                  native consumer state
action.SpaceDustAppetite = 87       attached QSL runtime state
```

Focused certification proves:

```qsl
action.Amount + action.SpaceDustAppetite
```

produces:

```text
97
```

- [x] Native consumer state resolves correctly.
- [x] Attached runtime vocabulary resolves correctly.
- [x] Both compose in an ordinary QSL expression.

## A5. Space Dust end-to-end proof - COMPLETE

The canonical expression is certified through the complete Wilds/QSL route:

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

Certified result:

```text
player.Health = 41.3
```

- [x] The exact consumer-owned Player is mutated.
- [x] Native `Action.Amount` composes with attached `SpaceDustAppetite`.
- [x] QSL returns `41.3` to the Wilds request route.
- [x] Wilds records the successful result as a completed Moment.
- [x] The exact resulting Moment is retained through Timeline and Epoch history.

**Gate:** PASSED.

A separate end-to-end architectural regression test now explicitly proves that QSL work executes inside the Wilds-owned lifecycle and that Wilds resolves the QSL result only after delegated computation returns.

## A6. Failure honesty - COMPLETE

Verified by the existing Wilds and Wilds/QSL certification surfaces:

- [x] Invalid QSL cannot appear successful.
- [x] QSL processing failure becomes failed Wilds work/history rather than a successful result.
- [x] Failed WorkUnits cannot produce successful Moments.
- [x] Cancelled WorkUnits cannot produce successful temporal history.
- [x] Certification distinguishes expected runtime failure from certification failure.

The Wilds/QSL core suite currently passes **156/156** while retaining **25 expected failed Moments** for cases whose correct runtime result is failure.

## A7. Package provenance continuity - OPEN / FINAL A-GATE

Confirm explicitly that package identity/provenance and QSL processing diagnostics remain available at the integration boundary without inventing a parallel Wilds diagnostic model.

Already established:

- QSL owns its parser/runtime diagnostic and provenance semantics.
- `QslProcessingResult` remains the QSL processing product.
- `WildsQslExecutionException` carries the unsuccessful `QslProcessingResult` into Wilds failure handling rather than translating QSL failure into an unrelated diagnostic representation.

Still to certify directly:

- [ ] focused package provenance/identity continuity assertion where applicable;
- [ ] focused assertion that QSL processing diagnostics remain accessible on the failed Wilds/QSL route;
- [ ] confirm no redundant Wilds diagnostic/provenance model is introduced for QSL internals.

**Close Milestone A after this focused gate passes.**

## A8. Wilds regression certification - COMPLETE

All current gates are green:

```text
Existing QSL core certification       156/156 GREEN
WildsQslRequest envelope tests        GREEN
Consumer binding/state composition    GREEN
Space Dust end-to-end proof           GREEN
Wilds/QSL lifecycle end-to-end test   GREEN
Full Wilds Release suite              50/50 GREEN
```

One unrelated Release warning remains:

```text
CS8604 - NeighborTraversal.cs
```

It is not part of the Wilds/QSL request-envelope integration.

---

# Milestone B: QSL Standalone Consumer Finalization

**Primary goal:** Finalize QSL as a clean standalone product surface while preserving the now-certified Wilds integration boundary.

This milestone no longer needs to solve the combined Wilds/QSL consumer-routing problem. The Wilds pass demonstrated that consumer-owned objects and runtime vocabulary can enter through Wilds, be delegated to QSL, mutate exact consumer state, and return to honest Wilds history.

For the combined product model:

> **Wilds is the exposed surface. QSL is delegated computation.**

Milestone B therefore concerns QSL's value as an independently usable product and the APIs required when QSL is intentionally consumed without Wilds.

## B1. First-class standalone consumer function registration - OPEN

If standalone QSL consumers should register functions without understanding runtime registry construction, promote consumer function registration to an intentional `QslEngine` surface.

Candidate:

```csharp
engine.RegisterFunction(...);
```

Define deliberately:

- registration;
- duplicate-name behavior;
- replacement policy;
- removal/unregistration;
- ownership and lifetime.

Do not change the underlying `IQslFunction` model merely to create convenience APIs.

This API is for the standalone QSL product boundary. Wilds/QSL consumers should not be required to bypass Wilds and construct QSL integration machinery directly.

## B2. Consumer function extension certification - OPEN

Add focused standalone-QSL certification proving that a consumer-owned `IQslFunction` can:

- be registered through the intended public QSL API;
- become callable through normal QSL syntax;
- receive its arguments;
- access runtime context where appropriate;
- return a value;
- participate in ordinary QSL composition;
- report invalid invocation honestly.

This becomes the behavioral counterpart to the Space Dust state example for standalone QSL.

## B3. Standalone QSL API coherence audit - OPEN

Review the direct `QslEngine` story specifically for hosts intentionally using QSL without Wilds:

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

Do not use this audit to move the combined Wilds/QSL front door away from Wilds.

The standalone QSL consumer should not need to reach into runtime implementation machinery for ordinary QSL integration tasks.

## B4. Capability and descriptor exposure decision - OPEN

Explicitly decide whether standalone-QSL capabilities and descriptors:

1. remain advanced registry extensions, or
2. receive matching `QslEngine` convenience APIs.

Do not broaden the public engine surface merely for symmetry. Add APIs only where they improve the real standalone product boundary.

## B5. Consumer lifecycle rules - OPEN

Document and certify compatibility-sensitive standalone QSL behavior:

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

## B6. Stable QslProcessingResult contract - OPEN

Document `QslProcessingResult` as the validated processing product returned by `QslEngine`.

The contract should clearly describe:

- returned value;
- overall success/failure;
- parse failure;
- evaluation failure;
- parser diagnostics;
- runtime/provenance diagnostics;
- AST exposure where intentionally public;
- exception handling.

QSL validates its own processing. It does not claim that consumer-supplied domain facts are semantically true.

The Wilds/QSL integration may preserve and carry this result on failure, but Wilds should not reinterpret QSL's internal processing semantics.

## B7. Standalone consumer integration sample - OPEN

Create one small host-neutral QSL example requiring no Wilds dependency.

It should demonstrate:

1. ordinary consumer-owned runtime objects;
2. consumer bindings;
3. dynamic attached state;
4. a state package;
5. a consumer-defined function;
6. QSL evaluation;
7. mutation of the original consumer object;
8. processing-result inspection.

The purpose of this sample is specifically to demonstrate the independently usable QSL product boundary. The combined Wilds/QSL sample should continue to enter through Wilds.

## B8. Golden examples - PARTIALLY COMPLETE

Keep two permanent examples in documentation and certification.

### Dynamic vocabulary: Space Dust - COMPLETE

```qsl
player.Health = player.Health -
    (action.Amount *
    (action.SpaceDustAppetite / 100))
```

This now proves both:

- QSL does not need compile-time knowledge of future consumer vocabulary; and
- Wilds can transport the required consumer context and runtime vocabulary through its certified lifecycle without domain-specific Core knowledge.

**Permanent certified result:** `Player.Health: 50 -> 41.3`.

### Consumer behavior - OPEN

Add one equally durable standalone consumer-function example proving that consumers can supply reusable application behavior without modifying the QSL language.

Together, the long-term standalone QSL model remains:

```text
Consumer supplies nouns
    objects + attached runtime vocabulary

Consumer supplies verbs
    reusable functions + capabilities

QSL supplies grammar and execution
```

For Wilds/QSL integration, Wilds additionally supplies the exposed request, coordination, and history boundary around that computation.

## B9. Release packaging - OPEN

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

## B10. Release-mode certification - OPEN

Run complete standalone QSL certification against the release artifact, not only against a source-tree Debug build.

**Gate:**

```text
Existing semantic certification   GREEN
Runtime-state certification       GREEN
Consumer-function certification   PENDING
Packaged artifact smoke test      PENDING
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

QSL processes the valid source and runtime context it is given. It does not decide whether an occurrence should propagate through Wilds.

## Wilds

Wilds is the exposed/front-door surface for the combined integration.

Wilds owns or may:

- receive consumer requests;
- establish the Pulse and WorkUnit lifecycle;
- resolve simulation context;
- decompose requests into work;
- produce or transport runtime vocabulary/packages;
- delegate QSL-backed work;
- preserve arbitrary runtime identity;
- complete WorkUnits from delegated results;
- resolve terminal Moments;
- retain Timeline and Epoch history;
- own relationship, propagation, eligibility, and causal execution policy.

Wilds should not make QSL vocabulary game-specific.

`WildsQslExecutor` and `QslEngine` are behind the Wilds-facing boundary for the combined integration. Certification may construct those pieces directly to prove the handshake, but that construction path is not the intended consumer-facing product model.

## Propagation and QSL processing

Propagation and QSL evaluation are separate concerns.

```text
QSL-backed local work
       |
       v
QSL evaluates
       |
       v
Wilds resolves result
       |
       +--> remain local
       |
       `--> propagate when Wilds policy says propagation applies
```

A non-propagating occurrence may still require QSL processing. `Propagate` should therefore not be treated as permission for QSL to evaluate valid delegated work.

## Combined model

```text
Consumer / World
       |
       v
     Wilds
request / coordinate / transport
       |
       v
Pulse + WorkUnit
       |
       v
QSL-backed delegated computation
       |
       v
QSL parse / evaluate / mutate
       |
       v
result returns to Wilds
       |
       v
WorkUnit -> Moment -> Timeline -> Epoch
```

QSL must remain useful without Wilds.

Wilds must remain conceptually broader than QSL.

Their integration should make each stronger without moving consumer domain meaning into either engine core.

---

# 6. Execution Order

1. [x] Freeze QSL at the certified runtime-state checkpoint.
2. [x] Perform the Wilds transport/integration pass.
3. [x] Establish and certify the `WildsQslRequest` envelope.
4. [x] Certify consumer bindings through Wilds.
5. [x] Certify QSL state-package/runtime-vocabulary transport through Wilds.
6. [x] Certify native + attached state composition.
7. [x] Certify Space Dust through Wilds and QSL (`50 -> 41.3`).
8. [x] Add the explicit Wilds/QSL lifecycle end-to-end regression test.
9. [x] Run existing QSL core certification: **156/156 GREEN**.
10. [x] Run full Wilds Release certification: **50/50 GREEN**.
11. [ ] Add focused package provenance/diagnostic continuity certification.
12. [ ] Formally close Milestone A.
13. [ ] Re-enter QSL only for standalone consumer/API/product finalization.
14. [ ] Decide and implement first-class consumer-function registration if retained as a standalone requirement.
15. [ ] Add consumer-function certification/golden example.
16. [ ] Complete standalone QSL API/lifecycle audit.
17. [ ] Package and certify the standalone QSL release artifact.

---

# 7. Definition of Done

## Milestone A closes when

- [x] Wilds can carry arbitrary QSL-facing consumer/runtime packages through its certified processing and temporal route.
- [x] Exact consumer/runtime identity is preserved where required.
- [x] Consumer bindings reach QSL without Wilds requiring domain knowledge.
- [x] Attached runtime vocabulary reaches the exact target object.
- [x] The canonical Space Dust expression succeeds end-to-end through Wilds and QSL.
- [x] Space Dust mutates the exact Player from `50` to `41.3`.
- [x] QSL results return to Wilds and become honest temporal history.
- [x] Failures and cancellations remain honest throughout the route.
- [x] Existing QSL core certification remains **156/156**.
- [x] Full Wilds Release certification remains **50/50**.
- [ ] Package provenance/diagnostic continuity receives its focused certification.

## Full QSL + Wilds milestone closes when

- [x] the Wilds/QSL runtime-state integration boundary is certified;
- [ ] Milestone A provenance/diagnostic continuity is closed;
- [ ] QSL exposes the intended coherent standalone consumer API;
- [ ] consumer-defined reusable functions have a first-class public path if retained as a standalone product requirement;
- [ ] runtime-state and consumer-function extension paths are certified;
- [ ] the standalone QSL release artifact passes certification/smoke testing;
- [x] neither QSL nor Wilds requires built-in domain knowledge of future consumer vocabulary.

## Permanent rules

> **Never delete the Space Dust test.**

It is the compact proof that QSL can compute over a concept that did not exist when QSL was compiled, combine it with native consumer state, and mutate the exact consumer-owned object without changing the language.

> **Wilds is the front door for the combined integration.**

Wilds owns the request lifecycle and temporal truth. QSL performs delegated computation inside that lifecycle.
