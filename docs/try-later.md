# Try later — alternatives to explore once the shop is finished

Not a backlog and not a roadmap. These are **other ways of doing something we already
chose**, kept so they can be tried and compared once the app works end to end. Each
entry says what we chose, what the alternative is, and why it was parked.

---

## Application reaches the database through the DbContext (option A)

**Chosen instead (3b, 2026-10-08):** option B, a repository port. `Shop.Application`
defines what it needs ("give me published coffees", "save this coffee") and
`Shop.Infrastructure` implements it.

**The alternative:** `Shop.Application` defines an interface over the database itself
(e.g. `IShopDbContext` exposing `DbSet<Coffee>`), and each use case writes its own
LINQ query. Infrastructure's `DbContext` implements that interface.

**What to compare when trying it:**
- Phase 1 catalog queries (filter, sort, search, paginate): one query per use case vs a
  growing list of repository methods.
- How much of the repository just passes calls through to EF Core.
- Testing: a fake repository vs a real or in-memory database.
- Application now references the EF Core package. Does the Phase 7 architecture test
  still describe the dependency rule honestly?

---

## Multiple currencies

**Chosen instead (Q2 + R-1, 2026-10-08):** one currency, EUR. Every price is still
stored as amount + currency, so the data is ready for more than one.

**The alternative:** sell in several currencies.

**What it would actually take** — storing the currency is only the first step:
- A **price list per currency**. Roasteries set round prices per market (€12.50, £11.00);
  they do not convert at today's exchange rate.
- **Rounding per currency.** R-1's "2 decimal places" becomes per-currency: the Japanese
  yen has none.
- **No mixed carts.** One cart, one order, one currency.
- **Reports** can no longer simply add amounts together.
- Payment provider settlement in more than one currency.

Out of scope in `docs/product/domain.md` §8 — this is an experiment, not a feature.
