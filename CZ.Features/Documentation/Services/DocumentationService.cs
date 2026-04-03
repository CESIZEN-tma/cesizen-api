using System.Text.RegularExpressions;
using Markdig;
using Microsoft.AspNetCore.Hosting;

namespace api.CZ.Features.Documentation.Services;

public class DocumentationService : IDocumentationService
{
    private readonly IWebHostEnvironment _env;
    private readonly MarkdownPipeline _pipeline;

    public DocumentationService(IWebHostEnvironment env)
    {
        _env = env;
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();
    }

    public async Task<string?> GetDocumentationAsHtmlAsync(string[] pathSegments)
    {
        var docRoot = Path.Combine(_env.ContentRootPath, "DOCUMENTATION");
        string filePath;
        string[] basePath;

        // Try to find README.md in folder first
        filePath = Path.Combine(docRoot, Path.Combine(pathSegments), "README.md");

        if (!File.Exists(filePath) && pathSegments.Length > 0)
        {
            // Try to find specific .md file in parent folder
            var parentSegments = pathSegments.Take(pathSegments.Length - 1).ToArray();
            var documentName = pathSegments.Last();
            filePath = Path.Combine(docRoot, Path.Combine(parentSegments), $"{documentName}.md");
            basePath = parentSegments;
        }
        else
        {
            basePath = pathSegments;
        }

        if (!File.Exists(filePath))
            return null;

        var markdown = await File.ReadAllTextAsync(filePath);
        var html = Markdown.ToHtml(markdown, _pipeline);

        // Rewrite markdown backlinks (*.md) to API routes
        html = RewriteLinks(html, basePath);

        // Wrap in styled HTML
        html = WrapWithStyles(html);

        return html;
    }

    private static string RewriteLinks(string html, string[] pathSegments)
    {
        var basePath = string.Join("/", pathSegments.Select(s => s.ToLowerInvariant()));

        // Convert markdown links to API routes
        return Regex.Replace(
            html,
            "href=\"([^\"]+)\\.md\"",
            match =>
            {
                var linkPath = match.Groups[1].Value;

                // Start with current path segments
                var segments = new List<string>(pathSegments.Select(s => s.ToLowerInvariant()));

                // Remove leading "./" if present
                if (linkPath.StartsWith("./"))
                {
                    linkPath = linkPath.Substring(2);
                }

                // Process the link path
                var linkParts = linkPath.Split('/', StringSplitOptions.RemoveEmptyEntries);

                foreach (var part in linkParts)
                {
                    if (part == "..")
                    {
                        // Go up one level
                        if (segments.Count > 0)
                            segments.RemoveAt(segments.Count - 1);
                    }
                    else if (!string.IsNullOrEmpty(part))
                    {
                        // Add to path
                        segments.Add(part.ToLowerInvariant());
                    }
                }

                // Remove "readme" from the end if present, since the service auto-finds README.md
                if (segments.Count > 0 && segments[segments.Count - 1] == "readme")
                {
                    segments.RemoveAt(segments.Count - 1);
                }

                // Build final URL
                var finalPath = string.Join("/", segments);
                return string.IsNullOrEmpty(finalPath)
                    ? "href=\"/api/public/docs\""
                    : $"href=\"/api/public/docs/{finalPath}\"";
            },
            RegexOptions.IgnoreCase
        );
    }

    private static string WrapWithStyles(string content)
    {
        return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>CesiZen Documentation</title>
    <style>
        {GetMarkdownStyles()}
    </style>
</head>
<body>
    <header class=""doc-header"">
        <div class=""doc-header-content"">
            <h1 class=""doc-title"">CesiZen Documentation</h1>
            <a href=""/api/public/docs"" class=""home-button"">Home</a>
        </div>
    </header>
    <div class=""markdown-body"">
        {content}
    </div>
</body>
</html>";
    }

    private static string GetMarkdownStyles()
    {
        return @"
body {
    margin: 0;
    padding: 0;
    background-color: #f6f8fa;
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Noto Sans', Helvetica, Arial, sans-serif;
    font-size: 16px;
    line-height: 1.6;
    color: #24292f;
}

.doc-header {
    background-color: #24292f;
    color: #ffffff;
    padding: 16px 0;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    position: sticky;
    top: 0;
    z-index: 1000;
}

.doc-header-content {
    max-width: 980px;
    margin: 0 auto;
    padding: 0 45px;
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.doc-title {
    margin: 0;
    font-size: 1.5em;
    font-weight: 600;
    color: #ffffff;
}

.home-button {
    padding: 8px 16px;
    background-color: #0969da;
    color: #ffffff;
    text-decoration: none;
    border-radius: 6px;
    font-weight: 500;
    font-size: 14px;
    transition: background-color 0.2s ease;
}

.home-button:hover {
    background-color: #0860ca;
    text-decoration: none;
}

.markdown-body {
    max-width: 980px;
    margin: 20px auto;
    padding: 45px;
    background-color: #ffffff;
    border-radius: 6px;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.markdown-body h1,
.markdown-body h2,
.markdown-body h3,
.markdown-body h4,
.markdown-body h5,
.markdown-body h6 {
    margin-top: 24px;
    margin-bottom: 16px;
    font-weight: 600;
    line-height: 1.25;
    color: #24292f;
}

.markdown-body h1 {
    font-size: 2em;
    padding-bottom: 0.3em;
    border-bottom: 1px solid #d0d7de;
}

.markdown-body h2 {
    font-size: 1.5em;
    padding-bottom: 0.3em;
    border-bottom: 1px solid #d0d7de;
}

.markdown-body h3 {
    font-size: 1.25em;
}

.markdown-body h4 {
    font-size: 1em;
}

.markdown-body h5 {
    font-size: 0.875em;
}

.markdown-body h6 {
    font-size: 0.85em;
    color: #57606a;
}

.markdown-body p {
    margin-top: 0;
    margin-bottom: 16px;
}

.markdown-body a {
    color: #0969da;
    text-decoration: none;
}

.markdown-body a:hover {
    text-decoration: underline;
}

.markdown-body code {
    padding: 0.2em 0.4em;
    margin: 0;
    font-size: 85%;
    background-color: #f6f8fa;
    border-radius: 6px;
    font-family: ui-monospace, SFMono-Regular, 'SF Mono', Menlo, Consolas, 'Liberation Mono', monospace;
}

.markdown-body pre {
    padding: 16px;
    overflow: auto;
    font-size: 85%;
    line-height: 1.45;
    background-color: #f6f8fa;
    border-radius: 6px;
    margin-top: 0;
    margin-bottom: 16px;
}

.markdown-body pre code {
    display: inline;
    max-width: auto;
    padding: 0;
    margin: 0;
    overflow: visible;
    line-height: inherit;
    background-color: transparent;
    border: 0;
}

.markdown-body blockquote {
    padding: 0 1em;
    color: #57606a;
    border-left: 0.25em solid #d0d7de;
    margin-top: 0;
    margin-bottom: 16px;
}

.markdown-body ul,
.markdown-body ol {
    margin-top: 0;
    margin-bottom: 16px;
    padding-left: 2em;
}

.markdown-body li {
    margin-top: 0.25em;
}

.markdown-body table {
    border-spacing: 0;
    border-collapse: collapse;
    margin-top: 0;
    margin-bottom: 16px;
    width: 100%;
    overflow: auto;
}

.markdown-body table th {
    font-weight: 600;
    padding: 6px 13px;
    border: 1px solid #d0d7de;
    background-color: #f6f8fa;
}

.markdown-body table td {
    padding: 6px 13px;
    border: 1px solid #d0d7de;
}

.markdown-body table tr {
    background-color: #ffffff;
    border-top: 1px solid #d0d7de;
}

.markdown-body table tr:nth-child(2n) {
    background-color: #f6f8fa;
}

.markdown-body img {
    max-width: 100%;
    height: auto;
    box-sizing: content-box;
    background-color: #ffffff;
}

.markdown-body hr {
    height: 0.25em;
    padding: 0;
    margin: 24px 0;
    background-color: #d0d7de;
    border: 0;
}

.markdown-body strong {
    font-weight: 600;
}

.markdown-body em {
    font-style: italic;
}

@media (max-width: 767px) {
    .markdown-body {
        padding: 15px;
        margin: 10px;
    }

    .doc-header-content {
        padding: 0 15px;
    }

    .doc-title {
        font-size: 1.2em;
    }

    .home-button {
        padding: 6px 12px;
        font-size: 13px;
    }
}
";
    }
}