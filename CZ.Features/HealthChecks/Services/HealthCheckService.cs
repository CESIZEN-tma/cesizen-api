using System.Diagnostics;
using api.CZ.Core.ResultPattern;
using api.CZ.Data.EFCore;
using Microsoft.EntityFrameworkCore;


namespace api.CZ.Features.HealthChecks.Services;

public class HealthCheckService : IHealthCheckService
{
    private readonly CesiZenDbContext _dbContext;
    private readonly ILogger<HealthCheckService> _logger;

    public HealthCheckService(
        CesiZenDbContext dbContext,
        ILogger<HealthCheckService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<HealthCheckResponse>> CheckHealthAsync(
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var checks = new List<HealthCheckEntry>();

        // Check API
        var apiCheck = CheckApi();
        checks.Add(apiCheck);

        // Check Database
        var dbCheck = await CheckDatabaseAsync(cancellationToken);
        checks.Add(dbCheck);

        stopwatch.Stop();

        // Determine overall status
        var overallStatus = checks.Any(c => c.Status == "Unhealthy")
            ? "Unhealthy"
            : checks.Any(c => c.Status == "Degraded")
                ? "Degraded"
                : "Healthy";

        var response = new HealthCheckResponse(
            Status: overallStatus,
            TotalDurationMs: stopwatch.Elapsed.TotalMilliseconds,
            Checks: checks
        );

        if (overallStatus == "Unhealthy")
        {
            _logger.LogWarning("Health check failed: {Status}", overallStatus);
        }

        return Result.Success(response);
    }

    private HealthCheckEntry CheckApi()
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Check memory usage
            var allocatedMemoryMb = GC.GetTotalMemory(false) / 1024 / 1024;
            const long maxMemoryMb = 1024; // 1 GB

            stopwatch.Stop();

            if (allocatedMemoryMb >= maxMemoryMb)
            {
                return new HealthCheckEntry(
                    Name: "api",
                    Status: "Unhealthy",
                    DurationMs: stopwatch.Elapsed.TotalMilliseconds,
                    Description: null,
                    Error: $"Excessive memory: {allocatedMemoryMb} MB"
                );
            }

            return new HealthCheckEntry(
                Name: "api",
                Status: "Healthy",
                DurationMs: stopwatch.Elapsed.TotalMilliseconds,
                Description: $"Memory: {allocatedMemoryMb} MB"
            );
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error during API health check");

            return new HealthCheckEntry(
                Name: "api",
                Status: "Unhealthy",
                DurationMs: stopwatch.Elapsed.TotalMilliseconds,
                Error: ex.Message
            );
        }
    }

    private async Task<HealthCheckEntry> CheckDatabaseAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Connection test
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);

            if (!canConnect)
            {
                stopwatch.Stop();
                return new HealthCheckEntry(
                    Name: "database",
                    Status: "Unhealthy",
                    DurationMs: stopwatch.Elapsed.TotalMilliseconds,
                    Error: "Unable to connect to database"
                );
            }

            // Read test (optional, but recommended)
            await _dbContext.Database.ExecuteSqlRawAsync(
                "SELECT 1",
                cancellationToken);

            stopwatch.Stop();

            return new HealthCheckEntry(
                Name: "database",
                Status: "Healthy",
                DurationMs: stopwatch.Elapsed.TotalMilliseconds,
                Description: "Connection and read OK"
            );
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error during database health check");

            return new HealthCheckEntry(
                Name: "database",
                Status: "Unhealthy",
                DurationMs: stopwatch.Elapsed.TotalMilliseconds,
                Error: ex.Message
            );
        }
    }
}