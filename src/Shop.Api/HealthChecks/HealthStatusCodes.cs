
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Shop.Api.HealthChecks;

static class HealthStatusCodes
{
    public static readonly IReadOnlyDictionary<HealthStatus, int> ByStatus = new Dictionary<HealthStatus, int>
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        // Hopper is shared by all instances; 503 here would pull every instance at once.
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    };
}