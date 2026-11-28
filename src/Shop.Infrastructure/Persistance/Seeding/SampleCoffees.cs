using Shop.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace Shop.Infrastructure.Persistence.Seeding;

public static class SampleCoffees
{
    public static async Task SeedAsync(ShopDb shopDb, CancellationToken ct = default)
    {
        // Published: on sale, sold from roasted stock.
        var yirgacheffe = new Coffee("Yirgacheffe Kochere", "Ethiopia", RoastStyle.Filter, StockMode.Shelf);
        yirgacheffe.SetPrice(BagSize.Grams250, new Money(14.50m, "EUR"));
        yirgacheffe.SetPrice(BagSize.Kilogram1, new Money(48.00m, "EUR"));
        yirgacheffe.Publish();
        await AddIfMissingAsync(shopDb, yirgacheffe, ct);

        // Draft: priced, not yet on sale, roasted to order.
        var huila = new Coffee("Huila Pink Bourbon", "Colombia", RoastStyle.Espresso, StockMode.RoastToOrder);
        huila.SetPrice(BagSize.Grams250, new Money(16.00m, "EUR"));
        await AddIfMissingAsync(shopDb, huila, ct);

        // Retired: was on sale, now withdrawn. Prices set before Retire, which locks the coffee.
        var santos = new Coffee("Santos Fazenda Ambiental", "Brazil", RoastStyle.Espresso, StockMode.Shelf);
        santos.SetPrice(BagSize.Grams250, new Money(11.00m, "EUR"));
        santos.Publish();
        santos.Retire();
        await AddIfMissingAsync(shopDb, santos, ct);

        // Published: on sale, roasted to order.
        var nyeri = new Coffee("Nyeri Karogoto", "Kenya", RoastStyle.Filter, StockMode.RoastToOrder);
        nyeri.SetPrice(BagSize.Grams250, new Money(15.50m, "EUR"));
        nyeri.SetPrice(BagSize.Kilogram1, new Money(52.00m, "EUR"));
        nyeri.Publish();
        await AddIfMissingAsync(shopDb, nyeri, ct);

        await shopDb.SaveChangesAsync(ct);
    }

    private static async Task AddIfMissingAsync(ShopDb shopDb, Coffee coffee, CancellationToken ct)
    {
        var exists = await shopDb.Coffees.AnyAsync(c => c.Name == coffee.Name && c.Origin == coffee.Origin, ct);
        if (exists) return;
        shopDb.Coffees.Add(coffee);
    }
}