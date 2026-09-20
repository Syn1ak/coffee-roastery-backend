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

    public Guid Id { get; }

    private readonly decimal _size = 9.5m;
    private readonly string _shopDisplayName;

    public HopperMonitor(ILogger<HopperMonitor> logger, IConfiguration config)
    {
        _logger = logger;
        _shopDisplayName = config["ShopDisplayName"] ?? "Shop";
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
        return $"It is sentence {_shopDisplayName}";
    }

    public string GetId()
    {
        return $"Id {Id}";
    }

    public string GetShopDisplayName()
    {
        return _shopDisplayName;
    }
}