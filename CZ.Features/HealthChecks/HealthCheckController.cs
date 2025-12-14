

using api.CZ.Features.HealthChecks.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.CZ.Features.HealthChecks;


[ApiController]
[Route("/public")]
public class HealthCheckController : ControllerBase
{
    private readonly IHealthCheckService _healthCheckService;
    private readonly ILogger<HealthCheckController> _logger;

    public HealthCheckController(
        IHealthCheckService healthCheckService,
        ILogger<HealthCheckController> logger)
    {
        _healthCheckService = healthCheckService;
        _logger = logger;
    }

    /// <summary>
    /// Check the health of the API and the database connection
    /// </summary>
    /// <remarks>
    /// This endpoint is only accessible to development environnment
    /// </remarks>
    [HttpGet("/public/health-check")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckHealth(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Health check asked from {IpAddress}", 
            HttpContext.Connection.RemoteIpAddress);

        var result = await _healthCheckService.CheckHealthAsync(cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable, 
                new 
                { 
                    message = result.Error,
                    status = "Unhealthy" 
                });
        }

        // Return 503 if unhealthy, else 200
        if (result.Value.Status == "Unhealthy")
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable, 
                result.Value);
        }

        return Ok(result.Value);
    }
}