# Security

Report suspected QSL Runtime security issues privately through the official TethraSolutions, LLC security/support channel supplied with the product.

Do not publish credentials, private data, or exploit details in public reports.

Include:

- affected QSL version
- affected host/runtime
- reproduction steps
- impact description
- relevant diagnostics

QSL evaluates host-provided source against host-provided vocabulary and registered behavior. Hosts are responsible for deciding which functions, capabilities, descriptors, consumer objects, bindings, state values, and packages are exposed to untrusted or semi-trusted source.

Treat runtime registration, consumer binding, and state/package ingress as application security boundaries. Package values are accepted as resolved upstream inputs; QSL does not independently validate their domain meaning.
