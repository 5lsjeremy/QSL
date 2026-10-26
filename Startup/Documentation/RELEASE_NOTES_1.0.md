# QSL Runtime 1.0 Release Notes

QSL Runtime 1.0 is the first commercial runtime release from TethraSolutions, LLC.

## Positioning

QSL provides an embeddable expression and intent runtime while leaving application state and domain semantics with the host.

## Included

- core runtime assembly targeting .NET Standard 2.1
- public engine, runtime, context, registry, and result surfaces
- host extension contracts
- provenance diagnostics
- documentation and integration examples
- Unity hosting guidance

## Important Boundary

QSL Runtime does not include Wilds, Stratyx, Cryo Nation content, the CLI certification harness, or internal audit reports.

## Dependency

QSL Runtime uses `System.Text.Json` 8.0.5 for provenance JSON export.

## Validation

Before public release, validate the packaged artifact through the established QSL certification suite and supported host smoke tests.
