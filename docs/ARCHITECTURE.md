# Architecture

`HTTP endpoints → OrderService → EF Core → SQL Server`

OrderService enforces:

1. A client-generated idempotency key identifies one logical order; a canonical SHA-256 digest protects against accidental key reuse.
2. All stock changes, order lines, and ledger entries commit in **one SERIALIZABLE transaction**.
3. The product `Version` value acts as an EF Core concurrency token and is incremented on every update, so a stale writer is rejected.
4. `StockMovement` records every decrement and links it to the original order.
5. `Order` has a unique index on `IdempotencyKey`, providing a final database-level duplicate barrier.

The API maps conflicting updates to `409` and insufficient availability to `409` without committing any partial inventory changes. A production deployment should add authentication, distributed tracing, SQL Server retry policies for transient deadlocks, and rate limiting.

Test coverage includes the idempotency replay path, rejection on a changed fingerprint, insufficient stock rollback and ledger integrity. This is not a claim of a production rollout or load-test benchmark.
