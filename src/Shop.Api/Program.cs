var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseHttpsRedirection();

var displayName = builder.Configuration["ShopDisplayName"] ?? "Shop";

app.MapGet("/hopper", () => new HopperMonitor(app.Logger, displayName).GetSize());

app.MapGet("/hopper/summary", () => new HopperMonitor(app.Logger, displayName).GetSentence());

app.Run();

public class HopperMonitor(ILogger logger, string shopDisplayName)
{
    private const decimal Size = 9.5m;

    public decimal GetSize()
    {
        if (Size <= 10m)
        {
            logger.LogWarning("Hopper is low: {Size}", Size);
        }
        return Size;
    }

    public string GetSentence()
    {
        return $"It is sentence {shopDisplayName}";
    }

    public string GetShopDisplayName()
    {
        return shopDisplayName;
    }
}

public class AppSettings
{
    public string ShopDisplayName { get; set; } = string.Empty;
}
