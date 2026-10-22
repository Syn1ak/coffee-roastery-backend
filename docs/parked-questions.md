# Parked questions — things to understand properly later

Not a backlog and not alternatives (those live in `docs/try-later.md`). These are
**concepts the user wants a clear picture of**, parked because digging in right then
would have cost more time than the current step was worth. Each entry says what is
unclear, where it came up, and where to start.

---

## How `dotnet ef` actually loads the EF Design library (and why it lives in `Shop.Api`)

**Came up:** 3c step 5, 2026-10-22 — adding `Microsoft.EntityFrameworkCore.Design` to
`Shop.Api` so `dotnet ef migrations add` works.

**What is known so far:**
- `dotnet-ef` (the tool, in `dotnet-tools.json`) is a thin launcher. The code that writes
  migration files and the model snapshot is in the **Design** package.
- The tool builds and starts the **startup project** (`Shop.Api`), runs `Program.cs` up
  to `builder.Build()`, and gets a `ShopDb` from the container.
- The Design package is referenced from `Shop.Api` with `PrivateAssets=all` (does not
  flow to other projects) and `IncludeAssets` without `compile` (our own code cannot
  call it). Keeping it in the app keeps the generator on the same EF version (10.0.4)
  as the app.

**What is still unclear — the deep "how":**
- How exactly the tool loads `Shop.Api.dll` and finds the Design library inside it.
- What `PrivateAssets` / `IncludeAssets` really control, step by step, and what would
  break if the package sat in `Shop.Infrastructure` instead.
- How the tool stops the app at `builder.Build()`.

**Start here:**
- [EF Core tools reference (.NET CLI)](https://learn.microsoft.com/ef/core/cli/dotnet)
- [Design-time DbContext creation](https://learn.microsoft.com/ef/core/cli/dbcontext-creation)
- [Controlling dependency assets (`PrivateAssets` / `IncludeAssets`)](https://learn.microsoft.com/nuget/consume-packages/package-references-in-project-files#controlling-dependency-assets)
