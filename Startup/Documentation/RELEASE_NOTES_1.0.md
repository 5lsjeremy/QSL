# QSL Runtime 1.0 Release Notes

QSL Runtime 1.0 is the first commercial runtime release from TethraSolutions, LLC.

## Positioning

QSL provides an embeddable expression and intent runtime while leaving application state, semantic meaning, and upstream policy with the host.

## Included

- core runtime assembly targeting .NET Standard 2.1
- public engine, runtime, context, registry, and result surfaces
- exact consumer-object binding
- native and runtime-attached object state
- state/package lifecycle operations
- state and decay package foundation with package provenance
- host extension contracts
- provenance diagnostics
- documentation and integration examples
- Unity hosting guidance

## Runtime-State Boundary

QSL can compute against native consumer-object members and attached state explicitly received through the runtime surface. Attached state is retained against exact object identity without requiring consumer types to implement QSL contracts.

Packages carry resolved values. QSL does not calculate host-specific propagation, eligibility, decay, or other upstream policy merely because a package represents such a lifecycle event.

## Important Boundary

QSL Runtime does not include Wilds, Stratyx, Cryo Nation content, the CLI certification harness, or internal audit reports.

## Dependency

QSL Runtime uses `System.Text.Json` 8.0.5 for provenance JSON export.

## Validation

Before public release, validate the packaged artifact through the established QSL certification suite and supported host smoke tests, including native-object and attached-state conformance coverage.
