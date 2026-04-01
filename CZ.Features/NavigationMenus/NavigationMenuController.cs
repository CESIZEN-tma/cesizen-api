using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api.CZ.Features.NavigationMenus.DTOs;
using api.CZ.Features.NavigationMenus.Services;

namespace api.CZ.Features.NavigationMenus;

[ApiController]
[Route("api/navigation-menus")]
[Authorize(Roles = "Administrator")]
public class NavigationMenuController : ControllerBase
{
    private readonly INavigationMenuService _service;
    private readonly ILogger<NavigationMenuController> _logger;

    public NavigationMenuController(INavigationMenuService service, ILogger<NavigationMenuController> logger)
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
        var menus = await _service.GetAllAsync();
        return Ok(menus);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var menu = await _service.GetByIdAsync(id);

        if (menu == null)
            return NotFound(new { error = "Navigation menu not found" });

        return Ok(menu);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNavigationMenuDto dto)
    {
        var adminId = GetAdminId();
        var menu = await _service.CreateAsync(dto, adminId);

        if (menu == null)
            return BadRequest(new { error = "Failed to create navigation menu" });

        return CreatedAtAction(nameof(GetById), new { id = menu.Id }, menu);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateNavigationMenuDto dto)
    {
        var adminId = GetAdminId();
        var menu = await _service.UpdateAsync(id, dto, adminId);

        if (menu == null)
            return NotFound(new { error = "Navigation menu not found" });

        return Ok(menu);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var adminId = GetAdminId();
        var deleted = await _service.DeleteAsync(id, adminId);

        if (!deleted)
            return NotFound(new { error = "Navigation menu not found" });

        return Ok(new { message = "Navigation menu deleted successfully" });
    }

    [HttpPut("positions")]
    public async Task<IActionResult> UpdatePositions([FromBody] List<UpdateMenuPositionDto> positions)
    {
        var adminId = GetAdminId();
        await _service.UpdatePositionsAsync(positions, adminId);
        return Ok(new { message = "Positions updated successfully" });
    }
}
