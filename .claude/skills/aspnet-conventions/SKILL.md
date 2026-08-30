---
name: aspnet-conventions
description: ASP.NET Core correctness and performance rules for this repo — async/await and thread-pool starvation, HttpContext lifetime and background work, EF Core query efficiency, HttpClientFactory, response headers and status codes, pagination, exceptions as control flow. Use before writing or reviewing any endpoint, controller, minimal API handler, middleware, DI registration, Program.cs change, EF Core query, or background/hosted service.
---

# ASP.NET Core conventions

Apply these when writing or reviewing server-side code in `src/Shop.Api`,
`src/Shop.Infrastructure`, or any handler that touches `HttpContext`.

Full source with rationale and code samples:
`references/aspnet-core-best-practices.md` (relative to this skill) — read it
when a rule below is not specific enough for the case at hand. Do not read it
by default.

## Async — the highest-value rules

- Make the **entire call stack** async: endpoint → handler → repository → EF Core.
- Never block on async: no `Task.Wait()`, no `.Result`, no `GetAwaiter().GetResult()`.
  This is sync-over-async and causes thread-pool starvation under load.
- Never wrap a synchronous API in `Task.Run` to "make it async", and never
  `await Task.Run(...)` immediately — ASP.NET Core already runs on pool threads.
- Never use `async void` in a handler. Return `Task` so the framework waits for
  completion; `async void` completes the HTTP request at the first `await`.
- Read request bodies and forms asynchronously: `ReadToEndAsync`,
  `ReadFormAsync` — **not** `ReadToEnd` or `HttpContext.Request.Form`.
- Don't buffer whole request/response bodies into memory; stream them.

## HttpContext lifetime

`HttpContext` is **not thread-safe** and **not valid after the request ends**.

- Don't store `IHttpContextAccessor.HttpContext` in a field.
- Don't touch `HttpContext` from multiple threads in parallel — copy the values
  you need (path, headers, user id) into locals *before* fanning out.
- Don't capture `HttpContext` or a scoped service (`DbContext`!) in a closure
  handed to background work. A request-scoped `DbContext` used after the request
  completes throws `ObjectDisposedException`. Copy the data out, or resolve a
  fresh scope from `IServiceScopeFactory` inside the background work.
- Long-running work does not belong in a request. Use a hosted/background
  service or a queue.

## EF Core and data access

- Every data-access call async.
- `AsNoTracking()` for read-only queries.
- Filter, project, and aggregate **in the database** — keep `.Where`, `.Select`,
  `.Sum` translatable; watch for silent client-side evaluation.
- Fetch only the columns needed; project to a DTO rather than loading entities.
- Avoid projection queries over collections that fan out into N+1 SQL calls.
- Minimize round trips — one call beats several.
- Cache frequently read, slow-changing data (`IMemoryCache` / distributed cache)
  when slightly stale data is acceptable.

## Endpoints and responses

- Paginate every collection endpoint (page size + index). Never return an
  unbounded list.
- Don't modify status code or headers after the response body has started —
  check `HttpResponse.HasStarted`, or set them via `HttpResponse.OnStarting`.
- In middleware, don't call `next()` once you've written to the response body.

## Misc

- `HttpClient`: resolve via `IHttpClientFactory`. Never `new HttpClient()` per
  call and dispose it — that exhausts sockets.
- Exceptions are for unexpected conditions, never normal control flow —
  especially on hot paths. Detect and handle the expected case instead.
- `HttpRequest.ContentLength` can be null; don't assume otherwise.
- Avoid allocating many short-lived large (>85 KB, LOH) objects on hot paths;
  pool buffers with `ArrayPool<T>`.

## Repo-specific

- `TreatWarningsAsErrors` is on solution-wide — a nullable warning is a build
  failure, so resolve nullability properly rather than sprinkling `!`.
- Keep `Shop.Domain` free of ASP.NET Core and EF Core references; these rules
  apply to the Api and Infrastructure layers.
