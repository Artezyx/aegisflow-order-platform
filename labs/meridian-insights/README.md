# Meridian Insights — .NET + Power BI

Independent analytics project for ASP.NET Core 10, SQL Server, Power Query M, DAX and Power BI embedded analytics (user-owns-data).

## Architecture
ASP.NET Core -> SQL Server tables FactSales and DimAccount -> Power BI semantic model.
Vite/TypeScript -> local KPI API; optional MSAL delegated sign-in -> Power BI report embed SDK.

## Run
1. Run sql/01_schema_seed.sql against a disposable SQL Server instance.
2. Configure ConnectionStrings__Meridian in the API process environment.
3. Run: dotnet run --project api/Meridian.Api.csproj --urls http://localhost:5098
4. From web/ run: npm install then npm run dev. Local metrics are shown at http://127.0.0.1:5173.
5. For a real embedded Power BI report, import powerbi/FactSales.pq, add measures from powerbi/Measures.dax, create and publish the report according to powerbi/REPORT_DESIGN.md. Register a Microsoft Entra SPA with the delegated Power BI Report.Read.All permission, set its redirect URI to http://127.0.0.1:5173, and configure web/.env.local:

~~~env
VITE_ENTRA_CLIENT_ID=your-client-id
VITE_ENTRA_TENANT_ID=your-tenant-id
VITE_POWERBI_REPORT_ID=your-report-id
VITE_POWERBI_EMBED_URL=https://app.powerbi.com/reportEmbed?reportId=...
VITE_API_BASE_URL=http://localhost:5098
~~~

## Test
- dotnet test tests/Meridian.Tests.csproj
- cd web && npm install && npm run build

## Security / verification
Only disposable sample data. The SQL metrics API is NOT authenticated: bind only to localhost, and add Entra JWT validation, roles and SQL least privilege before publishing. SPA holds the delegated Power BI token in memory; never commit bearer tokens or secrets. Power BI embed also requires report permissions and relevant user license/capacity. This repository does not contain a .pbix file or an already published report. CI checks compilation and domain rules, not live SQL/Entra/Power BI integration.
