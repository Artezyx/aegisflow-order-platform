# Evidencia técnica React + ASP.NET Core + SQL Server

Este módulo extiende AegisFlow con una **consola React 19 / TypeScript** que consume la API transaccional **ASP.NET Core 10** existente, conectada a **SQL Server mediante EF Core**. Es un proyecto personal de portafolio, no experiencia laboral.

## Ejecutar
1. Instala .NET 10, Node 22 y SQL Server 2022+ (o Docker).
2. Desde la raíz, configura las variables según `.env.template` y ejecuta `docker compose up -d`.
3. Inicia la API con `dotnet run --project src/AegisFlow.Api`. Debe escuchar en `http://localhost:5081`.
4. Ejecuta `cd web && npm install && npm run dev`.
5. Abre `http://localhost:5173`. Vite proxy redirige las llamadas `/api` al backend.
6. En `web`: `npm run typecheck && npm test && npm run build`. En la raíz: `dotnet test`.

## Qué demuestra el código
- React: hooks, composición de componentes, renderizado de productos, formulario controlado y estado con reducer (`web/src/App.tsx`, `web/src/state.ts`).
- REST: cliente tipado y manejo de respuestas HTTP (`web/src/api.ts`) contra endpoints `/api/products` y `/api/orders` en `src/AegisFlow.Api/Program.cs`.
- .NET: DI con `AddScoped`, servicios y validación de negocio (`src/AegisFlow.Api/`).
- SQL Server: EF Core, índice único, integridad y control de concurrencia (`src/AegisFlow.Api/AegisDbContext.cs`).
- Pruebas: Vitest/React Testing Library y xUnit (`web/src/*.test.ts*`, `tests/AegisFlow.Tests/`).
- CI: GitHub Actions (`.github/workflows/react.yml` y `.github/workflows/dotnet.yml`).
- Reintentos: clave idempotente reutilizable si el mismo pedido se reenvía tras un error de red.

## Limitaciones
No implementa autenticación, roles, autorización, SOAP, VB.NET, SSRS ni Azure DevOps. El entorno es local; no se ha verificado el despliegue productivo, protección multiusuario, pruebas E2E con navegador, pruebas de carga ni benchmarks SQL Server. Las pruebas CI sólo deben declararse aprobadas después de comprobar ejecuciones reales. **No desplegar en Internet con datos reales sin endurecimiento de seguridad.**

## Texto para CV cuando CI pase
AegisFlow — proyecto de portafolio: Desarrollo de consola React/TypeScript integrada a una API REST ASP.NET Core y SQL Server; gestión de inventario y pedidos, idempotencia, pruebas con Vitest/xUnit y CI GitHub Actions. Repositorio: https://github.com/Artezyx/aegisflow-order-platform
