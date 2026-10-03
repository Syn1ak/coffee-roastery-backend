using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Shop.Api.HealthChecks;

public static class HealthResponseWriter
{
    public static Task WriteJson(HttpContext context, HealthReport report)
    {
        var response = HealthResponse.From(report);

        return context.Response.WriteAsJsonAsync(response);
    }
}
