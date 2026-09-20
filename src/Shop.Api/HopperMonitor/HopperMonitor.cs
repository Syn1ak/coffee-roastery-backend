using Microsoft.Extensions.Options;
using Shop.Api.Configuration;

namespace Shop.Api.HopperMonitor;

public interface IId
{
    Guid Id { get; }

     decimal GetSize();

     string GetSentence();
}

public interface IIdTransient : IId { }
public interface IIdScoped : IId { }
public interface IIdSingleton : IId { }

public class HopperMonitor : IIdSingleton, IIdScoped, IIdTransient
{
    private readonly ILogger<HopperMonitor> _logger;

    private readonly IOptions<RoasterySettings> _roasterySettings;

    public Guid Id { get; }

    private readonly decimal _size = 9.5m;
    public HopperMonitor(ILogger<HopperMonitor> logger, IOptions<RoasterySettings> roasterySettings)
    {
        _logger = logger;
        _roasterySettings = roasterySettings;
        Id = Guid.NewGuid();
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