using Shop.Domain.Catalog;
using Shop.Infrastructure.Persistence;

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

        // Draft: priced, not yet on sale, roasted to order.
        var huila = new Coffee("Huila Pink Bourbon", "Colombia", RoastStyle.Espresso, StockMode.RoastToOrder);
        huila.SetPrice(BagSize.Grams250, new Money(16.00m, "EUR"));

        // Retired: was on sale, now withdrawn. Prices set before Retire, which locks the coffee.
        var santos = new Coffee("Santos Fazenda Ambiental", "Brazil", RoastStyle.Espresso, StockMode.Shelf);
        santos.SetPrice(BagSize.Grams250, new Money(11.00m, "EUR"));
        santos.Publish();
        santos.Retire();

        shopDb.Coffees.AddRange(yirgacheffe, huila, santos);

        await shopDb.SaveChangesAsync(ct);
    }
}