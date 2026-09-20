# Curriculum — concept ladder

Organised by **concept dependency**, not by feature. `.claude/ROADMAP.md` says what gets
built; this says what order it is learnable in.

Phase 0 is written out below. Later phases get written when we reach them — writing
exercises for code neither of us has seen would be invention, not planning.

---

## Progress

| Exercise | Concept | Status |
|---|---|---|
| 2a part 1 | Life without a container | **done** — 2026-08-29 |
| 2a part 2 | Registering and resolving | **done** — 2026-09-20 |
| 2b | Service lifetimes | **done** — 2026-09-20 |
| 2c | Configuration → options pattern | **current** |
| 2d | Health checks | not started |
| 2e | Minimal API vs controller | not started |

Commit after each. Small revertable commits are part of the method.

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

### → Read only after step 2
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

Commit message from the roadmap:

```
Add health check endpoint with options pattern and DI lifetime demo
```
