using Shop.Api.HopperMonitor;

namespace Shop.Api.HopperAlertTracker;

public class HopperAlertTracker(IIdSingleton monitor)
{
    public Guid Id = monitor.Id;

    public Guid GetStoredId()
    {
        return Id;
    }
}