using api.CZ.Features.Documentation.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.CZ.Features.Documentation;

[ApiController]
[Route("api/public/docs")]
public class DocumentationController : ControllerBase
{
    private readonly IDocumentationService _documentationService;

    public DocumentationController(IDocumentationService documentationService)
    {
        _documentationService = documentationService;
    }

    [HttpGet("{*path}")]
    public async Task<IActionResult> GetDocumentation(string? path = null)
    {
        // Default to root README if no path provided
        var pathSegments = string.IsNullOrEmpty(path)
            ? Array.Empty<string>()  // Empty array = root README.md
            : path.Split('/', StringSplitOptions.RemoveEmptyEntries)
                  .Select(p => p.ToUpperInvariant())
                  .ToArray();

        var html = await _documentationService.GetDocumentationAsHtmlAsync(pathSegments);

        if (html == null)
            return NotFound();

        return Content(html, "text/html");
    }
}