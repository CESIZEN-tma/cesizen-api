using System.Text.RegularExpressions;
using Markdig;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.CZ.Features.Documentation;

[ApiController]
[Route("api/public/docs/integration/authentification")]
public class DocumentationController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly MarkdownPipeline _pipeline;

    public DocumentationController(IWebHostEnvironment env)
    {
        _env = env;

        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();
    }

    [HttpGet]
    public async Task<IActionResult> GetReadme()
    {
        var filePath = Path.Combine(
            _env.ContentRootPath,
            "DOCUMENTATION",
            "INTEGRATION",
            "AUTHENTIFICATION",
            "README.md"
        );

        if (!System.IO.File.Exists(filePath))
            return NotFound();

        var markdown = await System.IO.File.ReadAllTextAsync(filePath);

        var html = Markdown.ToHtml(markdown, _pipeline);

        // Rewrite markdown backlinks (*.md) to API routes
        html = RewriteLinks(html);

        return Content(html, "text/html");
    }

    private static string RewriteLinks(string html)
    {
        // README.md, guide.md, faq.md → /api/public/docs/integration/authentification/{name}
        return Regex.Replace(
            html,
            "href=\"([^\"]+)\\.md\"",
            match =>
            {
                var docName = match.Groups[1].Value
                    .Split('/')
                    .Last();

                return $"href=\"/api/public/docs/integration/authentification/{docName}\"";
            },
            RegexOptions.IgnoreCase
        );
    }
}