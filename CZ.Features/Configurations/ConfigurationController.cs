using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api.CZ.Features.Configurations.DTOs;
using api.CZ.Features.Configurations.Services;

namespace api.CZ.Features.Configurations;

[ApiController]
[Route("api/configurations")]
[Authorize(Roles = "Administrator")]
public class ConfigurationController : ControllerBase
{
    private readonly IConfigurationService _service;
    private readonly ILogger<ConfigurationController> _logger;

    public ConfigurationController(IConfigurationService service, ILogger<ConfigurationController> logger)
    {
        _service = service;
        _logger = logger;
    }

    private Guid GetAdminId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var configurations = await _service.GetAllAsync();
        return Ok(configurations);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var configuration = await _service.GetByIdAsync(id);

        if (configuration == null)
            return NotFound(new { error = "Configuration not found" });

        return Ok(configuration);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateConfigurationDto dto)
    {
        var adminId = GetAdminId();
        var configuration = await _service.CreateAsync(dto, adminId);

        if (configuration == null)
            return BadRequest(new { error = "Failed to create configuration" });

        return CreatedAtAction(nameof(GetById), new { id = configuration.Id }, configuration);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateConfigurationDto dto)
    {
        var adminId = GetAdminId();
        var configuration = await _service.UpdateAsync(id, dto, adminId);

        if (configuration == null)
            return NotFound(new { error = "Configuration not found" });

        return Ok(configuration);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var adminId = GetAdminId();
        var deleted = await _service.DeleteAsync(id, adminId);

        if (!deleted)
            return NotFound(new { error = "Configuration not found" });

        return Ok(new { message = "Configuration deleted successfully" });
    }
}
