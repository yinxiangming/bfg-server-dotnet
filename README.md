# BFG .NET API

**.NET implementation** of the [BFG Framework](https://github.com/yinxiangming/bfg-framework) API — REST API for e-commerce and SaaS (auth, workspaces, shop, storefront, marketing, support, etc.).

## Quick start

**Requirements:** .NET 8 SDK, MySQL 8.x.

1. Clone and open the solution:
   ```bash
   cd bfg-server-dotnet
   dotnet restore
   ```

2. Configure environment: copy `.env.example` to `.env` and set at least:
   - `DATABASE_URL` — MySQL connection string, e.g.  
     `Server=localhost;Database=bfg;User=root;Password=yourpassword`
   - `JWT__SECRET_KEY` — a secure secret for JWT signing

3. Create database and apply migrations (from repo root):
   ```bash
   dotnet ef database update --project src/Bfg.Api
   ```
   If the EF CLI is not installed: `dotnet tool install --global dotnet-ef`

4. Run the API:
   ```bash
   dotnet run --project src/Bfg.Api
   ```
   API base: `http://localhost:5000` (or the port in `src/Bfg.Api/Properties/launchSettings.json`). Swagger UI: `http://localhost:5000/api/docs`

### CLI tools (`bfg-cli`)

General-purpose commands (DB from same `DATABASE_URL` / `.env` as the API). Entry: `tools/Bfg.Cli`.

```bash
cd bfg-server-dotnet
dotnet run --project tools/Bfg.Cli -- --help

# Remove all tenant data for one workspace (not common_user / finance_currency):
dotnet run --project tools/Bfg.Cli -- workspace purge <workspaceId>
dotnet run --project tools/Bfg.Cli -- workspace purge <workspaceId> --dry-run

# All workspaces (still does not delete users / superadmin; requires --confirm to run):
dotnet run --project tools/Bfg.Cli -- workspace purge-all --dry-run
dotnet run --project tools/Bfg.Cli -- workspace purge-all --confirm
```

## Environment variables

Loaded from `.env` in repo root (via DotNetEnv). Shell or launch settings override `.env`.

| Variable | Description |
|----------|-------------|
| `DATABASE_URL` or `ConnectionStrings__DefaultConnection` | MySQL connection string |
| `FRONTEND_URL` | Frontend origin (CORS, links) |
| `SITE_NAME` | Site name (default: BFG) |
| `JWT__SECRET_KEY` | JWT signing key (required) |
| `JWT__ISSUER`, `JWT__AUDIENCE` | JWT issuer/audience |
| `JWT__ACCESS_TOKEN_LIFETIME_MINUTES`, `JWT__REFRESH_TOKEN_LIFETIME_DAYS` | Token lifetimes |

## API overview

| Prefix | Module | Notes |
|--------|--------|--------|
| `/api/v1/auth/` | Auth | register, token, token/refresh, token/verify, forgot-password, reset-password-confirm, verify-email |
| `/api/v1/` | Common | workspaces, customers, addresses, settings, users, me, options/, countries/ |
| `/api/v1/web/` | Web | sites, themes, languages, pages, inquiries, blocks/, newsletter/unsubscribe/ |
| `/api/v1/` | Shop | products, categories, stores, carts, orders |
| `/api/v1/store/` | Storefront | products, categories, cart, orders, payments/callback/{gateway}, promo/ |
| `/api/v1/inbox/` | Inbox | messages, templates, recipients, sms |
| `/api/v1/` | Delivery, Marketing, Support, Finance | warehouses, campaigns, tickets, invoices, etc. |

List endpoints use `?page=1&page_size=20`. Responses use `Bfg.Api.Infrastructure.Pagination` (count, next, previous, results). Validation errors return JSON with field names and error messages per field.

## License

MIT. See the [BFG Framework](https://github.com/yinxiangming/bfg-framework) repo for the main project and full license text.
