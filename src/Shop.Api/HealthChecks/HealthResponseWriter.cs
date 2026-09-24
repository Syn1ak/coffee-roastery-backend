using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Shop.Api.HealthChecks;

public record HealthResponse(
    string Status,
    double TotalDurationMs,
    IEnumerable<HealthCheckEntry> Checks);

public record HealthCheckEntry(
    string Name,
    string Status,
    string? Description,
    double DurationMs,
    IReadOnlyDictionary<string, object> Data);

public static class HealthResponseWriter
{
    public static Task WriteJson(HttpContext context, HealthReport report)
    {
        var response = new HealthResponse(
            report.Status.ToString(),
            report.TotalDuration.TotalMilliseconds,
            report.Entries.Select(e => new HealthCheckEntry(
                e.Key,
                e.Value.Status.ToString(),
                e.Value.Description,
                e.Value.Duration.TotalMilliseconds,
                e.Value.Data)));

        return context.Response.WriteAsJsonAsync(response);
    }
}
