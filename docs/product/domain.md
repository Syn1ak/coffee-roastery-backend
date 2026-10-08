# The Roastery — domain brief

**Status:** draft vision, written by Claude on 2026-10-04. **Only what is marked ✅ in §9 and §7 is decided** — everything else is a proposal.
The user is the product owner / BA. Every ❓ is a decision for them; the answer goes into
§9, and the ❓ becomes ✅.

Decisions are made **just in time, not all at once**. Before each step, phase or
exercise, the `learn` skill's *business gate* finds the questions that work depends on.
Any that are still open get discussed and decided first. Questions nobody has needed yet
stay open.

How to read it:

- §1–3 — what the business is and what the website does
- §4 — who uses it
- §5 — the business timeline, from listing a coffee to delivering it (the core of this doc)
- §6 — the one big modelling decision that shapes everything else
- §7–8 — business rules, and what we are *not* building
- §9 — every open question in one table
- §10 — real websites to compare against

---

## 1. The business in one paragraph

A small **specialty coffee roastery**: 3–10 people, one roasting machine, a few hundred
orders a week. It buys raw ("green") coffee beans from importers, who source them from
specific farms. It roasts them in small batches on its own premises a couple of days a
week. Then it sells the roasted beans in bags **directly to home coffee drinkers through
its own website**. Some customers buy once. Regulars **subscribe** and get a bag every
few weeks. The website is both the shop window and the till — no Amazon, no marketplace.

Three things make this more than "a generic shop":

1. **Coffee goes stale.** Roasted beans taste best roughly 1–4 weeks after roasting and fade
   after that. Every bag carries a roast date, and customers check it.
2. **Coffees are seasonal.** A farm's harvest is bought in a limited amount. When it is
   roasted out, that coffee is gone until next year, maybe for good. The catalog changes
   constantly.
3. **Roasting capacity is limited.** One machine, a few roast days a week. You cannot
   roast an unlimited amount on demand.

---

## 2. Coffee 101 for a developer

Only what the system has to know about.

| Term | Meaning | Why the system cares |
|---|---|---|
| **Green coffee** | Raw, unroasted beans. Keeps about a year. | The roastery's real raw stock. |
| **Lot** | One purchase of green coffee from one farm and harvest, e.g. 20 sacks × 60 kg. | Finite. When it runs out, the coffee is retired. |
| **Origin** | Country → region → farm or washing station. "Ethiopia, Guji, Hambela." | The main thing customers filter by. |
| **Process** | How the coffee fruit was removed from the bean: washed, natural, honey, anaerobic. | Big influence on taste; a filter. |
| **Single origin / blend** | One lot, vs several mixed for a consistent taste year-round. | Blends are permanent products; single origins come and go. |
| **Roast style** | How dark. Specialty roasters usually say **filter** (lighter) or **espresso** (a bit darker). Some do one "omni" roast for both. | Is it a separate product or a variant? → **Q3** |
| **Batch** | One run of the roaster. 12 kg green in → about 10 kg roasted out. Beans lose **~15–20 % of their weight** roasting. | Links a bag to its roast date. Matters for stock math (§6). |
| **Roast date** | The day a batch was roasted. Printed on every bag. | Freshness rules, customer trust. |
| **Rest** | Beans taste best after resting ~5–14 days. Shipping very fresh is fine; shipping old is not. | Why "roasted on" matters at all. |
| **Tasting notes** | "Peach, jasmine, black tea." Marketing language, not flavouring. | Product text; can be filtered. |
| **Grind** | Whole bean, or ground for espresso / filter / French press / etc. | Usually done **when packing**, so it affects the order, not the stock. → **Q4** |
| **Bag size** | Typically 250 g for home drinkers and 1 kg for heavy users. | Different price. Maybe different stock. |

---

## 3. What the website is

This project builds the **backend API**. The pages below are what that API serves. Whether we
also build a frontend is **Q16**.

| Area | What a visitor sees and does |
|---|---|
| **Shop** | List of coffees. Filter by origin, process, roast style and tasting notes; sort by price or newest; search. Sold-out coffees shown or hidden. |
| **Coffee page** | Story, origin details, tasting notes, photos. Pick bag size and grind, see price and stock, plus "roasted on" or "ships on" (depends on §6). |
| **Cart** | Edit items, apply coupon, see shipping cost and total. Works without an account. |
| **Checkout** | Email, address, shipping method, payment. |
| **Account** | Sign in with email or Google / Microsoft. Order history, live order status, saved addresses. |
| **Subscriptions** | Choose a coffee or "roaster's choice", bag size, grind and frequency. Manage it later: skip, pause, change or cancel. |
| **Admin** (staff only) | Catalog, prices, stock, roast days, orders to pack, refunds, coupons, reports. |

---

## 4. Who uses it

| Actor | Who | What they need from the system |
|---|---|---|
| **Visitor** | Anyone, not signed in | Browse, build a cart, maybe check out as a guest (**Q5**) |
| **Customer** | Has an account | Buy, see own orders and their status, manage addresses |
| **Subscriber** | A customer with an active subscription | Get coffee on schedule without thinking; skip or pause easily |
| **Roaster** | Staff in production | "What do I roast on the next roast day, and how much?" and "What do I pack today?" |
| **Support** | Staff answering emails | Find any order, see what happened to it, cancel or refund |
| **Owner / Admin** | Runs the business | Catalog, prices, coupons, stock, reports, who changed what |

External systems: **payment provider** (Stripe in the roadmap), **email**, **Google /
Microsoft sign-in**, and a **shipping carrier** (tracking numbers only, see §8).

---

## 5. The business timeline

A text version of *event storming*. Each line is something that **happens** in the
business, in the past tense, in order. For each one: who causes it, the rules that apply,
and the open questions.

### 5.1 A coffee's life in the catalog

1. **Lot received.** Admin records that green coffee arrived: which coffee, how many kg.
   *(Only needed with the roast-to-order model in §6.)*
2. **Coffee drafted.** Admin creates a coffee: name, origin, process, roast style, tasting
   notes, photos, and a price per bag size. Not visible to customers yet.
3. **Coffee published.** Now visible and buyable. Rule: it must have at least one bag
   size with a price.
4. **Price changed.** Admin edits a price. Carts already holding this coffee → **Q7**.
5. **Batch roasted.** Roaster records "roasted 10 kg of Guji on 6 Oct". What this does to
   stock depends on §6.
6. **Coffee sold out.** Stock reaches zero; the shop shows "sold out". A "notify me when
   back" button? → **Q20**
7. **Coffee retired.** The season is over. The coffee disappears from the shop, but past
   orders still show it. Never deleted.

### 5.2 Shopping

1. **Item added to cart.** Coffee + bag size + grind + quantity. Guests have carts too.
2. **Customer signed in.** If the guest cart has items and the account cart also has
   items → **Q6**.
3. **Coupon applied.** Which kinds of coupon exist → **Q11**.
4. **Checkout started.** Address and shipping method chosen, totals calculated. Stock is
   **reserved** so two people cannot buy the last bag → **Q8**.
5. **Payment succeeded → Order placed.** Confirmation email sent.
   **Payment failed** → the customer can retry; after a time limit the reservation is
   released.

### 5.3 Fulfilment

1. **Order scheduled.** Assigned to the next roast day or packing day (depends on §6 and
   **Q19**).
2. **Order packed.** Bags taken or roasted, ground if needed, and labelled with the roast date.
3. **Order shipped.** Staff enter a tracking number. The customer gets an email and
   sees the status change live.
4. **Order delivered.** Who knows it arrived → **Q14**.
5. **Order cancelled.** Until when a customer can cancel → **Q9**.
6. **Order refunded.** Full or partial, by staff, with a reason → **Q10**.

### 5.4 Subscriptions

1. **Subscription started.** Coffee (or "roaster's choice"), bag size, grind, frequency,
   saved payment method.
2. **Shipment due.** A background job creates a normal order a few days before the next
   roast or ship day and charges the customer automatically.
3. **Subscription payment failed.** Retry schedule; what happens after repeated
   failures → **Q12**.
4. **Subscription skipped / paused / resumed / changed / cancelled.** By the subscriber,
   up to a cut-off before the next shipment.
5. **Subscribed coffee retired.** What the subscriber gets instead → **Q13**.

### 5.5 Running the business

- **Report requested.** Sales per coffee and per day, revenue, subscriber count, and
  coupon usage.
- **Change audited.** Every change to a price, stock level, order status or refund
  records who did it, when, and old → new.
- **Catalog imported.** Admin uploads a CSV of coffees at the start of a season.

---

## 6. The big decision: what is "stock"?

This one choice changes the catalog (Phase 1), the cart (Phase 3) and fulfilment
(Phase 4). Real roasteries use all three models below. → **Q1**

### (a) Roasted stock — "sell what's on the shelf"

The roaster roasts ahead of time. Each batch becomes N bags of each size, each with a roast
date. The website sells from those bags.

- **Stock** = number of bags, per coffee + bag size (+ which batch).
- Sell the oldest bags first. Bags older than X weeks are pulled or discounted → **Q18**.
- Grind is applied while packing, so it does not affect stock.
- ➕ Classic e-commerce, simplest to model. "The last bag" is an easy-to-see concurrency
  problem.
- ➖ Waste when you guess demand wrong. Customers may get bags that are 3 weeks old.

### (b) Roast to order — "we roast it for you"

There is no roasted stock. Orders collect until a **cut-off** (e.g. Sunday and Wednesday
midnight). The roaster roasts exactly what was ordered the next morning, and it ships the
same or next day.

- **Stock** = kg of **green** coffee left in each lot. Selling a 250 g bag uses
  ~300 g of green coffee, because of roast weight loss.
- Optionally, roasting capacity per roast day is limited too.
- The system creates a **roast plan**: "Monday: 14.2 kg green Guji, 8.7 kg green
  Huila…" It is the sum of all orders before cut-off.
- The coffee page shows "ships Tuesday", not "roasted on".
- ➕ Always fresh. Very common among small specialty roasteries. It's the part that makes
  this domain *unique* rather than a generic shop. The "last kg" problem is the same
  concurrency lesson as "the last bag".
- ➖ More domain logic: kg conversions, cut-offs, roast plan. It adds a "roast plan"
  feature the roadmap doesn't have yet.

### (c) Hybrid

Roasted stock for steady sellers (blends), roast-to-order for everything else. This is the
most realistic and the most complex. Not recommended for a learning project.

**Claude's recommendation: (b) roast to order.** It is what makes this a *roastery* and
not a T-shirt shop. The concurrency problem the roadmap wants to teach is still there. The
extra logic (kg math, cut-offs, roast plan) is the good kind for a rich `Shop.Domain`.
Choose **(a)** if you want the simplest model, so you can spend the effort on ASP.NET
instead.

---

## 7. Business rules — proposed defaults

Each one is Claude's proposal until confirmed in §9.

| # | Rule |
|---|---|
| R-1 | ✅ One currency (EUR, Q2). Money is a decimal amount + currency, rounded to 2 decimal places. Never `double`. — accepted 2026-10-08 |
| R-2 | Displayed prices include tax (the EU convention) → **Q2** |
| R-3 | A cart line remembers the price at the moment it was added. At checkout, if the current price differs, the customer sees the new price and must confirm it. |
| R-4 | Stock is reserved when checkout starts and released after 15 minutes without payment. |
| R-5 | Overselling is impossible: two simultaneous checkouts for the last unit → exactly one succeeds. |
| R-6 | A customer can cancel until the cut-off for the order's roast or packing day. After that, only staff can. |
| R-7 | Refunds are made by staff only, full or partial, always with a reason, and never more than was paid. |
| R-8 | One coupon per order. A coupon is a percentage or a fixed amount, with an expiry date, an optional minimum order, and usage limits (total and per customer). |
| R-9 | Free shipping above a threshold amount → **Q15** |
| R-10 | Coffees, customers and orders are never hard-deleted: orders must always be able to show what was bought. — ✅ **coffees** accepted 2026-10-08. Customers (GDPR right to erasure) and orders: confirm at the Phase 2 / Phase 4 gates |
| R-11 | Every change to a price, stock, order status or refund is audited: who, when, old → new. |
| R-12 | Customers see only their own orders. Support sees all orders. Only Admin changes the catalog, prices and coupons. |
| R-13 | A subscription order is a normal order: same prices, same stock rules, same fulfilment. |
| R-14 | A guest order can be claimed later by registering with the same email. |

---

## 8. Out of scope — proposed

| Not building | Why |
|---|---|
| **Wholesale / B2B** (selling to cafés) | Real roasteries do it, but it's a second business: price lists, invoices paid later, standing orders. → **Q17** |
| Green coffee buying and contracts | We only record "a lot arrived, N kg". |
| Roasting machine control and roast profiles | Specialised tools such as Cropster do this. |
| Printing shipping labels, carrier APIs | Staff type in the tracking number. |
| Multi-currency, multi-language | One market → **Q2** |
| Café / point of sale | Different system. |
| Gift cards, reviews, loyalty points | Could be added later; not core. |

---

## 9. Open questions — your decisions

Fill in the last column. "Default" is Claude's suggestion. You can accept it, change it or
reject it.

| # | Question | Why it matters | Default | Decision |
|---|---|---|---|---|
| **Q1** | Stock model: (a) roasted stock, (b) roast to order, (c) hybrid? | Shapes Phases 1, 3 and 4 (§6) | (b) | ✅ **(c) hybrid** — 2026-10-07 |
| **Q2** | Which country/market does the shop sell in? | Currency, whether prices include tax, shipping carrier, payment provider (Stripe does not accept merchants from every country; its test mode works anywhere for learning) | One EU country, EUR, tax-inclusive prices, domestic shipping only | ✅ **Currency: EUR** — 2026-10-08. Country, tax, shipping: not decided yet |
| **Q3** | Filter roast vs espresso roast of the same lot: one product with a variant, or two products? | The shape of the product model | Two products; roast style is a property of the coffee | ✅ **(i) two products, roast style is a property of the coffee** — 2026-10-08 |
| **Q4** | Which bag sizes and grinds? Is grind a stock-affecting variant or a free order option? | Variant model | 250 g and 1 kg; grind = free option chosen per order line | ✅ **250 g and 1 kg; grind = free option per order line** — 2026-10-08 |
| **Q5** | Can guests check out without an account? | Identity, orders without a user | Yes (+ R-14) | |
| **Q6** | Guest cart + account cart on sign-in: merge, keep one, or ask? | Cart merge logic | Merge, adding up quantities | |
| **Q7** | Price changes while an item is in a cart: old price, new price, or confirm? | Price snapshot rule | Confirm at checkout (R-3) | |
| **Q8** | When is stock reserved, and for how long? | Concurrency, abandoned checkouts | At checkout start, 15 min (R-4) | |
| **Q9** | Until when can a customer cancel? | Order state machine | Until cut-off (R-6) | |
| **Q10** | Partial refunds allowed? | Payment and order states | Yes, by staff (R-7) | |
| **Q11** | Coupon kinds and limits? Do coupons apply to subscriptions? | Discount rules | R-8; not on subscriptions | |
| **Q12** | Subscriptions: which frequencies? "Roaster's choice"? Skip/pause? Failed-payment policy? | Background jobs, billing | Every 1 / 2 / 4 weeks; roaster's choice yes; skip and pause yes; 3 retries over 7 days, then auto-pause and email | |
| **Q13** | A subscriber's coffee is retired — then what? | Subscription rules | Switch to roaster's choice and email them | |
| **Q14** | How do we know an order was delivered? | Last order state | Staff mark it, or it's assumed after N days; no carrier integration | |
| **Q15** | Shipping cost: flat rate, free above a threshold, or by weight? | Checkout totals | Flat rate, free above a threshold | |
| **Q16** | Do we build a frontend, or only the API? | Project scope | API only (OpenAPI + `.http` files); maybe a small client late | |
| **Q17** | Wholesale (B2B) in scope? | Doubles the domain | No | |
| **Q18** | (Q1 = a or c) Max age of a roasted bag before it's pulled? | Freshness rule | 4 weeks | |
| **Q19** | (Q1 = b or c) Which roast days, cut-off time, max kg per roast day? | Roast plan, "ships on" date | Mon + Thu roasting, cut-off 23:59 the day before, 60 kg capacity per day | |
| **Q20** | "Notify me when back in stock"? | Extra feature | No — seasonal coffees rarely come back | |
| **Q21** | The shop's name? | Not needed — just nicer than "Shop" | — | |
| **Q22** | (Q1 = c) How is it decided whether a coffee is sold from roasted stock or roasted to order? | Whether a coffee carries a stock mode, and who sets it | Admin picks per coffee; new coffees default to roast to order | ✅ **(ii) admin picks per coffee** — 2026-10-07. Starting value for a new coffee: not decided yet |
| **Q23** | Which roast styles does the shop offer? | The allowed values of a coffee's roast style (Q3) | Filter and espresso | ✅ **(i) filter and espresso only** — 2026-10-08 |
| **Q24** | A coffee's lifecycle (§5.1): is there a hidden draft state? What must be true to publish? Can a retired coffee come back? | Coffee states and their rules | Draft → published → retired. Publish needs at least one bag size with a price. Retired is final; next year's harvest is a new coffee | ✅ **all three defaults** — 2026-10-08 |

---

## 10. Is this a copy of something?

**As a business: yes.** This is the standard model of a specialty roastery's own webshop.
There are thousands. **As software: no.** A real roastery this size would not write
it from scratch. They use Shopify or WooCommerce, a subscription app on top, and a
roastery tool for production. We write it ourselves because the point of the project is
the backend. So the business is ordinary on purpose, and the backend is the part that's
ours.

### Roastery webshops — study these like a BA

Go through 2–3 of them and pretend to buy a coffee, up to the payment page. Write down
**every choice you were asked to make**: each choice becomes a field or a rule in our
system. Then read their shipping / FAQ page and look for roast days, cut-offs and
freshness promises. Open a subscription page and note what can be configured.

- Onyx Coffee Lab (US) — <https://onyxcoffeelab.com>
- Counter Culture Coffee (US) — <https://counterculturecoffee.com>
- Stumptown Coffee (US) — <https://www.stumptowncoffee.com>
- Blue Bottle Coffee (US) — <https://bluebottlecoffee.com>
- Sey Coffee (US) — <https://www.seycoffee.com>
- Square Mile Coffee Roasters (UK) — <https://shop.squaremilecoffee.com>
- Tim Wendelboe (Norway) — <https://timwendelboe.no>
- Coffee Collective (Denmark) — <https://coffeecollective.dk>
- La Cabra (Denmark) — <https://lacabra.com>

### What we are *not*

- Trade Coffee — <https://www.drinktrade.com> — a **marketplace** selling many roasters'
  coffee. It's a different business: no roasting, many suppliers.

### The software real roasteries use instead of writing this

- Cropster — <https://www.cropster.com> — production side: green stock, roast plans and
  roast logs. Worth a look for §6 (b).
- Recharge — <https://getrecharge.com> — subscriptions bolted onto Shopify. Its feature
  list is a good checklist for §5.4.

### Coffee background

- Specialty Coffee Association — <https://sca.coffee>
- Sweet Maria's coffee library — <https://library.sweetmarias.com> — origins, processes,
  roasting, written for home roasters.

---

## 11. After the decisions

1. Answers are written into §9 with a date, at the business gate of the step that needs
   them.
2. `.claude/ROADMAP.md` is updated to follow them: for example, a roast-plan feature if
   Q1 = (b), or the subscription scope from Q12.
3. Exercise 3b designs the first real entity in `Shop.Domain` from the answers to Q1, Q3
   and Q4. Those are the first gate.
4. Each later phase starts at its own gate, and goes deeper into its own rules there.
