using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api.CZ.Features.UserSavedConfigurations.DTOs;
using api.CZ.Features.UserSavedConfigurations.Services;

namespace api.CZ.Features.UserSavedConfigurations;

[ApiController]
[Route("api/user-saved-configurations")]
[Authorize]
public class UserSavedConfigurationController : ControllerBase
{
    private readonly IUserSavedConfigurationService _service;
    private readonly ILogger<UserSavedConfigurationController> _logger;

    public UserSavedConfigurationController(IUserSavedConfigurationService service, ILogger<UserSavedConfigurationController> logger)
    {
        _service = service;
        _logger = logger;
    }

    private Guid GetUserId()
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
            return NotFound(new { error = "User saved configuration not found" });

        return Ok(configuration);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserSavedConfigurationDto dto)
    {
        var configuration = await _service.CreateAsync(dto);

        if (configuration == null)
            return BadRequest(new { error = "Failed to create user saved configuration" });

        return CreatedAtAction(nameof(GetById), new { id = configuration.Id }, configuration);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserSavedConfigurationDto dto)
    {
        var configuration = await _service.UpdateAsync(id, dto);

        if (configuration == null)
            return NotFound(new { error = "User saved configuration not found" });

        return Ok(configuration);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
            return NotFound(new { error = "User saved configuration not found" });

        return Ok(new { message = "User saved configuration deleted successfully" });
    }

    [HttpPost("from-quiz")]
    public async Task<IActionResult> CreateFromQuizResponses([FromBody] QuizResponsesDto dto)
    {
        var userId = GetUserId();

        try
        {
            var configuration = await _service.CreateFromQuizResponsesAsync(userId, dto);

            if (configuration == null)
                return BadRequest(new { error = "Failed to generate configuration from quiz responses" });

            return CreatedAtAction(nameof(GetById), new { id = configuration.Id }, configuration);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
