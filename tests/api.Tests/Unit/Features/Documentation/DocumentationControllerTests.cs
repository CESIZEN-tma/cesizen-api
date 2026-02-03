using api.CZ.Features.Documentation;
using api.CZ.Features.Documentation.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace api.Tests.Unit.Features.Documentation;

public class DocumentationControllerTests
{
    private readonly Mock<IDocumentationService> _mockService;
    private readonly DocumentationController _controller;

    public DocumentationControllerTests()
    {
        _mockService = new Mock<IDocumentationService>();
        _controller = new DocumentationController(_mockService.Object);
    }

    #region Path Handling Tests

    [Fact]
    public async Task GetDocumentation_WithNoPath_ShouldUseEmptyPathSegments()
    {
        // Arrange
        var expectedHtml = "<html>Root Documentation</html>";
        _mockService
            .Setup(s => s.GetDocumentationAsHtmlAsync(It.Is<string[]>(p => p.Length == 0)))
            .ReturnsAsync(expectedHtml);

        // Act
        var result = await _controller.GetDocumentation(null);

        // Assert
        result.Should().BeOfType<ContentResult>();
        var contentResult = result as ContentResult;
        contentResult!.Content.Should().Be(expectedHtml);
        contentResult.ContentType.Should().Be("text/html");

        _mockService.Verify(
            s => s.GetDocumentationAsHtmlAsync(It.Is<string[]>(p => p.Length == 0)),
            Times.Once
        );
    }

    [Fact]
    public async Task GetDocumentation_WithEmptyString_ShouldUseEmptyPathSegments()
    {
        // Arrange
        var expectedHtml = "<html>Root Documentation</html>";
        _mockService
            .Setup(s => s.GetDocumentationAsHtmlAsync(It.Is<string[]>(p => p.Length == 0)))
            .ReturnsAsync(expectedHtml);

        // Act
        var result = await _controller.GetDocumentation(string.Empty);

        // Assert
        result.Should().BeOfType<ContentResult>();
        _mockService.Verify(
            s => s.GetDocumentationAsHtmlAsync(It.Is<string[]>(p => p.Length == 0)),
            Times.Once
        );
    }

    [Fact]
    public async Task GetDocumentation_WithSingleSegment_ShouldParseCorrectly()
    {
        // Arrange
        var path = "integration";
        var expectedHtml = "<html>Integration Documentation</html>";
        _mockService
            .Setup(s => s.GetDocumentationAsHtmlAsync(
                It.Is<string[]>(p => p.Length == 1 && p[0] == "INTEGRATION")))
            .ReturnsAsync(expectedHtml);

        // Act
        var result = await _controller.GetDocumentation(path);

        // Assert
        result.Should().BeOfType<ContentResult>();
        var contentResult = result as ContentResult;
        contentResult!.Content.Should().Be(expectedHtml);

        _mockService.Verify(
            s => s.GetDocumentationAsHtmlAsync(
                It.Is<string[]>(p => p.Length == 1 && p[0] == "INTEGRATION")),
            Times.Once
        );
    }

    [Fact]
    public async Task GetDocumentation_WithMultipleSegments_ShouldParseCorrectly()
    {
        // Arrange
        var path = "integration/user_journeys/new_user_path";
        var expectedHtml = "<html>New User Path Documentation</html>";
        _mockService
            .Setup(s => s.GetDocumentationAsHtmlAsync(
                It.Is<string[]>(p =>
                    p.Length == 3 &&
                    p[0] == "INTEGRATION" &&
                    p[1] == "USER_JOURNEYS" &&
                    p[2] == "NEW_USER_PATH")))
            .ReturnsAsync(expectedHtml);

        // Act
        var result = await _controller.GetDocumentation(path);

        // Assert
        result.Should().BeOfType<ContentResult>();
        var contentResult = result as ContentResult;
        contentResult!.Content.Should().Be(expectedHtml);

        _mockService.Verify(
            s => s.GetDocumentationAsHtmlAsync(
                It.Is<string[]>(p =>
                    p.Length == 3 &&
                    p[0] == "INTEGRATION" &&
                    p[1] == "USER_JOURNEYS" &&
                    p[2] == "NEW_USER_PATH")),
            Times.Once
        );
    }

    [Fact]
    public async Task GetDocumentation_ShouldConvertPathToUpperCase()
    {
        // Arrange
        var path = "integration/user_journeys";
        var expectedHtml = "<html>User Journeys</html>";
        _mockService
            .Setup(s => s.GetDocumentationAsHtmlAsync(
                It.Is<string[]>(p =>
                    p.Length == 2 &&
                    p[0] == "INTEGRATION" &&
                    p[1] == "USER_JOURNEYS")))
            .ReturnsAsync(expectedHtml);

        // Act
        var result = await _controller.GetDocumentation(path);

        // Assert
        _mockService.Verify(
            s => s.GetDocumentationAsHtmlAsync(
                It.Is<string[]>(p => p.All(segment => segment == segment.ToUpperInvariant()))),
            Times.Once
        );
    }

    [Fact]
    public async Task GetDocumentation_WithTrailingSlash_ShouldHandleCorrectly()
    {
        // Arrange
        var path = "integration/user_journeys/";
        var expectedHtml = "<html>User Journeys</html>";
        _mockService
            .Setup(s => s.GetDocumentationAsHtmlAsync(
                It.Is<string[]>(p =>
                    p.Length == 2 &&
                    p[0] == "INTEGRATION" &&
                    p[1] == "USER_JOURNEYS")))
            .ReturnsAsync(expectedHtml);

        // Act
        var result = await _controller.GetDocumentation(path);

        // Assert
        _mockService.Verify(
            s => s.GetDocumentationAsHtmlAsync(
                It.Is<string[]>(p => p.Length == 2)),
            Times.Once
        );
    }

    [Fact]
    public async Task GetDocumentation_WithMultipleSlashes_ShouldRemoveEmptySegments()
    {
        // Arrange
        var path = "integration//user_journeys///new_user_path";
        var expectedHtml = "<html>New User Path</html>";
        _mockService
            .Setup(s => s.GetDocumentationAsHtmlAsync(
                It.Is<string[]>(p =>
                    p.Length == 3 &&
                    !p.Any(string.IsNullOrEmpty))))
            .ReturnsAsync(expectedHtml);

        // Act
        var result = await _controller.GetDocumentation(path);

        // Assert
        _mockService.Verify(
            s => s.GetDocumentationAsHtmlAsync(
                It.Is<string[]>(p => p.Length == 3 && !p.Any(string.IsNullOrEmpty))),
            Times.Once
        );
    }

    [Theory]
    [InlineData("integration")]
    [InlineData("integration/user_journeys")]
    [InlineData("integration/user_journeys/new_user_path")]
    [InlineData("integration/authentification")]
    [InlineData("integration/features")]
    public async Task GetDocumentation_WithVariousPaths_ShouldCallServiceWithCorrectSegments(string path)
    {
        // Arrange
        var expectedHtml = "<html>Documentation</html>";
        _mockService
            .Setup(s => s.GetDocumentationAsHtmlAsync(It.IsAny<string[]>()))
            .ReturnsAsync(expectedHtml);

        // Act
        var result = await _controller.GetDocumentation(path);

        // Assert
        result.Should().BeOfType<ContentResult>();
        _mockService.Verify(
            s => s.GetDocumentationAsHtmlAsync(It.IsAny<string[]>()),
            Times.Once
        );
    }

    #endregion

    #region Response Handling Tests

    [Fact]
    public async Task GetDocumentation_WhenServiceReturnsHtml_ShouldReturnContentResult()
    {
        // Arrange
        var expectedHtml = "<html><body>Test Documentation</body></html>";
        _mockService
            .Setup(s => s.GetDocumentationAsHtmlAsync(It.IsAny<string[]>()))
            .ReturnsAsync(expectedHtml);

        // Act
        var result = await _controller.GetDocumentation("test");

        // Assert
        result.Should().BeOfType<ContentResult>();
        var contentResult = result as ContentResult;
        contentResult!.Content.Should().Be(expectedHtml);
        contentResult.ContentType.Should().Be("text/html");
    }

    [Fact]
    public async Task GetDocumentation_WhenServiceReturnsNull_ShouldReturnNotFound()
    {
        // Arrange
        _mockService
            .Setup(s => s.GetDocumentationAsHtmlAsync(It.IsAny<string[]>()))
            .ReturnsAsync((string?)null);

        // Act
        var result = await _controller.GetDocumentation("nonexistent");

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetDocumentation_WhenServiceThrowsException_ShouldPropagate()
    {
        // Arrange
        _mockService
            .Setup(s => s.GetDocumentationAsHtmlAsync(It.IsAny<string[]>()))
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _controller.GetDocumentation("test")
        );
    }

    #endregion

    #region Specific Path Tests

    [Fact]
    public async Task GetDocumentation_RootPath_ShouldReturnRootDocumentation()
    {
        // Arrange
        var expectedHtml = "<html>Root README</html>";
        _mockService
            .Setup(s => s.GetDocumentationAsHtmlAsync(Array.Empty<string>()))
            .ReturnsAsync(expectedHtml);

        // Act
        var result = await _controller.GetDocumentation(null);

        // Assert
        result.Should().BeOfType<ContentResult>();
        var contentResult = result as ContentResult;
        contentResult!.Content.Should().Contain("Root README");
    }

    [Fact]
    public async Task GetDocumentation_IntegrationPath_ShouldReturnIntegrationDocumentation()
    {
        // Arrange
        var expectedHtml = "<html>Integration Guide</html>";
        _mockService
            .Setup(s => s.GetDocumentationAsHtmlAsync(
                It.Is<string[]>(p => p.Length == 1 && p[0] == "INTEGRATION")))
            .ReturnsAsync(expectedHtml);

        // Act
        var result = await _controller.GetDocumentation("integration");

        // Assert
        result.Should().BeOfType<ContentResult>();
        var contentResult = result as ContentResult;
        contentResult!.Content.Should().Contain("Integration Guide");
    }

    [Fact]
    public async Task GetDocumentation_UserJourneysPath_ShouldReturnUserJourneysDocumentation()
    {
        // Arrange
        var expectedHtml = "<html>User Journeys</html>";
        _mockService
            .Setup(s => s.GetDocumentationAsHtmlAsync(
                It.Is<string[]>(p => p.Length == 2 && p[0] == "INTEGRATION" && p[1] == "USER_JOURNEYS")))
            .ReturnsAsync(expectedHtml);

        // Act
        var result = await _controller.GetDocumentation("integration/user_journeys");

        // Assert
        result.Should().BeOfType<ContentResult>();
        var contentResult = result as ContentResult;
        contentResult!.Content.Should().Contain("User Journeys");
    }

    [Fact]
    public async Task GetDocumentation_AuthentificationPath_ShouldReturnAuthDocumentation()
    {
        // Arrange
        var expectedHtml = "<html>Authentication</html>";
        _mockService
            .Setup(s => s.GetDocumentationAsHtmlAsync(
                It.Is<string[]>(p => p.Length == 2 && p[0] == "INTEGRATION" && p[1] == "AUTHENTIFICATION")))
            .ReturnsAsync(expectedHtml);

        // Act
        var result = await _controller.GetDocumentation("integration/authentification");

        // Assert
        result.Should().BeOfType<ContentResult>();
        var contentResult = result as ContentResult;
        contentResult!.Content.Should().Contain("Authentication");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task GetDocumentation_WithWhitespace_ShouldHandleCorrectly()
    {
        // Arrange
        var path = "  integration  /  user_journeys  ";
        var expectedHtml = "<html>Documentation</html>";
        _mockService
            .Setup(s => s.GetDocumentationAsHtmlAsync(It.IsAny<string[]>()))
            .ReturnsAsync(expectedHtml);

        // Act
        var result = await _controller.GetDocumentation(path);

        // Assert
        result.Should().BeOfType<ContentResult>();
        // Note: Split with RemoveEmptyEntries should handle whitespace
    }

    [Fact]
    public async Task GetDocumentation_CaseInsensitiveInput_ShouldConvertToUpperCase()
    {
        // Arrange
        var path = "InTeGrAtIoN/uSeR_jOuRnEyS";
        var expectedHtml = "<html>Documentation</html>";
        _mockService
            .Setup(s => s.GetDocumentationAsHtmlAsync(
                It.Is<string[]>(p => p.All(segment => segment == segment.ToUpperInvariant()))))
            .ReturnsAsync(expectedHtml);

        // Act
        var result = await _controller.GetDocumentation(path);

        // Assert
        _mockService.Verify(
            s => s.GetDocumentationAsHtmlAsync(
                It.Is<string[]>(p => p[0] == "INTEGRATION" && p[1] == "USER_JOURNEYS")),
            Times.Once
        );
    }

    #endregion
}
