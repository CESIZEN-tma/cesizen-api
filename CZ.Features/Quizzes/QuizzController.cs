using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api.CZ.Features.Quizzes.DTOs;
using api.CZ.Features.Quizzes.Services;

namespace api.CZ.Features.Quizzes;

[ApiController]
[Route("api/quizzes")]
public class QuizzController : ControllerBase
{
    private readonly IQuizzService _service;
    private readonly ILogger<QuizzController> _logger;

    public QuizzController(IQuizzService service, ILogger<QuizzController> logger)
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
        var quizzes = await _service.GetAllAsync();
        return Ok(quizzes);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var quizz = await _service.GetByIdAsync(id);

        if (quizz == null)
            return NotFound(new { error = "Quiz not found" });

        return Ok(quizz);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Create([FromBody] CreateQuizzDto dto)
    {
        var adminId = GetAdminId();
        var quizz = await _service.CreateAsync(dto, adminId);
        return CreatedAtAction(nameof(GetById), new { id = quizz.Id }, quizz);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateQuizzDto dto)
    {
        var adminId = GetAdminId();
        var quizz = await _service.UpdateAsync(id, dto, adminId);

        if (quizz == null)
            return NotFound(new { error = "Quiz not found" });

        return Ok(quizz);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var adminId = GetAdminId();
        var deleted = await _service.DeleteAsync(id, adminId);

        if (!deleted)
            return NotFound(new { error = "Quiz not found" });

        return Ok(new { message = "Quiz deleted successfully" });
    }
}
