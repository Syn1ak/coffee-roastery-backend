
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shop.Api.Hopper;

namespace Shop.Api.HealthChecks;

public class HopperHealthCheck : IHealthCheck
{
    private readonly HopperMonitor _monitor;

    public HopperHealthCheck(HopperMonitor monitor)
    {
        _monitor = monitor;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default
    )
    {
        decimal size = _monitor.GetSize();
        var data = new Dictionary<string, object>
        {
            ["sizeKg"] = size
        };

        if (size > 10)
        {
            return Task.FromResult(HealthCheckResult.Healthy($"Hopper ok: {size} kg", data));
        }

        if (size > 0)
        {
            return Task.FromResult(HealthCheckResult.Degraded($"Hopper low: {size} kg", data: data));
        }

        return Task.FromResult(HealthCheckResult.Unhealthy("Hopper empty", data: data));
    }
}