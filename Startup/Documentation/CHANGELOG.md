# Changelog

All notable commercial releases of QSL Runtime are documented here.

## 1.0.0

### Added

- initial supported QSL Runtime commercial baseline
- .NET Standard 2.1 runtime package identity
- `QslEngine` parsing, evaluation, consumer-binding, and runtime-state gateway
- registry, context, evaluator, operator, descriptor, capability, function, and runtime-object surfaces
- exact consumer-object binding through `QslConsumerBinding`
- identity-keyed runtime-attached state through `QslAttachedStateStore`
- native and attached member access and assignment
- state receipt, update, removal, unbinding, and context clearing lifecycle
- `IQslPackage` and `QslPackageBase` package foundation
- `QslStatePackage` batch state receipt
- `QslDecayPackage` for resolved maintenance values and optional zero-value retirement
- package identity and provenance lifecycle
- structured parse and processing results
- provenance-backed runtime diagnostics and JSON export
- 1.0 documentation and Unity integration guidance

### Certification

- release must retain the certified QSL semantic baseline and pass the established certification suite before publication
- consumer/runtime-state behavior should be covered by dedicated conformance tests without weakening the core semantic baseline
