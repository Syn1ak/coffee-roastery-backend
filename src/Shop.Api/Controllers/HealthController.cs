using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shop.Api.HealthChecks;

namespace Shop.Api.Controllers;

[ApiController]
[Route("health")]
public class HealthController(HealthCheckService healthCheckService) : ControllerBase
{

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        HealthReport healthReport = await healthCheckService.CheckHealthAsync(ct);
        var statusCode = HealthStatusCodes.ByStatus[healthReport.Status];
        return StatusCode(statusCode, HealthResponse.From(healthReport));
    }
}