# Curriculum — concept ladder

Organised by **concept dependency**, not by feature. `.claude/ROADMAP.md` says what gets
built; this says what order it is learnable in — and every step must leave a piece of
the roadmap built behind it.

Phase 0 is written out below. Later phases get written when we reach them — writing
exercises for code neither of us has seen would be invention, not planning.

---

## Spike → real

Decided 2026-10-04. Learning must produce the shop, not just understanding.

| Kind | What it is | Where code lives | Fate |
|---|---|---|---|
| **Spike** | Throwaway code built to feel a pain (beat 2–3 of the loop) | Anywhere in `Shop.Api` | **Deleted** at graduation |
| **Real** | A roadmap feature, built properly | Its proper project and feature folder (`Domain` / `Application/<Feature>/<UseCase>` / `Infrastructure` / `Api`) | Stays |
| **Graduation** | Last row of every step. Delete the spikes, wire what was learned into the real code | — | Leaves no toy code behind |

Rules:

- **Phase 0** is plumbing with no business feature yet, so spikes are normal there.
- **From Phase 1 the exercise *is* the feature.** The naive version is the first draft
  of the real code, and the wall is hit inside it (e.g. write the catalog query naively,
  watch it N+1, then fix it in place). A separate spike only when the pain is unsafe or
  too tangled to feel in real code — and say *why* it is a spike.
- Every exercise states its kind on the **Goal** line, so it is never ambiguous whether
  the code is meant to survive.
- A step is not done until its graduation commit is in.
- Every step and exercise has a **Business** line listing the `docs/product/domain.md`
  questions it depends on. The learn skill's business gate makes sure those are decided
  before the work starts.

---

## Progress

| Exercise | Concept | Kind | Status |
|---|---|---|---|
| 2a part 1 | Life without a container | spike | **done** — 2026-08-29 |
| 2a part 2 | Registering and resolving | spike | **done** — 2026-09-20 |
| 2b | Service lifetimes | spike | **done** — 2026-09-20 |
| 2c | Configuration → options pattern | real | **done** — 2026-09-20 · `RoasterySettings` stays |
| 2d | Health checks | real | **done** — 2026-09-24 · `/health` stays, hopper check does not |
| 2e | Minimal API vs controller | spike | **done** — 2026-10-03 · chose minimal APIs |
| 3a | A database with no ORM | spike | **done** — 2026-10-07 · deleted at 3-grad |
| 3b | Rows ↔ objects: the DbContext | real | **current** — business gate passed 2026-10-08 |
| 3c | Schema changes as code: migrations | real | not started |
| 3d | Starting data: seeding | real | not started |
| 3-grad | Graduation: Steps 2 + 3 | graduation | not started |

Commit after each. Small revertable commits are part of the method. Commit titles use
the `2x: ...` / `3x: ...` prefix so history maps to exercises; graduation commits use
`3-grad: ...`.

Step 3 rows after 3a are a planned order only — their full text gets written when we
reach them.

### Why this order

`.claude/ROADMAP.md` Phase 0 lists one row — "Solution scaffold, health check endpoint ·
Covers: project structure, DI lifetimes, options pattern, `IConfiguration`" — and an
early attempt at it taught options and health checks **before** DI. Both of those are
consumers of the container, so the user was asked to use a container before anyone said
what one was. DI comes first here for that reason.

---

## Exercise 2a — Why does anything need a container?

**Goal:** feel the problem dependency injection solves. Part 1 uses no container at all.

### Part 1 — life without one *(done)*

1. Class `HopperMonitor` with a method returning a hardcoded `12.5m`.
2. `GET /hopper` that does `new HopperMonitor()` inside the handler and returns the
   value. Curl it, see a number. That is the baseline.
3. Make it log a warning when the hopper is low. It needs a logger. Try to give it one.
4. Add `GET /hopper/summary` that also uses `HopperMonitor`, returning a sentence.
5. Make it also know the shop's display name, which must live in `appsettings.json`,
   not in C#. Get it there by any means.

One sentence after each of 3, 4, 5: **what got worse?**

**What the user found:** the logger has to be threaded through every caller; config
arrives as untyped strings; everything ends up in one file. They also wrote an
`AppSettings` class and could not connect it to anything — right instinct, no mechanism.

**What they had not yet seen**, surfaced as experiments:

- A fresh instance is constructed **per request, per endpoint**. Prove it with a `Guid`
  field and two curls. Free today; not free when the object holds a connection or a
  warmed cache. → 2b.
- `app.Logger`'s category is the **application name**, not the class. Prove it by adding
  a per-class filter under `LogLevel` in `appsettings.json` and watching it do nothing.
  Every class logging through it is indistinguishable in output.
- A **config typo fails silently** — `Configuration["ShopDisplayNam"]` plus a `??`
  fallback serves wrong data forever while looking perfectly healthy. → 2c.
- `new` **welds** the endpoint to one concrete class. No fake for tests, no per-
  environment implementation. This is the wall that hurts most once tests exist.

### Part 2 — registering and resolving *(done)*

Rewrite so that:

1. `HopperMonitor` lives in its own file and namespace (types declared after top-level
   statements land in the global namespace — that is why one file felt wrong).
2. Neither endpoint uses `new`. Both **ask** for a `HopperMonitor` and receive one.
3. The logger arrives on its own, correctly categorised. No `app.Logger`.
4. Config still works. It may stay ugly; 2c fixes it.

Expect **two** registration failures for two different reasons. Both exception messages
are unusually clear. 15 minutes, then ask.

### → Read now
- [Minimal APIs overview](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis) — creation and route handlers only
- [Top-level statements](https://learn.microsoft.com/dotnet/csharp/fundamentals/program-structure/top-level-statements)
- [Lambda expressions](https://learn.microsoft.com/dotnet/csharp/language-reference/operators/lambda-expressions)
- [File-scoped namespaces](https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/namespace)
- [`dotnet watch`](https://learn.microsoft.com/dotnet/core/tools/dotnet-watch)
- [`decimal`](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/floating-point-numeric-types) — why `12.5m`

### → Read only after the wall
- [Dependency injection in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/dependency-injection)
- [Dependency injection in .NET](https://learn.microsoft.com/dotnet/core/extensions/dependency-injection)

### → Deeper
- [DI guidelines](https://learn.microsoft.com/dotnet/core/extensions/dependency-injection-guidelines) — the anti-patterns section
- [Logging in .NET](https://learn.microsoft.com/dotnet/core/extensions/logging)

---

## Exercise 2b — How many of these things exist, and for how long?

**Goal:** make singleton / scoped / transient concrete instead of memorised. You will
not read a definition of them until after step 4.

### The task

1. Give the things you registered in 2a an identity generated at construction, and
   return it from an endpoint. Call the endpoint twice. Count how many objects were
   built.
2. Make one endpoint ask for the **same type twice** and return both identities. Are
   they the same object within one call?
3. Register the same class three different ways — three registrations, three identities
   in one response — and compare across two calls. Fill this in from what you observe:

   | Registration | Same within one request? | Same across two requests? |
   |---|---|---|
   | | | |

4. Now break it: make something that lives for the **whole application** depend on
   something that only makes sense **for one request**. Run it.
5. Then check what happens to that same failure when the app is not in Development.

Write one sentence per row of the table, and one on what step 5 implies for shipping.

### → Read now
- [Interfaces](https://learn.microsoft.com/dotnet/csharp/fundamentals/types/interfaces) — why marker interfaces are needed to register three variants
- [`Guid`](https://learn.microsoft.com/dotnet/api/system.guid)

### → Read only after step 4
- [Service lifetimes](https://learn.microsoft.com/dotnet/core/extensions/dependency-injection#service-lifetimes)
- [Scope validation](https://learn.microsoft.com/dotnet/core/extensions/dependency-injection-guidelines#scope-validation)

### → Deeper
- Andrew Lock on captive dependencies — <https://andrewlock.net>

---

## Exercise 2c — Configuration that fails loudly

**Goal:** turn silent misconfiguration into a boot failure. Starts from the typo
experiment in 2a.

### The task

1. Reproduce the silent failure: typo the config key, watch the app serve wrong data
   with a 200. Note how long it would take to notice in production.
2. Make the settings arrive as a **typed object** rather than string lookups — the
   `AppSettings` class you wrote in 2a and could not plug in. Two real settings: the
   roastery display name and a support email address.
3. Make a **missing** value stop the app from starting. Delete the config section and
   prove it.
4. Make an **invalid** value stop it too — a support email that is not an email shape.
5. Give a property a default like `= "Shop"` and re-run step 3. Explain why validation
   now passes and why that is a trap.
6. Edit `appsettings.json` while the app is running. Does the running app see it?
   Should it? Find out what controls that.

Steps 3 and 6 are where the surprises are.

### → Read now
- [Data annotations](https://learn.microsoft.com/dotnet/api/system.componentmodel.dataannotations)
- [Nullable reference types](https://learn.microsoft.com/dotnet/csharp/nullable-references) — why `= null!` behaves differently from `= ""` here
- [Object and collection initializers](https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/object-and-collection-initializers)

### → Read only after step 1
- [Configuration in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/configuration/)
- [Options pattern in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/configuration/options)
- [Options pattern in .NET](https://learn.microsoft.com/dotnet/core/extensions/options)

### → Deeper
- [Options validation](https://learn.microsoft.com/dotnet/core/extensions/options#options-validation)

---

## Exercise 2d — Is this instance fit to receive traffic?

**Goal:** assemble 2a–2c into something real. Mostly assembly, few new concepts.

### The task

A load balancer needs one URL to poll to decide whether this instance should get
traffic. The ops team needs to know *which* subsystem is unhappy and why, not just
yes/no.

1. An endpoint that returns a hardcoded "ok". Curl it.
2. Make it actually consult something — the hopper monitor from 2a and the settings
   from 2c.
3. Give it **three** states, not two: fine · running but impaired · broken. Decide what
   the middle one means for a coffee roastery.
4. Make the HTTP **status code** reflect the state. Before you write it, predict what
   the framework does by default with the middle state, then check. You will be wrong.
5. Return structured detail per subsystem rather than a bare word.
6. Decide whether a load balancer should route traffic to an impaired instance, and
   make the code agree with your answer.

Step 4 is the exercise. Step 6 is the opinion.

### → Read now
- [System.Text.Json overview](https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/overview)
- [Records](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/record) — a good shape for a response DTO

### → Read only after step 3
- [Health checks in ASP.NET Core](https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks)

### → Deeper
- [Kubernetes liveness vs readiness probes](https://kubernetes.io/docs/tasks/configure-pod-container/configure-liveness-readiness-startup-probes/) — where the three states came from

---

## Exercise 2e — Minimal API or controller?

**Goal:** an opinion backed by evidence. `.claude/ROADMAP.md` Phase 0 requires a mix of
both across the first endpoints precisely so this choice is made on evidence.

### The task

1. Expose the **same** health data a second time, as a controller.
2. Notice what you had to duplicate between the two. Fix the duplication.
3. Write down, in your own words: what did each style make easy, and what did each make
   hard? Consider discoverability, testing, routing, filters/middleware, and what
   `Program.cs` looks like once there are thirty endpoints.
4. State which you would rather maintain on this project, and why. Keep both for now.

No wall here — this one is judgement, and the answer is yours.

### → Read now
- [Web API with controllers](https://learn.microsoft.com/aspnet/core/web-api/)
- [Routing](https://learn.microsoft.com/aspnet/core/fundamentals/routing)

### → Deeper
- [Route groups and endpoint filters](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/route-handlers) — how minimal APIs answer the "thirty endpoints" objection

---

## Closing Step 2

Closed 2026-10-03 with `2e: minimal API vs controller, choose minimal APIs`.

Step 2 predates spike → real and had no graduation of its own. Its toys —
`HopperMonitor`, the `/hopper` endpoints, `HopperHealthCheck`, `HealthController` — are
removed in **3-grad**, once there is a real database to health-check instead.

---

# Step 3 — Persistence

`.claude/ROADMAP.md` Phase 0 row: "Postgres + EF Core, first migrations · Covers: EF Core,
migrations, data seeding".

**Business:** 3a none (spike) · 3b–3d Q1 (stock model), Q2 (currency only), Q3 (roast
style), Q4 (bag sizes, grind), Q22 (stock mode per coffee), Q23 (roast styles), Q24
(coffee lifecycle), R-1 (money), R-10 (never hard-delete — coffees).

### Why this order

The roadmap puts "Docker Compose / Aspire host" last in Phase 0, but EF Core needs a
running Postgres first. Starting one container is treated as **mechanics** inside 3a, not
a concept; orchestration proper stays where the roadmap puts it.

3a deliberately uses **no ORM**. Without it, EF Core is an incantation: `DbContext`,
change tracking and migrations each replace a specific pain that has to be felt first.

### What Step 3 leaves behind

3a is a spike; 3b–3d are real. By the end of the step the repo has:

- The first real catalog entity in `Shop.Domain` — the coffee from 3a, which becomes the
  seed of Phase 1's products. Variants, categories and images are Phase 1, not here.
- The `DbContext` and its mapping in `Shop.Infrastructure`.
- A first migration and seed data, committed, so a fresh clone gets the same database.
- How `Application` reaches the database (the `DbContext` directly, or a port it
  defines) is the user's decision in 3b.
- The entity's shape comes from the business decisions in `docs/product/domain.md`
  listed on the **Business** line above. The business gate settles them before 3b
  starts.

**3-grad** then deletes the 3a raw-SQL code and the Step 2 toys, and replaces the hopper
health check with a database one, so `/health` reports something real.

---

## Exercise 3a — What does an ORM actually replace?

**Goal (spike):** feel the two jobs EF Core does — turning rows into objects, and keeping
every copy of the database schema identical. You will NOT use EF Core here. This code is
deleted at 3-grad.

### The task

Code may live anywhere in `Shop.Api` — it is a spike. Where database code *belongs* is a
decision for 3b.

1. Run Postgres in a container on your machine. Connect to it with a SQL client, create a
   table of coffees for sale (name, origin, price per kg) by hand, insert two rows by hand.
2. `GET /coffees` returns those rows as JSON. Open a connection, send SQL, turn each row
   into a C# object yourself. The connection string comes from configuration, the 2c way.
3. `POST /coffees` adds one. Then `GET /coffees?origin=...` filters — build that SQL by
   gluing the origin into the string. Now send an origin of `x' OR '1'='1`.
   **This step is supposed to hurt.** What came back, and why? Fix it before moving on.
4. The shop now needs a roast level on every coffee. Add it end to end. Count every place
   you had to edit. One sentence: what got worse?
5. Throw the database away (remove the container *and its data*) and start a fresh one.
   Your app now fails. One sentence: how would a teammate cloning this repo — or
   production — get the same table, with the same changes, applied in the same order?

Step 3 is the security lesson. Step 5 is the wall.

### → Read now
- [`using` / `await using`](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/using) — connections must be disposed
- [Asynchronous programming](https://learn.microsoft.com/dotnet/csharp/asynchronous-programming/) — every database call here is async
- [Npgsql basic usage](https://www.npgsql.org/doc/basic-usage.html) — the raw .NET driver for Postgres
- [`postgres` Docker image](https://hub.docker.com/_/postgres) — env vars, volumes
- [Docker Compose quickstart](https://docs.docker.com/compose/gettingstarted/)

### → Read only after step 5
- [EF Core overview](https://learn.microsoft.com/ef/core/)
- [Migrations overview](https://learn.microsoft.com/ef/core/managing-schemas/migrations/)

### → Deeper
- [OWASP SQL injection prevention](https://cheatsheetseries.owasp.org/cheatsheets/SQL_Injection_Prevention_Cheat_Sheet.html)
- [Npgsql connection pooling](https://www.npgsql.org/doc/connection-string-parameters.html#pooling) — why opening a connection per request is cheap

---

## Exercise 3b — What does it take to turn rows into objects, and back?

**Goal (real):** feel the two jobs of a `DbContext` — mapping and change tracking — by
doing both by hand first, in the real code. Builds the first real catalog entity
(`.claude/ROADMAP.md` Phase 0, "Postgres + EF Core"). Code lives in:

- `Shop.Domain/Catalog/` — the `Coffee` entity and the small types it needs
- `Shop.Application/Catalog/<UseCase>/` — `ListCoffees`, `PublishCoffee`
- `Shop.Infrastructure/Persistence/` — everything that talks to Postgres
- `Shop.Api` — the endpoints, minimal APIs (2e)

The 3a spike stays untouched and is deleted at 3-grad. Pick routes that do not collide
with it (e.g. under `/catalog`).

**Business:** Q1 = hybrid, Q22 = admin picks stock mode per coffee, Q3 = roast style is
a property of the coffee, Q23 = filter / espresso, Q4 = 250 g and 1 kg (grind is not on
the coffee), Q2 = EUR, R-1 = exact decimal + currency, 2 places, Q24 = draft → published
→ retired, publish needs ≥ 1 priced bag size, retired is final, R-10 = coffees never
deleted. **No stock quantities yet** — that is Phase 1 / 3. Origin stays a plain text
field; process, tasting notes and photos are Phase 1.

### Decide before step 3

How does `Shop.Application` reach the database? It cannot reference
`Shop.Infrastructure`. Two shapes are defensible — try one, form an opinion, and we
discuss it after step 3. This is an architecture decision, so it is yours.

### The task

1. **The coffee, with no database in mind.** Model `Coffee` in `Shop.Domain` as plain
   C#. Every business rule above must be enforced by the class itself: it starts as a
   draft, cannot be published without a priced bag size, cannot leave "retired", and
   offers no way to be deleted. A price that breaks R-1 must be impossible to construct.
   One sentence: **where did the rules end up living, and what can no longer be done to
   a coffee from outside?**
2. **A table for it, by hand.** In your SQL client, create the table(s) this entity
   needs. Decide where the per-bag-size prices live. Insert two coffees by hand — one
   draft, one published.
3. **Read it, by hand.** `ListCoffees`: customers see published coffees only. Write the
   query and the row → `Coffee` mapping yourself with Npgsql, in
   `Shop.Infrastructure`. One sentence: **what did you have to do to get a row back
   into an object whose rules from step 1 forbid building it that way?**
4. **Change it, by hand.** `PublishCoffee`: load a draft, call your domain method, save
   it back. One sentence: **which columns does your UPDATE write, and why those?**
5. **The wall.** Retiring a coffee and changing a bag size price are next. Write the
   save **once**, so it works for publish, retire and price change alike — no UPDATE per
   operation. Try it until it hurts. One sentence: **what would your save need to know
   that only the object, before and after, can tell it?**
6. **Replace your mapping and your save** with the framework feature, in place, against
   the table you made by hand in step 2. Delete the hand-written code it replaces. The
   endpoints and the domain class must not change.
7. **Watch it.** Make the framework print the SQL it sends. Publish a coffee and compare
   its UPDATE with yours from step 4.

Step 1 is the domain lesson. Step 5 is the wall. Step 7 is the payoff.

You will still create the table by hand in 3b. That is deliberate — 3c fixes it.

### → Read now
- [Enums](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/enum)
- [Properties](https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/properties) — private setters, init-only
- [Records](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/record) — value equality, a candidate shape for a price
- [Exceptions — creating and throwing](https://learn.microsoft.com/dotnet/csharp/fundamentals/exceptions/creating-and-throwing-exceptions) — a broken rule in step 1
- [PostgreSQL `CREATE TABLE`](https://www.postgresql.org/docs/current/sql-createtable.html)
- [PostgreSQL numeric types](https://www.postgresql.org/docs/current/datatype-numeric.html) — `numeric(p,s)` and R-1

### → Read only after you hit the wall (step 5)
- [EF Core overview](https://learn.microsoft.com/ef/core/)
- [DbContext lifetime, configuration and initialization](https://learn.microsoft.com/ef/core/dbcontext-configuration/)
- [Change tracking](https://learn.microsoft.com/ef/core/change-tracking/)
- [Creating and configuring a model](https://learn.microsoft.com/ef/core/modeling/)
- [Entity types with constructors](https://learn.microsoft.com/ef/core/modeling/constructors) — answers your step 3 sentence
- [Npgsql EF Core provider](https://www.npgsql.org/efcore/)
- [Simple logging](https://learn.microsoft.com/ef/core/logging-events-diagnostics/simple-logging) — step 7

### → Deeper, once it works
- [Tracking vs no-tracking queries](https://learn.microsoft.com/ef/core/querying/tracking)
- [Backing fields](https://learn.microsoft.com/ef/core/modeling/backing-field)
- [Owned entity types](https://learn.microsoft.com/ef/core/modeling/owned-entities) — one way to map a price
