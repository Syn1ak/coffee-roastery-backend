# Coffee Roastery Shop — Backend Plan

A backend API for a specialty coffee roastery, built on ASP.NET Core.
This document is the build plan: what the system is, how it is structured,
and the order in which it gets built.

---

## 1. What we are building

**A specialty coffee roastery shop.** Physical goods plus a subscription tier.

The domain is deliberately narrow — not a generic marketplace — but its business rules
are rich enough to demand real engineering:

| Domain trait | Backend implication |
|---|---|
| Variants (grind, bag size, roast level) | Real product modeling, not `Product { Name, Price }` |
| Roast dates / batch freshness | Inventory concurrency, optimistic locking |
| Subscriptions ("ship every 2 weeks") | Recurring background jobs, recurring billing |
| Small catalog | Finite scope; the system can actually be finished |

### Capabilities the finished system has

- Browse, search, and filter a product catalog with variants and images
- Register and sign in, including Google and Microsoft SSO
- Build a cart as a guest, then keep it after signing in
- Apply coupons, reserve stock, and check out with real payment handling
- Track an order through its lifecycle, with live status updates
- Manage recurring subscription shipments
- Administer catalog, stock, and orders behind role-based access
- Report on sales, with an audit trail behind every change

---

## 2. Platform & approach

The system runs on **.NET 10** against **PostgreSQL**, with Postgres, Redis, RabbitMQ, and
MailHog all provisioned as containers. Local orchestration is Docker Compose, moving to
**.NET Aspire** for the dependency graph and its dashboard.

The API is a **modular monolith** — not microservices, and not a nine-project layered
cathedral. One service carved out later (Notifications, in Phase 5) covers distributed
messaging without paying the full distributed-systems tax.

### Authentication

Authentication is **JWT with refresh token rotation**:

- **Access token** — short-lived (~15 min), returned to the client and held in memory.
- **Refresh token** — long-lived, issued as an **httpOnly + Secure cookie**, rotated on
  every use, with replay detection: reusing an already-consumed refresh token revokes the
  entire token family.

**SSO is backend-driven.** The client hits `/auth/challenge/google`; the API performs the
OIDC handshake, links or creates the local account, and issues our own access token and
refresh cookie. The client never talks to the identity provider directly.

Two later evolutions are possible once the core is stable: refactoring to a BFF /
cookie-session model, and replacing hand-rolled token issuance with **OpenIddict** to run
a full OAuth2 authorization server.

---

## 3. Architecture

Four projects:

```
Shop.Api             → endpoints, middleware, auth wiring, DI composition root
Shop.Application     → use cases, organised in FEATURE FOLDERS
Shop.Domain          → entities, value objects, domain events, business rules
Shop.Infrastructure  → EF Core, Redis, Stripe, email, blob storage
```

### Key convention: vertical slices

Inside `Shop.Application`, organise by **feature**, not by technical layer:

```
Application/
  Orders/
    PlaceOrder/
      PlaceOrderCommand.cs
      PlaceOrderHandler.cs
      PlaceOrderValidator.cs
      PlaceOrderResponse.cs
    CancelOrder/
    GetOrderById/
  Catalog/
  Cart/
```

**Not** `Services/`, `Repositories/`, `Interfaces/`. Horizontal layering spreads a single
change across four folders; vertical slices keep it in one.

### Dependency rule

`Domain` references nothing. `Application` references `Domain`.
`Infrastructure` references `Application` + `Domain`. `Api` references everything.
Enforced by an architecture test in Phase 7.

---

## 4. Roadmap

### Phase 0 — Foundation

Replace the `WeatherForecast` scaffold with a real solution.

| Feature | Covers |
|---|---|
| Solution scaffold, health check endpoint | Project structure, DI lifetimes, options pattern, `IConfiguration` |
| Postgres + EF Core, first migrations | EF Core, migrations, data seeding |
| Global exception handling | Middleware, `IExceptionHandler`, `ProblemDetails` |
| Request validation | FluentValidation, model binding |
| Structured logging | Serilog, log enrichment, correlation IDs |
| OpenAPI document | `Microsoft.AspNetCore.OpenApi`, schema generation, client codegen |
| Docker Compose / Aspire host | Container orchestration for local development |

A mix of **Minimal APIs** and **Controllers** is used across the first endpoints, so the
choice between them is made on evidence rather than assumption.

---

### Phase 1 — Catalog (read-heavy)

| Feature | Covers |
|---|---|
| Products, categories, variants, images | EF relationships, owned types, value objects |
| Search / filter / sort / paginate | `IQueryable` composition, projections, keyset pagination |
| Product detail endpoint | N+1 detection, `AsSplitQuery`, compiled queries, indexes |
| Catalog caching | `IMemoryCache` → `HybridCache` (.NET 9+), cache invalidation strategy |
| Image upload + thumbnail generation | File streaming, blob storage (MinIO/Azurite), `IHostedService` |
| ETags / conditional GET | HTTP caching semantics, response compression |
| Full-text product search | Postgres `tsvector` via EF, or raw SQL |

This is the phase where EF Core has to *perform*, not merely work.

---

### Phase 2 — Identity & authentication

| Feature | Covers |
|---|---|
| Register / login | ASP.NET Core Identity, password hashing |
| JWT access + refresh tokens | Token issuance, rotation, revocation, replay detection |
| Google + Microsoft SSO | OIDC/OAuth2 flows, `AddOpenIdConnect`, external claim mapping, account linking |
| Email confirmation, password reset | Identity token providers, transactional email |
| Roles: Customer / Support / Admin | Policy-based authorization, claims transformation |
| "Only view your own order" | **Resource-based authorization** (`IAuthorizationHandler`) |
| Two-factor auth | TOTP authenticators |
| Login throttling | Built-in rate limiting middleware |
| CORS for the web client origin | CORS policy configuration |

---

### Phase 3 — Cart & checkout

| Feature | Covers |
|---|---|
| Guest cart, merged into user cart on login | Distributed cache (Redis), session strategies |
| Price snapshot at add-to-cart time | Domain modeling, Money as a value object |
| Coupons / discount rules | Strategy pattern, rules with real edge cases |
| Stock reservation | **Optimistic concurrency** (`rowversion`), transactions, isolation levels |
| Duplicate-submit protection | **Idempotency keys** |

---

### Phase 4 — Orders & payments

The core of the system. Webhooks, outbox, and idempotency are what make order handling
correct under real-world failure conditions rather than merely working on the happy path.

| Feature | Covers |
|---|---|
| Order lifecycle (Pending → Paid → Shipped → Delivered / Refunded) | State machine, domain events |
| Stripe integration (test mode) | `IHttpClientFactory`, typed clients, **Polly** resilience pipelines |
| Stripe webhooks | Signature verification, at-least-once delivery, idempotent consumers |
| Reliable "order placed → send email" | **Transactional outbox pattern** |
| Scheduled work (subscription shipments, abandoned carts) | `BackgroundService`, Hangfire or Quartz.NET |
| Invoice PDF generation | QuestPDF, streaming responses |

---

### Phase 5 — Messaging & real-time

| Feature | Covers |
|---|---|
| Extract a Notifications worker service | RabbitMQ + MassTransit (or Azure Service Bus) |
| Publish / consume domain events | Event-driven design, consumer retries, dead-letter queues |
| Live order tracking for customers | **SignalR**, hubs, user-targeted messages |
| Live admin dashboard ("new order!") | SignalR groups, Redis backplane |

---

### Phase 6 — Admin & operations

| Feature | Covers |
|---|---|
| Admin CRUD + bulk CSV import | `IAsyncEnumerable`, streaming, `System.Threading.Channels` |
| Sales reports / analytics | **Dapper + raw SQL**, where EF is the wrong tool |
| Audit log, soft delete | EF interceptors, global query filters, `SaveChangesAsync` overrides |
| Feature flags | `Microsoft.FeatureManagement` |
| Health checks, metrics, traces | **OpenTelemetry**, `IHealthCheck`, Aspire dashboard |

---

### Phase 7 — Quality & delivery

| Feature | Covers |
|---|---|
| Unit tests | xUnit, NSubstitute, domain rules tested in isolation |
| Integration tests | `WebApplicationFactory` + **Testcontainers** (real Postgres per run) |
| Architecture tests | NetArchTest — enforces the dependency rule from §3 |
| CI pipeline | GitHub Actions, Docker multi-stage builds |
| Deployment | Azure Container Apps or Fly.io |

