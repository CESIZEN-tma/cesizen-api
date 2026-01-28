using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api.CZ.Features.Bookmarks.DTOs;
using api.CZ.Features.Bookmarks.Services;

namespace api.CZ.Features.Bookmarks;

[ApiController]
[Route("api/bookmarks")]
[Authorize]
public class BookmarkController : ControllerBase
{
    private readonly IBookmarkService _service;
    private readonly ILogger<BookmarkController> _logger;

    public BookmarkController(IBookmarkService service, ILogger<BookmarkController> logger)
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
    public async Task<IActionResult> GetUserBookmarks()
    {
        var userId = GetUserId();
        var bookmarks = await _service.GetUserBookmarksAsync(userId);
        return Ok(bookmarks);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBookmark([FromBody] CreateBookmarkDto dto)
    {
        var userId = GetUserId();

        try
        {
            var bookmark = await _service.CreateBookmarkAsync(userId, dto);
            return Created($"/api/bookmarks/{bookmark.ConfigurationId}", bookmark);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpDelete("{configurationId:guid}")]
    public async Task<IActionResult> DeleteBookmark(Guid configurationId)
    {
        var userId = GetUserId();
        var deleted = await _service.DeleteBookmarkAsync(userId, configurationId);

        if (!deleted)
            return NotFound(new { error = "Bookmark not found" });

        return Ok(new { message = "Bookmark deleted successfully" });
    }
}
