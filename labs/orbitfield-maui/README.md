# OrbitField — .NET MAUI Offline Field Work App

A cross-platform .NET MAUI app for technicians and sales field teams: create local work orders, edit workflow status and synchronize pending changes with a REST API when online.

## Stack
- .NET MAUI 10: Android and iOS target frameworks, C#, XAML, MVVM with CommunityToolkit.Mvvm.
- SQLite local storage, offline-first work queue, async REST synchronization, retryable network failures.
- Companion ASP.NET Core 10 sync API with idempotent upserts and server-side validation.
- Portable business logic xUnit tests plus GitHub Actions validating .NET domain and API.

## Run
1. Install .NET 10 SDK, Android SDK, MAUI workload, and Android emulator. iOS requires a Mac with Xcode and matching .NET workload.
2. From the app folder run dotnet workload install maui, then dotnet build -f net10.0-android.
3. Run dotnet run --project server/OrbitField.SyncApi.csproj --urls http://localhost:5099.
4. Set SyncSettings.ApiBaseUrl for your emulator/host (Android emulator uses http://10.0.2.2:5099 for local development; iOS simulator can use localhost).
5. Android will require development-only cleartext HTTP permission if you choose that local URL. For deployed environments use HTTPS.

## Security / limits
The companion API is an unprotected **local development-only** demonstration and uses an in-memory record store. Data is lost on server restart. Do not expose it publicly or sync real business/customer information. Replace with authenticated JWT + tenant policies, persistent DB, TLS and server-controlled conflict resolution. The MAUI client stores local SQLite records without encryption; use platform-protected storage for sensitive fields. Offline operations and portable domain tests can be validated separately, but Android/iOS build and device testing require their respective workloads and have NOT been verified in this environment. The GitHub CI below tests portable logic/API only; it does not imply a successful mobile build.
