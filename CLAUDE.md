# Shop — coffee roastery backend (practice project)

ASP.NET Core backend for a specialty coffee roastery. This is a **learning
project**: architecture decisions belong to the user, mechanics are mine.

## Layout

`Shop.slnx` (XML solution format — not `.sln`) with four projects under `src/`:

| Project | Role |
|---|---|
| `Shop.Domain` | Entities, value objects, domain rules. No infrastructure deps. |
| `Shop.Application` | Use cases / vertical slices. Depends on Domain only. |
| `Shop.Infrastructure` | EF Core, external services. Implements Application ports. |
| `Shop.Api` | Composition root, endpoints, DI, middleware. |

Architecture is hybrid Clean + vertical-slice. `scratch/` is a throwaway C#
playground — not part of the solution, ignore it unless asked.

`Directory.Build.props` applies to every project: `net10.0`, nullable enabled,
implicit usings, **`TreatWarningsAsErrors`** — a warning fails the build.

## Build

```
dotnet build          # whole solution
dotnet run --project src/Shop.Api
```

## Reference docs — read on demand, do NOT preload

These are not loaded automatically and should not be read at session start.
Open one only when the prompt actually touches its topic:

- `.claude/ROADMAP.md` — what we are building, the phase order, and what
  "done" means for each phase. **Read before** planning work, choosing the
  next task, estimating scope, or answering "what's next" / "where are we".

ASP.NET Core conventions are **not** in this list — they live in the
`aspnet-conventions` skill, which loads itself when relevant and carries the
full Microsoft Learn guidance in its own `references/` folder.
