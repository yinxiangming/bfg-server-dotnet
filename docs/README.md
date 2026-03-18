# BFG .NET API docs

## OpenAPI contract

API contract is aligned with the Django DRF server (resale `src/server`). To export the current OpenAPI spec from Django:

1. Run the Django server, then: `GET http://<server>/api/schema/` (drf-spectacular).
2. Save the JSON to `docs/openapi/schema.json` in this repo or in resale `docs/openapi/`.

This repo uses the same paths and request/response shapes for seamless replacement.

## DB schema

Table structure matches Django models and migrations. EF Core entities and Fluent API use the same table/column names as Django (e.g. `common_workspace`, `common_user`, `web_site`, `shop_product`). See Django app labels and model names in resale `src/server/bfg2/bfg/*/models*.py`.

## API routes (aligned with Django)

| Prefix | Module | Notes |
|--------|--------|--------|
| `/api/v1/auth/` | Auth | register, token, token/refresh, token/verify, forgot-password, reset-password-confirm, verify-email |
| `/api/v1/` | Common | workspaces, customers, addresses, settings, email-configs, users, customer-segments, customer-tags, staff-roles, me, options/, countries/ |
| `/api/v1/web/` | Web | sites, themes, languages, pages, inquiries, blocks/types/, blocks/validate/, newsletter/unsubscribe/ |
| `/api/v1/` | Shop | products, categories, stores, carts, orders |
| `/api/v1/store/` | Storefront | products, categories, cart, orders, payments/callback/{gateway}, promo/ |
| `/api/v1/inbox/` | Inbox | messages, templates, recipients, sms |
| `/api/v1/` | Delivery, Marketing, Support, Finance | warehouses, campaigns, tickets, invoices, etc. |

## Pagination and errors

- List endpoints support `?page=1&page_size=20` (DRF-style). Use `Bfg.Api.Infrastructure.Pagination` for consistent wrapping (count, next, previous, results).
- 400 validation errors: return JSON object with field names as keys and arrays of error strings (e.g. `{"email": ["This field is required."]}`) to match DRF serializer errors.

## .env (local development)

Like Django’s `load_dotenv()`, the API loads a `.env` file at startup via **DotNetEnv**: it looks for `.env` in the current directory and parents (e.g. repo root). Variables from the environment (shell, container, launch settings) override values from `.env`. Copy `.env.example` to `.env` and set your local values; `.env` is in `.gitignore`.

## Environment variables

- `DATABASE_URL` – MySQL connection string (e.g. `Server=localhost;Database=bfg-dotnet;User=root;Password=;`) or `ConnectionStrings:DefaultConnection`
- `FRONTEND_URL` – Frontend base URL (no hardcoded URLs in code)
- `SITE_NAME` – Site name (default: BFG)
- `Jwt:SecretKey` / `JWT__SECRET_KEY` – JWT signing key
- `Jwt:Issuer`, `Jwt:Audience`, `Jwt:AccessTokenLifetimeMinutes`, `Jwt:RefreshTokenLifetimeDays`
