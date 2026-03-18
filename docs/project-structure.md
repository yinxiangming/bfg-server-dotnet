# BFG .NET Project Structure

The BFG .NET API uses a **solution with multiple projects**: domain and data live in `Bfg.Core`, the Web API in `Bfg.Api`.

## Directory Overview

```
bfg-server-dotnet/
├── src/
│   ├── Bfg.Core/          # Domain and data layer (class library)
│   └── Bfg.Api/           # Web API entry and endpoints
├── docs/
├── .env.example
├── .env                   # Local config (not committed)
└── README.md
```

---

## Bfg.Core (class library, net9.0)

Domain entities and EF Core context; no HTTP dependencies. Table names align with the Django backend (e.g. `common_workspace`).

### Directories and Namespaces

| Directory | Namespace | Description |
|-----------|-----------|-------------|
| *(root)* | `Bfg.Core` | `BfgDbContext` |
| **Common/** | `Bfg.Core.Common` | Workspace, User, Customer, Address, Settings, Audit, Media, Staff, etc. |
| **Web/** | `Bfg.Core.Web` | Site, Theme, Language, Page, Inquiry |
| **Shop/** | `Bfg.Core.Shop` | Product, ProductCategory, Variant, Store, Cart, Order, OrderItem, etc. |
| **Delivery/** | `Bfg.Core.Delivery` | Warehouse, Carrier, FreightService, DeliveryZone, Shipment |
| **Finance/** | `Bfg.Core.Finance` | Currency, PaymentGateway, Payment, PaymentMethod, Invoice |
| **Support/** | `Bfg.Core.Support` | SupportTicket, TicketMessage |
| **Inbox/** | `Bfg.Core.Inbox` | InboxMessage, MessageTemplate, Notification |
| **Promo/** | `Bfg.Core.Promo` | Voucher, Campaign, DiscountRule, GiftCard |

### Dependencies

- `Pomelo.EntityFrameworkCore.MySql` — MySQL access

---

## Bfg.Api (Web project, net9.0)

ASP.NET Core Minimal API referencing `Bfg.Core`. Configuration is loaded from `.env` (DotNetEnv); entry point is `Program.cs`.

### Directories and Responsibilities

| Directory | Description |
|-----------|-------------|
| **Program.cs** | Registers DbContext, JWT, Swagger, middleware, and maps all endpoints |
| **Configuration/** | `AppOptions`, `JwtOptions`, `ConfigExtensions` (connection string, JWT, FrontendUrl, etc. from env) |
| **Endpoints/** | Module-based Minimal API: Auth, Common, Web, Shop, Delivery, Finance, Support, Marketing, Storefront, Me, Inbox, OtherModule |
| **Services/** | Application services: `JwtService`, `CustomerNumberService`, `OrderNumberService`, etc. |
| **Infrastructure/** | Shared structures such as `Pagination` |
| **Middleware/** | e.g. `WorkspaceMiddleware` (workspace/tenant resolution) |
| **Migrations/** | EF Core migrations (`BfgDbContext` lives in Core; MigrationsAssembly set to Bfg.Api) |
| **BfgDbContextFactory.cs** | Used by EF design-time/CLI to create DbContext |

### Endpoint Mapping (Program.cs)

- `MapAuthEndpoints()` → `/api/v1/auth/`
- `MapCommonEndpoints()` → workspaces, customers, settings, users, me, options, countries, etc.
- `MapWebEndpoints()` → `/api/v1/web/` (sites, themes, languages, pages, inquiries)
- `MapShopEndpoints()` → products, categories, stores, carts, orders
- `MapDeliveryEndpoints()`, `MapFinanceEndpoints()`, `MapSupportEndpoints()`, `MapMarketingEndpoints()`
- `MapStorefrontEndpoints()` → `/api/v1/store/`
- `MapMeEndpoints()`, `MapOtherModuleEndpoints()`, `MapInboxEndpoints()`

List endpoints use `?page=1&page_size=20`; response shape is in `Bfg.Api.Infrastructure.Pagination`.

### Dependencies (main)

- `Bfg.Core` (project reference)
- `BCrypt.Net-Next`, `DotNetEnv`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `Pomelo.EntityFrameworkCore.MySql`, `Swashbuckle.AspNetCore`, `Microsoft.EntityFrameworkCore.Design` (design-time only)

---

## Configuration and Running

- **Environment variables:** See the Environment variables table in the repo root `README.md`; values are loaded from `.env` first.
- **Database:** MySQL; connection string from `DATABASE_URL` or `ConnectionStrings__DefaultConnection`.
- **Migrations:** From repo root run `dotnet ef database update --project src/Bfg.Api`.
- **Run:** `dotnet run --project src/Bfg.Api`; Swagger at `/api/docs`, schema at `/api/schema/v1/swagger.json`.
