using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api.CZ.Features.PasswordsInfos.DTOs;
using api.CZ.Features.PasswordsInfos.Services;

namespace api.CZ.Features.PasswordsInfos;

[ApiController]
[Route("api/passwords-infos")]
[Authorize]
public class PasswordsInfoController : ControllerBase
{
    private readonly IPasswordsInfoService _service;
    private readonly ILogger<PasswordsInfoController> _logger;

    public PasswordsInfoController(IPasswordsInfoService service, ILogger<PasswordsInfoController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var infos = await _service.GetAllAsync();
        return Ok(infos);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var info = await _service.GetByIdAsync(id);

        if (info == null)
            return NotFound(new { error = "Password info not found" });

        return Ok(info);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePasswordsInfoDto dto)
    {
        var info = await _service.CreateAsync(dto);

        if (info == null)
            return BadRequest(new { error = "Failed to create password info" });

        return CreatedAtAction(nameof(GetById), new { id = info.Id }, info);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePasswordsInfoDto dto)
    {
        var info = await _service.UpdateAsync(id, dto);

        if (info == null)
            return NotFound(new { error = "Password info not found" });

        return Ok(info);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
            return NotFound(new { error = "Password info not found" });

        return Ok(new { message = "Password info deleted successfully" });
    }
}
