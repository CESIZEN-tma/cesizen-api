using System.Reflection;
using api.CZ.Features.Documentation.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Moq;

namespace api.Tests.Unit.Features.Documentation;

public class DocumentationServiceTests
{
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    private readonly DocumentationService _service;
    private readonly string _testDocRoot;

    public DocumentationServiceTests()
    {
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _testDocRoot = Path.Combine(Path.GetTempPath(), "CesiZenDocsTest", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDocRoot);
        _mockEnvironment.Setup(e => e.ContentRootPath).Returns(Path.GetDirectoryName(_testDocRoot)!);

        _service = new DocumentationService(_mockEnvironment.Object);
    }

    #region Link Rewriting Tests

    [Fact]
    public void RewriteLinks_ShouldConvertSimpleRelativeLinkFromRoot()
    {
        // Arrange
        var html = @"<a href=""./INTEGRATION/README.md"">Integration</a>";
        var pathSegments = Array.Empty<string>();

        // Act
        var result = InvokeRewriteLinks(html, pathSegments);

        // Assert
        result.Should().Contain(@"href=""/api/public/docs/integration""");
    }

    [Fact]
    public void RewriteLinks_ShouldConvertMultiLevelRelativeLink()
    {
        // Arrange
        var html = @"<a href=""./INTEGRATION/USER_JOURNEYS/README.md"">User Journeys</a>";
        var pathSegments = Array.Empty<string>();

        // Act
        var result = InvokeRewriteLinks(html, pathSegments);

        // Assert
        result.Should().Contain(@"href=""/api/public/docs/integration/user_journeys""");
    }

    [Fact]
    public void RewriteLinks_ShouldConvertSpecificDocumentLink()
    {
        // Arrange
        var html = @"<a href=""./INTEGRATION/USER_JOURNEYS/NEW_USER_PATH.md"">New User</a>";
        var pathSegments = Array.Empty<string>();

        // Act
        var result = InvokeRewriteLinks(html, pathSegments);

        // Assert
        result.Should().Contain(@"href=""/api/public/docs/integration/user_journeys/new_user_path""");
    }

    [Fact]
    public void RewriteLinks_ShouldHandleParentDirectoryNavigation()
    {
        // Arrange
        var html = @"<a href=""../README.md"">Back to Integration</a>";
        var pathSegments = new[] { "INTEGRATION", "USER_JOURNEYS" };

        // Act
        var result = InvokeRewriteLinks(html, pathSegments);

        // Assert
        result.Should().Contain(@"href=""/api/public/docs/integration""");
    }

    [Fact]
    public void RewriteLinks_ShouldHandleMultipleParentDirectories()
    {
        // Arrange
        var html = @"<a href=""../../README.md"">Back to Root</a>";
        var pathSegments = new[] { "INTEGRATION", "USER_JOURNEYS" };

        // Act
        var result = InvokeRewriteLinks(html, pathSegments);

        // Assert
        result.Should().Contain(@"href=""/api/public/docs""");
    }

    [Fact]
    public void RewriteLinks_ShouldHandleParentThenDown()
    {
        // Arrange
        var html = @"<a href=""../AUTHENTIFICATION/README.md"">Authentication</a>";
        var pathSegments = new[] { "INTEGRATION", "USER_JOURNEYS" };

        // Act
        var result = InvokeRewriteLinks(html, pathSegments);

        // Assert
        result.Should().Contain(@"href=""/api/public/docs/integration/authentification""");
    }

    [Fact]
    public void RewriteLinks_ShouldRemoveReadmeSuffix()
    {
        // Arrange
        var html = @"<a href=""./FOLDER/README.md"">Folder</a>";
        var pathSegments = Array.Empty<string>();

        // Act
        var result = InvokeRewriteLinks(html, pathSegments);

        // Assert
        result.Should().Contain(@"href=""/api/public/docs/folder""");
        result.Should().NotContain("readme");
    }

    [Fact]
    public void RewriteLinks_ShouldConvertToLowercase()
    {
        // Arrange
        var html = @"<a href=""./INTEGRATION/USER_JOURNEYS/NEW_USER_PATH.md"">Path</a>";
        var pathSegments = Array.Empty<string>();

        // Act
        var result = InvokeRewriteLinks(html, pathSegments);

        // Assert
        result.Should().Contain(@"href=""/api/public/docs/integration/user_journeys/new_user_path""");
        result.Should().NotContain("INTEGRATION");
        result.Should().NotContain("USER_JOURNEYS");
    }

    [Fact]
    public void RewriteLinks_ShouldHandleSameLevelLink()
    {
        // Arrange
        var html = @"<a href=""./NEW_USER_PATH.md"">New User</a>";
        var pathSegments = new[] { "INTEGRATION", "USER_JOURNEYS" };

        // Act
        var result = InvokeRewriteLinks(html, pathSegments);

        // Assert
        result.Should().Contain(@"href=""/api/public/docs/integration/user_journeys/new_user_path""");
    }

    [Fact]
    public void RewriteLinks_ShouldHandleMultipleLinks()
    {
        // Arrange
        var html = @"
            <a href=""./INTEGRATION/README.md"">Integration</a>
            <a href=""./INTEGRATION/USER_JOURNEYS/README.md"">User Journeys</a>
            <a href=""./INTEGRATION/AUTHENTIFICATION/README.md"">Auth</a>
        ";
        var pathSegments = Array.Empty<string>();

        // Act
        var result = InvokeRewriteLinks(html, pathSegments);

        // Assert
        result.Should().Contain(@"href=""/api/public/docs/integration""");
        result.Should().Contain(@"href=""/api/public/docs/integration/user_journeys""");
        result.Should().Contain(@"href=""/api/public/docs/integration/authentification""");
    }

    [Fact]
    public void RewriteLinks_ShouldPreserveNonMarkdownLinks()
    {
        // Arrange
        var html = @"<a href=""https://example.com"">External</a>";
        var pathSegments = Array.Empty<string>();

        // Act
        var result = InvokeRewriteLinks(html, pathSegments);

        // Assert
        result.Should().Contain(@"href=""https://example.com""");
    }

    [Fact]
    public void RewriteLinks_ShouldHandleEmptyBasePath()
    {
        // Arrange
        var html = @"<a href=""./README.md"">Root</a>";
        var pathSegments = Array.Empty<string>();

        // Act
        var result = InvokeRewriteLinks(html, pathSegments);

        // Assert
        result.Should().Contain(@"href=""/api/public/docs""");
    }

    [Fact]
    public void RewriteLinks_ShouldHandleComplexRelativePath()
    {
        // Arrange
        var html = @"<a href=""../../FEATURES/QUIZZES/README.md"">Quizzes</a>";
        var pathSegments = new[] { "INTEGRATION", "USER_JOURNEYS" };

        // Act
        var result = InvokeRewriteLinks(html, pathSegments);

        // Assert
        result.Should().Contain(@"href=""/api/public/docs/features/quizzes""");
    }

    [Theory]
    [InlineData("./INTEGRATION/README.md", new string[] { }, "/api/public/docs/integration")]
    [InlineData("./USER_JOURNEYS/NEW_USER_PATH.md", new[] { "INTEGRATION" }, "/api/public/docs/integration/user_journeys/new_user_path")]
    [InlineData("../README.md", new[] { "INTEGRATION", "USER_JOURNEYS" }, "/api/public/docs/integration")]
    [InlineData("../../README.md", new[] { "INTEGRATION", "USER_JOURNEYS" }, "/api/public/docs")]
    [InlineData("../AUTHENTIFICATION/LOGIN.md", new[] { "INTEGRATION", "USER_JOURNEYS" }, "/api/public/docs/integration/authentification/login")]
    public void RewriteLinks_ShouldConvertVariousPathFormats(string markdownLink, string[] pathSegments, string expectedUrl)
    {
        // Arrange
        var html = $@"<a href=""{markdownLink}"">Link</a>";

        // Act
        var result = InvokeRewriteLinks(html, pathSegments);

        // Assert
        result.Should().Contain($@"href=""{expectedUrl}""");
    }

    #endregion

    #region File Handling Tests

    [Fact]
    public async Task GetDocumentationAsHtmlAsync_ShouldReturnNull_WhenFileNotFound()
    {
        // Arrange
        var pathSegments = new[] { "NONEXISTENT", "FILE" };

        // Act
        var result = await _service.GetDocumentationAsHtmlAsync(pathSegments);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetDocumentationAsHtmlAsync_ShouldFindReadmeInFolder()
    {
        // Arrange
        var folderPath = Path.Combine(_testDocRoot, "DOCUMENTATION", "TEST_FOLDER");
        Directory.CreateDirectory(folderPath);
        var readmePath = Path.Combine(folderPath, "README.md");
        await File.WriteAllTextAsync(readmePath, "# Test Readme");

        var pathSegments = new[] { "TEST_FOLDER" };

        // Act
        var result = await _service.GetDocumentationAsHtmlAsync(pathSegments);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("Test Readme");
    }

    [Fact]
    public async Task GetDocumentationAsHtmlAsync_ShouldFindSpecificMarkdownFile()
    {
        // Arrange
        var folderPath = Path.Combine(_testDocRoot, "DOCUMENTATION", "TEST_FOLDER");
        Directory.CreateDirectory(folderPath);
        var filePath = Path.Combine(folderPath, "SPECIFIC_DOC.md");
        await File.WriteAllTextAsync(filePath, "# Specific Document");

        var pathSegments = new[] { "TEST_FOLDER", "SPECIFIC_DOC" };

        // Act
        var result = await _service.GetDocumentationAsHtmlAsync(pathSegments);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("Specific Document");
    }

    [Fact]
    public async Task GetDocumentationAsHtmlAsync_ShouldConvertMarkdownToHtml()
    {
        // Arrange
        var folderPath = Path.Combine(_testDocRoot, "DOCUMENTATION", "TEST_FOLDER");
        Directory.CreateDirectory(folderPath);
        var readmePath = Path.Combine(folderPath, "README.md");
        await File.WriteAllTextAsync(readmePath, @"
# Heading 1
## Heading 2

This is **bold** and *italic*.

- List item 1
- List item 2
");

        var pathSegments = new[] { "TEST_FOLDER" };

        // Act
        var result = await _service.GetDocumentationAsHtmlAsync(pathSegments);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("<h1");
        result.Should().Contain("<h2");
        result.Should().Contain("<strong>bold</strong>");
        result.Should().Contain("<em>italic</em>");
        result.Should().Contain("<ul>");
        result.Should().Contain("<li>");
    }

    [Fact]
    public async Task GetDocumentationAsHtmlAsync_ShouldWrapContentInHtmlTemplate()
    {
        // Arrange
        var folderPath = Path.Combine(_testDocRoot, "DOCUMENTATION", "TEST_FOLDER");
        Directory.CreateDirectory(folderPath);
        var readmePath = Path.Combine(folderPath, "README.md");
        await File.WriteAllTextAsync(readmePath, "# Test");

        var pathSegments = new[] { "TEST_FOLDER" };

        // Act
        var result = await _service.GetDocumentationAsHtmlAsync(pathSegments);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("<!DOCTYPE html>");
        result.Should().Contain("<html lang=\"en\">");
        result.Should().Contain("<head>");
        result.Should().Contain("<title>CesiZen Documentation</title>");
        result.Should().Contain("<style>");
        result.Should().Contain("CesiZen Documentation");
        result.Should().Contain(@"<a href=""/api/public/docs"" class=""home-button"">Home</a>");
        result.Should().Contain("<div class=\"markdown-body\">");
    }

    [Fact]
    public async Task GetDocumentationAsHtmlAsync_ShouldHandleRootReadme()
    {
        // Arrange
        var docRoot = Path.Combine(_testDocRoot, "DOCUMENTATION");
        Directory.CreateDirectory(docRoot);
        var readmePath = Path.Combine(docRoot, "README.md");
        await File.WriteAllTextAsync(readmePath, "# Root Documentation");

        var pathSegments = Array.Empty<string>();

        // Act
        var result = await _service.GetDocumentationAsHtmlAsync(pathSegments);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("Root Documentation");
    }

    #endregion

    #region Markdown Features Tests

    [Fact]
    public async Task GetDocumentationAsHtmlAsync_ShouldSupportCodeBlocks()
    {
        // Arrange
        var folderPath = Path.Combine(_testDocRoot, "DOCUMENTATION", "TEST");
        Directory.CreateDirectory(folderPath);
        var readmePath = Path.Combine(folderPath, "README.md");
        await File.WriteAllTextAsync(readmePath, @"
```csharp
var x = 10;
Console.WriteLine(x);
```
");

        var pathSegments = new[] { "TEST" };

        // Act
        var result = await _service.GetDocumentationAsHtmlAsync(pathSegments);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("<pre>");
        result.Should().Contain("<code");
        result.Should().Contain("var x = 10;");
    }

    [Fact]
    public async Task GetDocumentationAsHtmlAsync_ShouldSupportTables()
    {
        // Arrange
        var folderPath = Path.Combine(_testDocRoot, "DOCUMENTATION", "TEST");
        Directory.CreateDirectory(folderPath);
        var readmePath = Path.Combine(folderPath, "README.md");
        await File.WriteAllTextAsync(readmePath, @"
| Header 1 | Header 2 |
|----------|----------|
| Cell 1   | Cell 2   |
| Cell 3   | Cell 4   |
");

        var pathSegments = new[] { "TEST" };

        // Act
        var result = await _service.GetDocumentationAsHtmlAsync(pathSegments);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("<table>");
        result.Should().Contain("<thead>");
        result.Should().Contain("<tbody>");
        result.Should().Contain("<th>");
        result.Should().Contain("<td>");
    }

    [Fact]
    public async Task GetDocumentationAsHtmlAsync_ShouldRewriteLinksInContent()
    {
        // Arrange
        var folderPath = Path.Combine(_testDocRoot, "DOCUMENTATION", "INTEGRATION");
        Directory.CreateDirectory(folderPath);
        var readmePath = Path.Combine(folderPath, "README.md");
        await File.WriteAllTextAsync(readmePath, @"
# Integration Guide

[User Journeys](./USER_JOURNEYS/README.md)
[Authentication](./AUTHENTIFICATION/README.md)
[Back to Root](../README.md)
");

        var pathSegments = new[] { "INTEGRATION" };

        // Act
        var result = await _service.GetDocumentationAsHtmlAsync(pathSegments);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain(@"href=""/api/public/docs/integration/user_journeys""");
        result.Should().Contain(@"href=""/api/public/docs/integration/authentification""");
        result.Should().Contain(@"href=""/api/public/docs""");
    }

    #endregion

    #region Helper Methods

    private string InvokeRewriteLinks(string html, string[] pathSegments)
    {
        var method = typeof(DocumentationService).GetMethod(
            "RewriteLinks",
            BindingFlags.NonPublic | BindingFlags.Static
        );

        return (string)method!.Invoke(null, new object[] { html, pathSegments })!;
    }

    #endregion
}
