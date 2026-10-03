using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Shop.Api.HealthChecks;

public record HealthResponse(
    string Status,
    double TotalDurationMs,
    IEnumerable<HealthCheckEntry> Checks)
{
    public static HealthResponse From(HealthReport report)
    {
        return new HealthResponse(
            report.Status.ToString(),
            report.TotalDuration.TotalMilliseconds,
            report.Entries.Select(HealthCheckEntry.From));
    }
}

public record HealthCheckEntry(
    string Name,
    string Status,
    string? Description,
    double DurationMs,
    IReadOnlyDictionary<string, object> Data)
{
    public static HealthCheckEntry From(KeyValuePair<string, HealthReportEntry> e)
    {
        return new HealthCheckEntry(
                e.Key,
                e.Value.Status.ToString(),
                e.Value.Description,
                e.Value.Duration.TotalMilliseconds,
                e.Value.Data);
    }
}