# Shop — Coffee Roastery Backend

An ASP.NET Core backend for a specialty coffee roastery: a product catalog with
variants, authentication, cart and checkout, order and payment handling, and
recurring subscription shipments.

> **Status: early.** Phase 0 (foundation) is in progress. There is no database,
> no catalog and no authentication yet. The `Hopper*` types in `Shop.Api` are
> deliberate scaffolding used to exercise DI and configuration — they are not
> part of the product and will be removed.

This is a learning project. The full build plan lives in
[`.claude/ROADMAP.md`](.claude/ROADMAP.md).

## Requirements

- [.NET SDK 10.0](https://dotnet.microsoft.com/download) or later

## Getting started

```bash
dotnet build
```

```bash
dotnet run --project src/Shop.Api
```

The default (`https`) launch profile serves `https://localhost:7241` and
`http://localhost:5256`. To pick one explicitly:

```bash
dotnet run --project src/Shop.Api --launch-profile http
```

To run outside the launch profile — which is the only way to get a non-Development
environment, since the profile hardcodes `ASPNETCORE_ENVIRONMENT` — set the
variables yourself:

```bash
ASPNETCORE_ENVIRONMENT=Production ASPNETCORE_URLS=http://localhost:5256 dotnet run --project src/Shop.Api --no-launch-profile
```

The startup banner prints `Hosting environment:` — trust that line, not the
variable you thought you set.

## Endpoints

All current endpoints are scaffolding.

| Method | Route | Returns |
|---|---|---|
| `GET` | `/hopper` | Current hopper reading |
| `GET` | `/hopper/summary` | A sentence using the configured shop name |
| `GET` | `/hopper/ids` | Instance ids for singleton, scoped and transient registrations |
| `GET` | `/hopper/tracker` | A singleton's stored id next to a live scoped id |

```bash
curl -k https://localhost:7241/hopper/ids
```

## Configuration

`src/Shop.Api/appsettings.json`:

```json
{
  "CoffeeRoastery": {
    "ShopDisplayName": "Roastry",
    "SupportEmailAddress": "roastrysupport@gmail.com"
  }
}
```

Bound to `RoasterySettings` and validated with `ValidateOnStart()`, so a missing
or malformed value **stops the application from starting** rather than surfacing
as a 500 on some later request. A startup failure here is not a bug; it is the
configuration telling you it is wrong.

Scope validation (`ValidateScopes` / `ValidateOnBuild`) is enabled in **every**
environment, not just Development. Captive dependencies fail at boot in
production too.

## Project layout

```
Shop.slnx                 XML solution format (not .sln)
Directory.Build.props     net10.0, nullable, implicit usings, warnings-as-errors
src/
  Shop.Domain             Entities, value objects, domain rules. No dependencies.
  Shop.Application        Use cases as vertical slices. Depends on Domain.
  Shop.Infrastructure     EF Core and external services. Depends on Application.
  Shop.Api                Endpoints, DI, middleware. Depends on all of the above.
scratch/                  Throwaway C# playground. Not in the solution.
```

Architecture is hybrid Clean + vertical slice. Inside `Shop.Application`, code is
organised by **feature** (`Orders/PlaceOrder/…`), not by technical layer — no
`Services/`, `Repositories/` or `Interfaces/` folders.

The dependency rule is one-directional: `Domain` → `Application` →
`Infrastructure` → `Api`. It will be enforced by an architecture test in Phase 7.

## Notes for contributors

`TreatWarningsAsErrors` is on for every project — a warning fails the build.

A green build is not evidence that the app works. Container registrations,
configuration binding and minimal API parameter binding are all resolved at
runtime, so verify with an actual request before committing.
