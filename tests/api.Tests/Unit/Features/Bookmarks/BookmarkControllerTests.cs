using System.Security.Claims;
using api.CZ.Features.Bookmarks;
using api.CZ.Features.Bookmarks.DTOs;
using api.CZ.Features.Bookmarks.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace api.Tests.Unit.Features.Bookmarks;

public class BookmarkControllerTests
{
    private readonly Mock<IBookmarkService> _mockService;
    private readonly Mock<ILogger<BookmarkController>> _mockLogger;
    private readonly BookmarkController _controller;
    private readonly Guid _testUserId;

    public BookmarkControllerTests()
    {
        _mockService = new Mock<IBookmarkService>();
        _mockLogger = new Mock<ILogger<BookmarkController>>();
        _controller = new BookmarkController(_mockService.Object, _mockLogger.Object);
        _testUserId = Guid.NewGuid();

        // Setup user claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    #region GetUserBookmarks Tests

    [Fact]
    public async Task GetUserBookmarks_ShouldReturnOkWithBookmarks()
    {
        // Arrange
        var bookmarks = new List<GetBookmarkDto>
        {
            new GetBookmarkDto
            {
                UserId = _testUserId,
                ConfigurationId = Guid.NewGuid(),
                ConfigurationName = "My Config 1",
                CreationTime = DateTime.UtcNow
            },
            new GetBookmarkDto
            {
                UserId = _testUserId,
                ConfigurationId = Guid.NewGuid(),
                ConfigurationName = "My Config 2",
                CreationTime = DateTime.UtcNow
            }
        };

        _mockService
            .Setup(s => s.GetUserBookmarksAsync(_testUserId))
            .ReturnsAsync(bookmarks);

        // Act
        var result = await _controller.GetUserBookmarks();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(bookmarks);

        _mockService.Verify(s => s.GetUserBookmarksAsync(_testUserId), Times.Once);
    }

    [Fact]
    public async Task GetUserBookmarks_WhenNoBookmarks_ShouldReturnOkWithEmptyList()
    {
        // Arrange
        _mockService
            .Setup(s => s.GetUserBookmarksAsync(_testUserId))
            .ReturnsAsync(new List<GetBookmarkDto>());

        // Act
        var result = await _controller.GetUserBookmarks();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var bookmarks = okResult!.Value as IEnumerable<GetBookmarkDto>;
        bookmarks.Should().BeEmpty();

        _mockService.Verify(s => s.GetUserBookmarksAsync(_testUserId), Times.Once);
    }

    #endregion

    #region CreateBookmark Tests

    [Fact]
    public async Task CreateBookmark_WhenSuccessful_ShouldReturnCreatedWithBookmark()
    {
        // Arrange
        var configurationId = Guid.NewGuid();
        var createDto = new CreateBookmarkDto
        {
            ConfigurationId = configurationId
        };

        var createdBookmark = new GetBookmarkDto
        {
            UserId = _testUserId,
            ConfigurationId = configurationId,
            ConfigurationName = "My Configuration",
            CreationTime = DateTime.UtcNow
        };

        _mockService
            .Setup(s => s.CreateBookmarkAsync(_testUserId, createDto))
            .ReturnsAsync(createdBookmark);

        // Act
        var result = await _controller.CreateBookmark(createDto);

        // Assert
        result.Should().BeOfType<CreatedResult>();
        var createdResult = result as CreatedResult;
        createdResult!.Location.Should().Be($"/api/bookmarks/{configurationId}");
        createdResult.Value.Should().BeEquivalentTo(createdBookmark);

        _mockService.Verify(s => s.CreateBookmarkAsync(_testUserId, createDto), Times.Once);
    }

    [Fact]
    public async Task CreateBookmark_WhenAlreadyExists_ShouldReturnConflict()
    {
        // Arrange
        var createDto = new CreateBookmarkDto
        {
            ConfigurationId = Guid.NewGuid()
        };

        _mockService
            .Setup(s => s.CreateBookmarkAsync(_testUserId, createDto))
            .ThrowsAsync(new InvalidOperationException("Bookmark already exists"));

        // Act
        var result = await _controller.CreateBookmark(createDto);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
        var conflictResult = result as ConflictObjectResult;

        _mockService.Verify(s => s.CreateBookmarkAsync(_testUserId, createDto), Times.Once);
    }

    #endregion

    #region DeleteBookmark Tests

    [Fact]
    public async Task DeleteBookmark_WhenSuccessful_ShouldReturnOkWithMessage()
    {
        // Arrange
        var configurationId = Guid.NewGuid();

        _mockService
            .Setup(s => s.DeleteBookmarkAsync(_testUserId, configurationId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteBookmark(configurationId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;

        _mockService.Verify(s => s.DeleteBookmarkAsync(_testUserId, configurationId), Times.Once);
    }

    [Fact]
    public async Task DeleteBookmark_WhenBookmarkNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var configurationId = Guid.NewGuid();

        _mockService
            .Setup(s => s.DeleteBookmarkAsync(_testUserId, configurationId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteBookmark(configurationId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        _mockService.Verify(s => s.DeleteBookmarkAsync(_testUserId, configurationId), Times.Once);
    }

    #endregion
}
