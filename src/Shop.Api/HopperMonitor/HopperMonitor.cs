namespace Shop.Api.HopperMonitor;

public class HopperMonitor
{
    private readonly ILogger<HopperMonitor> _logger;

    private readonly decimal _size = 9.5m;
    private readonly string _shopDisplayName;

    public HopperMonitor(ILogger<HopperMonitor> logger, IConfiguration config)
    {
        _logger = logger;
        _shopDisplayName = config["ShopDisplayName"] ?? "Shop";
    }

    public decimal GetSize()
    {
        if (_size <= 10m)
        {
            _logger.LogWarning("Hopper is low: {Size}", _size);
        }
        return _size;
    }

    public string GetSentence()
    {
        return $"It is sentence {_shopDisplayName}";
    }

    public string GetShopDisplayName()
    {
        return _shopDisplayName;
    }
}