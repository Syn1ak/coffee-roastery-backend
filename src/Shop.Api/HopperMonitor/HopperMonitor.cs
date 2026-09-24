using Microsoft.Extensions.Options;
using Shop.Api.Configuration;

namespace Shop.Api.Hopper;

public class HopperMonitor
{
    private readonly ILogger<HopperMonitor> _logger;

    private readonly IOptions<RoasterySettings> _roasterySettings;

    public Guid Id { get; }

    private readonly decimal[] _sizes = { 150m, 50m, 5m, 0m };

    private int _counter = 0;

    public HopperMonitor(ILogger<HopperMonitor> logger, IOptions<RoasterySettings> roasterySettings)
    {
        _logger = logger;
        _roasterySettings = roasterySettings;
        Id = Guid.NewGuid();
    }

    public decimal GetSize()
    {
        int next = Interlocked.Increment(ref _counter);
        int idx = next % _sizes.Length;
        decimal curSize = _sizes[idx];
        if (curSize <= 10m)
        {
            _logger.LogWarning("Hopper is low: {Size}", curSize);
        }
        return curSize;
    }

    public string GetSentence()
    {
        return $"It is sentence {GetShopDisplayName()}";
    }

    public string GetId()
    {
        return $"Id {Id}";
    }

    public string GetShopDisplayName()
    {
        return _roasterySettings.Value.ShopDisplayName;
    }
}