# AegisFlow
### Transactional Order & Inventory API

A transactional order-processing service built with **C# / ASP.NET Core 10**, **Entity Framework Core 10** and **SQL Server**. It reserves inventory atomically, records a stock ledger and protects order creation against retries.

## Capabilities

- REST API: product registration, catalog queries, order creation, order retrieval and XML receipt export.
- Serializable database transactions, optimistic concurrency with version tokens, unique idempotency keys.
- SHA-256 payload fingerprinting: same key + same payload replays safely; reused keys with changed payload are rejected (`409`).
- Quantity checks, stock ledger and validation-friendly `400` / `404` / `409` outcomes.
- xUnit + SQLite-backed integration-style tests; GitHub Actions CI.
- SQL Server initial schema migration via EF Core.

## Quick start

Requirements: .NET 10 SDK and SQL Server 2022+ (local / container). On Windows, install SQL Server Developer or use Docker.

```bash
docker compose up -d
dotnet restore
dotnet run --project src/AegisFlow.Api
```

The API reads `ConnectionStrings__DefaultConnection` or `appsettings.json`. Never commit production credentials. Docker Compose reads `SA_PASSWORD` from your environment; set it first (see `.env.template`). Open `http://localhost:5081/health`.

## Create inventory and an order

```bash
curl -X POST http://localhost:5081/api/products -H "Content-Type: application/json" -d '{"sku":"SKU-2026-001","name":"Precision Keyboard","available":8}'
```

Copy the generated `id` from the response.

```bash
curl -X POST http://localhost:5081/api/orders \
  -H "Content-Type: application/json" -H "Idempotency-Key: sale-2026-0001" \
  -d '{"items":[{"productId":"PUT-PRODUCT-ID-HERE","quantity":2}]}'
```

Repeat the request with the same key and payload: no inventory is deducted twice. `GET /api/orders/{id}/receipt.xml` exports a structured XML receipt.

## Test

```bash
dotnet test
```

CI runs restore, build and tests on push/PR. Database server integration under concurrent traffic is a separate environment-dependent verification: see [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md).

## Endpoints

| Method | Path | Description |
|---|---|---|
| GET | `/health` | Health response |
| POST | `/api/products` | Create a SKU |
| GET | `/api/products` | Catalog |
| GET | `/api/products/{id}` | Inventory detail |
| POST | `/api/orders` | Atomic placement; requires `Idempotency-Key` |
| GET | `/api/orders/{id}` | Order + lines |
| GET | `/api/orders/{id}/receipt.xml` | XML receipt |

## Design

[Architecture and invariants](docs/ARCHITECTURE.md) · [OpenAPI contract](docs/openapi.yaml) · [API requests](requests.http)
