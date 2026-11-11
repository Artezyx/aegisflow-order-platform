# JWT authorization and performance profile

AegisFlow now validates signed HS256 JWTs with issuer, audience, expiration
and signature verification. Read-only tokens cannot create inventory or
orders. Supported single-valued `scope` claims: `inventory.read`,
`inventory.write`, `orders.read`, `orders.write`.
For multiple scopes include one separate scope claim per privilege.

Set a random signing key using environment variable
`Auth__SigningKey` with at least 32 UTF-8 bytes. Do not commit it.
Optional `Auth__Issuer` and `Auth__Audience` default to
`aegisflow-local` and `aegisflow-api`.

No token issuing endpoint is exposed; use an OIDC provider or a controlled
local token fixture. In production, use asymmetric signing, JWKS key rotation,
TLS, centrally issued short-lived tokens and rate limiting. The local
SQL Server demo still needs an appropriate DB identity, migration policy and
network hardening before deployment.

Run `dotnet test AegisFlow.slnx` for authorization policy unit tests
and transaction/concurrency tests.

Run this opt-in k6 profile only against disposable environments:

```sh
JWT="<short-lived-test-token>" BASE_URL=http://localhost:5081 k6 run load/catalog-smoke.js
```

12 virtual users for 40 seconds; p95 under 800 ms and errors below
1% are unmeasured target thresholds. Document observed hardware,
transaction throughput, p50/p95, error rate and SQL connection utilization.
