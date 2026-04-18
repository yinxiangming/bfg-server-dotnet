# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Restore & build
dotnet restore
dotnet build Bfg.sln

# Run API (http://localhost:3002, Swagger at /api/docs)
dotnet run --project src/Bfg.Api

# Run all tests
dotnet test

# Run a single test class
dotnet test --filter "FullyQualifiedName~CheckoutTotalsCalculatorTests"

# Database migrations (install EF CLI first if needed: dotnet tool install --global dotnet-ef)
dotnet ef database update --project src/Bfg.Api
dotnet ef migrations add <MigrationName> --project src/Bfg.Api

# CLI tool
dotnet run --project tools/Bfg.Cli -- --help
dotnet run --project tools/Bfg.Cli -- workspace purge <workspaceId> --dry-run
```

## Architecture

### Solution layout

| Project | Role |
|---------|------|
| `src/Bfg.Core` | Domain entities + EF Core `BfgDbContext` (no HTTP dependency) |
| `src/Bfg.Api` | ASP.NET Core Minimal API — endpoints, services, middleware, migrations |
| `tools/Bfg.Cli` | Admin CLI for workspace operations (shares the same DB) |
| `test/Bfg.Api.Tests` | xUnit unit tests for API services |

### Core layer (`Bfg.Core`)

All EF Core entities live here, namespaced by module: `Common`, `Web`, `Shop`, `Delivery`, `Finance`, `Support`, `Inbox`, `Promo`. `BfgDbContext` is the single DbContext for the whole application. Table names follow Django conventions (snake_case, e.g. `common_workspace`, `shop_product`) enforced via `EFCore.NamingConventions`.

### API layer (`Bfg.Api`)

- **Program.cs** — bootstraps DI (DbContext, JWT, Swagger, services), registers middleware, and calls `Map*Endpoints()` for each module.
- **Endpoints/** — one file per module (e.g. `ShopEndpoints.cs`). Each file is a static class with extension methods called from `Program.cs`.
- **Services/** — stateless application services: `JwtService`, `CartService`, `OrderCheckoutService`, `CustomerNumberService`, `OrderNumberService`, `AppPasswordHasher`.
- **Middleware/WorkspaceMiddleware.cs** — resolves the current tenant (workspace) from the request before endpoints run.
- **Migrations/** — EF Core migrations targeting `BfgDbContext`. The MigrationsAssembly is set to `Bfg.Api`, not `Bfg.Core`.
- **Configuration/ConfigExtensions.cs** — reads all environment variables and binds them to typed options (`AppOptions`, `JwtOptions`).

### Multi-tenancy

Every request is scoped to a workspace (tenant). `WorkspaceMiddleware` populates workspace context early in the pipeline; endpoint handlers and services rely on it for data isolation.

### Environment / configuration

Copy `.env.example` to `.env` in the repo root. Required variables:

| Variable | Purpose |
|----------|---------|
| `DATABASE_URL` | MySQL connection string |
| `JWT__SECRET_KEY` | JWT signing key |
| `FRONTEND_URL` | CORS origin and link generation |

`DotNetEnv` loads `.env` at startup; shell environment and `launchSettings.json` take precedence.

### API conventions

- **Routes:** `/api/v1/{module}/{resource}` (storefront: `/api/v1/store/`)
- **Pagination:** query params `?page=1&page_size=20`; response shape `{count, next, previous, results}` from `Bfg.Api.Infrastructure.Pagination`
- **JSON:** snake_case property names
- **Validation errors:** `{ "field_name": ["error message"] }` — mirrors Django REST Framework shape
- **Auth:** JWT Bearer (HS256); token refresh supported via `/api/v1/auth/token/refresh`
- **Passwords:** BCrypt via `AppPasswordHasher`

### Adding a new endpoint

1. Add domain entity/model to the appropriate module folder in `Bfg.Core` and register it in `BfgDbContext`.
2. Create a migration: `dotnet ef migrations add <Name> --project src/Bfg.Api`.
3. Add or extend the corresponding `*Endpoints.cs` file in `src/Bfg.Api/Endpoints/`.
4. If business logic is non-trivial, add a service in `src/Bfg.Api/Services/` and register it in `Program.cs`.
