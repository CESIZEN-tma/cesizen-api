using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api.CZ.Features.InformationTags.DTOs;
using api.CZ.Features.InformationTags.Services;

namespace api.CZ.Features.InformationTags;

[ApiController]
[Route("api/information-tags")]
[Authorize(Roles = "Administrator")]
public class InformationTagController : ControllerBase
{
    private readonly IInformationTagService _service;
    private readonly ILogger<InformationTagController> _logger;

    public InformationTagController(IInformationTagService service, ILogger<InformationTagController> logger)
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
        var tags = await _service.GetAllAsync();
        return Ok(tags);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tag = await _service.GetByIdAsync(id);

        if (tag == null)
            return NotFound(new { error = "Information tag not found" });

        return Ok(tag);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInformationTagDto dto)
    {
        var adminId = GetAdminId();
        var tag = await _service.CreateAsync(dto, adminId);

        if (tag == null)
            return BadRequest(new { error = "Failed to create information tag" });

        return CreatedAtAction(nameof(GetById), new { id = tag.Id }, tag);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInformationTagDto dto)
    {
        var adminId = GetAdminId();
        var tag = await _service.UpdateAsync(id, dto, adminId);

        if (tag == null)
            return NotFound(new { error = "Information tag not found" });

        return Ok(tag);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var adminId = GetAdminId();
        var deleted = await _service.DeleteAsync(id, adminId);

        if (!deleted)
            return NotFound(new { error = "Information tag not found" });

        return Ok(new { message = "Information tag deleted successfully" });
    }
}
