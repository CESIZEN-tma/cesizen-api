using api.CZ.Features.Documentation;
using api.CZ.Features.Documentation.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace api.Tests.Integration.Features.Documentation;

public class DocumentationIntegrationTests : IDisposable
{
    private readonly string _testDocRoot;
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    private readonly DocumentationService _service;
    private readonly DocumentationController _controller;

    public DocumentationIntegrationTests()
    {
        // Setup test directory
        _testDocRoot = Path.Combine(Path.GetTempPath(), "CesiZenDocsIntegrationTest", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDocRoot);

        // Setup mock environment
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _mockEnvironment.Setup(e => e.ContentRootPath).Returns(Path.GetDirectoryName(_testDocRoot)!);

        // Create service and controller
        _service = new DocumentationService(_mockEnvironment.Object);
        _controller = new DocumentationController(_service);

        // Setup test documentation structure
        SetupTestDocumentation();
    }

    private void SetupTestDocumentation()
    {
        var docRoot = Path.Combine(_testDocRoot, "DOCUMENTATION");
        Directory.CreateDirectory(docRoot);

        // Root README
        File.WriteAllText(
            Path.Combine(docRoot, "README.md"),
            @"# CesiZen Documentation

[Integration Guide](./INTEGRATION/README.md)
[Features](./FEATURES/README.md)
"
        );

        // Integration folder
        var integrationPath = Path.Combine(docRoot, "INTEGRATION");
        Directory.CreateDirectory(integrationPath);
        File.WriteAllText(
            Path.Combine(integrationPath, "README.md"),
            @"# Integration Guide

[User Journeys](./USER_JOURNEYS/README.md)
[Authentication](./AUTHENTIFICATION/README.md)
[Back to Root](../README.md)
"
        );

        // User Journeys folder
        var userJourneysPath = Path.Combine(integrationPath, "USER_JOURNEYS");
        Directory.CreateDirectory(userJourneysPath);
        File.WriteAllText(
            Path.Combine(userJourneysPath, "README.md"),
            @"# User Journeys

[New User Path](./NEW_USER_PATH.md)
[Returning User Path](./RETURNING_USER_PATH.md)
[Back to Integration](../README.md)
"
        );

        File.WriteAllText(
            Path.Combine(userJourneysPath, "NEW_USER_PATH.md"),
            @"# New User Path

This is the new user journey.

[Quiz Flow](./QUIZ_TO_EXERCISE_PATH.md)
[Back to User Journeys](./README.md)
[Auth Docs](../AUTHENTIFICATION/README.md)
"
        );

        File.WriteAllText(
            Path.Combine(userJourneysPath, "RETURNING_USER_PATH.md"),
            "# Returning User Path\n\nReturning user content."
        );

        File.WriteAllText(
            Path.Combine(userJourneysPath, "QUIZ_TO_EXERCISE_PATH.md"),
            @"# Quiz to Exercise Path

Detailed quiz flow.

[Back to New User Path](./NEW_USER_PATH.md)
"
        );

        // Authentification folder
        var authPath = Path.Combine(integrationPath, "AUTHENTIFICATION");
        Directory.CreateDirectory(authPath);
        File.WriteAllText(
            Path.Combine(authPath, "README.md"),
            @"# Authentication

[Login](./LOGIN.md)
[Registration](./REGISTRATION.md)
[Back to Integration](../README.md)
"
        );

        File.WriteAllText(
            Path.Combine(authPath, "LOGIN.md"),
            "# Login\n\nLogin documentation."
        );

        File.WriteAllText(
            Path.Combine(authPath, "REGISTRATION.md"),
            "# Registration\n\nRegistration documentation."
        );

        // Features folder
        var featuresPath = Path.Combine(docRoot, "FEATURES");
        Directory.CreateDirectory(featuresPath);
        File.WriteAllText(
            Path.Combine(featuresPath, "README.md"),
            @"# Features

[Back to Root](../README.md)
"
        );
    }

    #region Full Flow Integration Tests

    [Fact]
    public async Task GetDocumentation_RootPath_ShouldReturnRootReadmeWithLinks()
    {
        // Act
        var result = await _controller.GetDocumentation(null);

        // Assert
        result.Should().BeOfType<ContentResult>();
        var contentResult = result as ContentResult;

        contentResult!.Content.Should().NotBeNull();
        contentResult.Content.Should().Contain("CesiZen Documentation");
        contentResult.Content.Should().Contain(@"href=""/api/public/docs/integration""");
        contentResult.Content.Should().Contain(@"href=""/api/public/docs/features""");
    }

    [Fact]
    public async Task GetDocumentation_IntegrationPath_ShouldReturnIntegrationReadmeWithConvertedLinks()
    {
        // Act
        var result = await _controller.GetDocumentation("integration");

        // Assert
        result.Should().BeOfType<ContentResult>();
        var contentResult = result as ContentResult;

        contentResult!.Content.Should().Contain("Integration Guide");
        contentResult.Content.Should().Contain(@"href=""/api/public/docs/integration/user_journeys""");
        contentResult.Content.Should().Contain(@"href=""/api/public/docs/integration/authentification""");
        contentResult.Content.Should().Contain(@"href=""/api/public/docs"""); // Back to root link
    }

    [Fact]
    public async Task GetDocumentation_UserJourneysPath_ShouldReturnUserJourneysReadmeWithLinks()
    {
        // Act
        var result = await _controller.GetDocumentation("integration/user_journeys");

        // Assert
        result.Should().BeOfType<ContentResult>();
        var contentResult = result as ContentResult;

        contentResult!.Content.Should().Contain("User Journeys");
        contentResult.Content.Should().Contain(@"href=""/api/public/docs/integration/user_journeys/new_user_path""");
        contentResult.Content.Should().Contain(@"href=""/api/public/docs/integration/user_journeys/returning_user_path""");
        contentResult.Content.Should().Contain(@"href=""/api/public/docs/integration"""); // Back to integration
    }

    [Fact]
    public async Task GetDocumentation_NewUserPathDocument_ShouldReturnDocumentWithMultipleLinks()
    {
        // Act
        var result = await _controller.GetDocumentation("integration/user_journeys/new_user_path");

        // Assert
        result.Should().BeOfType<ContentResult>();
        var contentResult = result as ContentResult;

        contentResult!.Content.Should().Contain("New User Path");
        contentResult.Content.Should().Contain(@"href=""/api/public/docs/integration/user_journeys/quiz_to_exercise_path""");
        contentResult.Content.Should().Contain(@"href=""/api/public/docs/integration/user_journeys"""); // Back to user journeys
        contentResult.Content.Should().Contain(@"href=""/api/public/docs/integration/authentification"""); // Cross-reference to auth
    }

    [Fact]
    public async Task GetDocumentation_QuizPath_ShouldReturnDocumentWithBackLink()
    {
        // Act
        var result = await _controller.GetDocumentation("integration/user_journeys/quiz_to_exercise_path");

        // Assert
        result.Should().BeOfType<ContentResult>();
        var contentResult = result as ContentResult;

        contentResult!.Content.Should().Contain("Quiz to Exercise Path");
        contentResult.Content.Should().Contain(@"href=""/api/public/docs/integration/user_journeys/new_user_path""");
    }

    [Fact]
    public async Task GetDocumentation_AuthenticationPath_ShouldReturnAuthReadmeWithLinks()
    {
        // Act
        var result = await _controller.GetDocumentation("integration/authentification");

        // Assert
        result.Should().BeOfType<ContentResult>();
        var contentResult = result as ContentResult;

        contentResult!.Content.Should().Contain("Authentication");
        contentResult.Content.Should().Contain(@"href=""/api/public/docs/integration/authentification/login""");
        contentResult.Content.Should().Contain(@"href=""/api/public/docs/integration/authentification/registration""");
        contentResult.Content.Should().Contain(@"href=""/api/public/docs/integration""");
    }

    [Fact]
    public async Task GetDocumentation_LoginDocument_ShouldReturnLoginDoc()
    {
        // Act
        var result = await _controller.GetDocumentation("integration/authentification/login");

        // Assert
        result.Should().BeOfType<ContentResult>();
        var contentResult = result as ContentResult;

        contentResult!.Content.Should().Contain("Login");
        contentResult.Content.Should().Contain("Login documentation");
    }

    [Fact]
    public async Task GetDocumentation_NonexistentPath_ShouldReturnNotFound()
    {
        // Act
        var result = await _controller.GetDocumentation("nonexistent/path");

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Navigation Flow Tests

    [Fact]
    public async Task NavigationFlow_RootToIntegrationToUserJourneys_ShouldWork()
    {
        // Step 1: Get root
        var rootResult = await _controller.GetDocumentation(null);
        rootResult.Should().BeOfType<ContentResult>();
        var rootContent = (rootResult as ContentResult)!.Content!;
        rootContent.Should().Contain(@"href=""/api/public/docs/integration""");

        // Step 2: Get integration
        var integrationResult = await _controller.GetDocumentation("integration");
        integrationResult.Should().BeOfType<ContentResult>();
        var integrationContent = (integrationResult as ContentResult)!.Content!;
        integrationContent.Should().Contain(@"href=""/api/public/docs/integration/user_journeys""");

        // Step 3: Get user journeys
        var userJourneysResult = await _controller.GetDocumentation("integration/user_journeys");
        userJourneysResult.Should().BeOfType<ContentResult>();
        var userJourneysContent = (userJourneysResult as ContentResult)!.Content!;
        userJourneysContent.Should().Contain("User Journeys");
    }

    [Fact]
    public async Task NavigationFlow_DeepLinkAndBackNavigation_ShouldWork()
    {
        // Step 1: Go to new user path
        var newUserResult = await _controller.GetDocumentation("integration/user_journeys/new_user_path");
        newUserResult.Should().BeOfType<ContentResult>();
        var newUserContent = (newUserResult as ContentResult)!.Content!;

        // Should have back link to user journeys
        newUserContent.Should().Contain(@"href=""/api/public/docs/integration/user_journeys""");

        // Should have cross-reference to auth
        newUserContent.Should().Contain(@"href=""/api/public/docs/integration/authentification""");

        // Step 2: Navigate to auth via cross-reference
        var authResult = await _controller.GetDocumentation("integration/authentification");
        authResult.Should().BeOfType<ContentResult>();
        var authContent = (authResult as ContentResult)!.Content!;
        authContent.Should().Contain("Authentication");
    }

    #endregion

    #region HTML Template Tests

    [Fact]
    public async Task GetDocumentation_ShouldIncludeHtmlTemplate()
    {
        // Act
        var result = await _controller.GetDocumentation(null);

        // Assert
        var contentResult = result as ContentResult;
        var html = contentResult!.Content!;

        html.Should().Contain("<!DOCTYPE html>");
        html.Should().Contain("<html lang=\"en\">");
        html.Should().Contain("<title>CesiZen Documentation</title>");
        html.Should().Contain("CesiZen Documentation"); // Header
        html.Should().Contain(@"<a href=""/api/public/docs"" class=""home-button"">Home</a>");
        html.Should().Contain("<div class=\"markdown-body\">");
        html.Should().Contain("<style>");
    }

    [Fact]
    public async Task GetDocumentation_HomeButtonShouldAlwaysPointToRoot()
    {
        // Test at various depths
        var paths = new[]
        {
            null,
            "integration",
            "integration/user_journeys",
            "integration/user_journeys/new_user_path"
        };

        foreach (var path in paths)
        {
            // Act
            var result = await _controller.GetDocumentation(path);

            // Assert
            var contentResult = result as ContentResult;
            contentResult!.Content.Should().Contain(@"<a href=""/api/public/docs"" class=""home-button"">Home</a>");
        }
    }

    #endregion

    #region Markdown Rendering Tests

    [Fact]
    public async Task GetDocumentation_ShouldConvertMarkdownToHtml()
    {
        // Act
        var result = await _controller.GetDocumentation(null);

        // Assert
        var contentResult = result as ContentResult;
        var html = contentResult!.Content!;

        // Should convert markdown heading to HTML
        html.Should().Contain("<h1");
        html.Should().Contain("CesiZen Documentation");

        // Should convert links
        html.Should().Contain("<a href=");
    }

    [Fact]
    public async Task GetDocumentation_ComplexMarkdown_ShouldRenderCorrectly()
    {
        // Create a test file with complex markdown
        var testPath = Path.Combine(_testDocRoot, "DOCUMENTATION", "TEST_COMPLEX.md");
        File.WriteAllText(testPath, @"
# Complex Test

## Heading 2

**Bold** and *italic* text.

- List item 1
- List item 2

```csharp
var code = ""test"";
```

[Link](./README.md)
");

        // Act
        var result = await _controller.GetDocumentation("test_complex");

        // Assert
        var contentResult = result as ContentResult;
        var html = contentResult!.Content!;

        html.Should().Contain("<h1");
        html.Should().Contain("<h2");
        html.Should().Contain("<strong>Bold</strong>");
        html.Should().Contain("<em>italic</em>");
        html.Should().Contain("<ul>");
        html.Should().Contain("<li>");
        html.Should().Contain("<pre>");
        html.Should().Contain("<code");
        html.Should().Contain(@"href=""/api/public/docs""");
    }

    #endregion

    #region Case Sensitivity Tests

    [Theory]
    [InlineData("integration")]
    [InlineData("Integration")]
    [InlineData("INTEGRATION")]
    [InlineData("InTeGrAtIoN")]
    public async Task GetDocumentation_CaseInsensitiveInput_ShouldWork(string path)
    {
        // Act
        var result = await _controller.GetDocumentation(path);

        // Assert
        result.Should().BeOfType<ContentResult>();
        var contentResult = result as ContentResult;
        contentResult!.Content.Should().Contain("Integration Guide");
    }

    #endregion

    public void Dispose()
    {
        // Cleanup test directory
        if (Directory.Exists(_testDocRoot))
        {
            try
            {
                Directory.Delete(_testDocRoot, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
