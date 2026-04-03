using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api.CZ.Features.InformationPages.DTOs;
using api.CZ.Features.InformationPages.Services;

namespace api.CZ.Features.InformationPages;

[ApiController]
[Route("api/information-pages")]
[Authorize(Roles = "Administrator")]
public class InformationPageController : ControllerBase
{
    private readonly IInformationPageService _service;
    private readonly ILogger<InformationPageController> _logger;

    public InformationPageController(IInformationPageService service, ILogger<InformationPageController> logger)
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
        var pages = await _service.GetAllAsync();
        return Ok(pages);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var page = await _service.GetByIdAsync(id);

        if (page == null)
            return NotFound(new { error = "Information page not found" });

        return Ok(page);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInformationPageDto dto)
    {
        var adminId = GetAdminId();
        var page = await _service.CreateAsync(dto, adminId);

        if (page == null)
            return BadRequest(new { error = "Failed to create information page" });

        return CreatedAtAction(nameof(GetById), new { id = page.Id }, page);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInformationPageDto dto)
    {
        var adminId = GetAdminId();
        var page = await _service.UpdateAsync(id, dto, adminId);

        if (page == null)
            return NotFound(new { error = "Information page not found" });

        return Ok(page);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var adminId = GetAdminId();
        var deleted = await _service.DeleteAsync(id, adminId);

        if (!deleted)
            return NotFound(new { error = "Information page not found" });

        return Ok(new { message = "Information page deleted successfully" });
    }
}
