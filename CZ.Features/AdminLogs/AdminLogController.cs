using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api.CZ.Features.AdminLogs.DTOs;
using api.CZ.Features.AdminLogs.Services;

namespace api.CZ.Features.AdminLogs;

[ApiController]
[Route("api/admin/logs")]
[Authorize(Roles = "Administrator")]
public class AdminLogController : ControllerBase
{
    private readonly IAdminLogService _service;
    private readonly ILogger<AdminLogController> _logger;

    public AdminLogController(IAdminLogService service, ILogger<AdminLogController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetFilteredLogs([FromQuery] AdminLogFilterDto filter)
    {
        var logs = await _service.GetFilteredLogsAsync(filter);
        return Ok(logs);
    }

    [HttpGet("recent")]
    public async Task<IActionResult> GetRecentLogs([FromQuery] int count = 50)
    {
        var logs = await _service.GetRecentLogsAsync(count);
        return Ok(logs);
    }

    [HttpGet("administrator/{id:guid}")]
    public async Task<IActionResult> GetLogsByAdministrator(Guid id)
    {
        var logs = await _service.GetLogsByAdministratorAsync(id);
        return Ok(logs);
    }

    [HttpGet("entity/{type}/{id:guid}")]
    public async Task<IActionResult> GetLogsByEntity(string type, Guid id)
    {
        var logs = await _service.GetLogsByEntityAsync(type, id);
        return Ok(logs);
    }

    [HttpGet("lineage/{entityType}/{entityId:guid}")]
    public async Task<IActionResult> GetEntityLineage(string entityType, Guid entityId)
    {
        var lineage = await _service.GetEntityLineageAsync(entityType, entityId);
        return Ok(lineage);
    }
}
