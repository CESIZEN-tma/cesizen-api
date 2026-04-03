using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api.CZ.Features.Administrators.DTOs;
using api.CZ.Features.Administrators.Services;

namespace api.CZ.Features.Administrators;

[ApiController]
[Route("api/administrators")]
[Authorize(Roles = "Administrator")]
public class AdministratorController : ControllerBase
{
    private readonly IAdministratorService _service;
    private readonly ILogger<AdministratorController> _logger;

    public AdministratorController(IAdministratorService service, ILogger<AdministratorController> logger)
    {
        _service = service;
        _logger = logger;
    }

    private Guid GetAdminId()
    {
        var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(adminIdClaim!);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var admins = await _service.GetAllAsync();
        return Ok(admins);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var admin = await _service.GetByIdAsync(id);

        if (admin == null)
            return NotFound(new { error = "Administrator not found" });

        return Ok(admin);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAdministratorDto dto)
    {
        var creatorAdminId = GetAdminId();
        var admin = await _service.CreateAsync(dto, creatorAdminId);

        if (admin == null)
            return BadRequest(new { error = "Failed to create administrator. Email may already exist." });

        return CreatedAtAction(nameof(GetById), new { id = admin.Id }, admin);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAdministratorDto dto)
    {
        var adminId = GetAdminId();
        var admin = await _service.UpdateAsync(id, dto, adminId);

        if (admin == null)
            return NotFound(new { error = "Administrator not found" });

        return Ok(admin);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var adminId = GetAdminId();

        try
        {
            var deleted = await _service.DeleteAsync(id, adminId);

            if (!deleted)
                return NotFound(new { error = "Administrator not found" });

            return Ok(new { message = "Administrator deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
