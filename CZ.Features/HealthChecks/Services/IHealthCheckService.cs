// IHealthCheckService.cs

using api.CZ.Core.ResultPattern;

namespace api.CZ.Features.HealthChecks.Services;

public interface IHealthCheckService
{
    Task<Result<HealthCheckResponse>> CheckHealthAsync(CancellationToken cancellationToken = default);
}

public record HealthCheckResponse(
    string Status,
    double TotalDurationMs,
    List<HealthCheckEntry> Checks
);

public record HealthCheckEntry(
    string Name,
    string Status,
    double DurationMs,
    string? Description = null,
    string? Error = null
);