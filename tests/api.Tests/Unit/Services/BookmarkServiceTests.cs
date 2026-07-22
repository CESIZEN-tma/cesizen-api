using FluentAssertions;
using Moq;
using api.CZ.Features.Bookmarks.DTOs;
using api.CZ.Features.Bookmarks.Factories;
using api.CZ.Features.Bookmarks.Models;
using api.CZ.Features.Bookmarks.Repositories;
using api.CZ.Features.Bookmarks.Services;
using api.CZ.Features.Configurations.Models;

namespace api.Tests.Unit.Services;

public class BookmarkServiceTests
{
    private readonly Mock<IBookmarkRepository> _mockRepository;
    private readonly Mock<IBookmarkFactory> _mockFactory;
    private readonly BookmarkService _sut;

    public BookmarkServiceTests()
    {
        _mockRepository = new Mock<IBookmarkRepository>();
        _mockFactory = new Mock<IBookmarkFactory>();
        _sut = new BookmarkService(_mockRepository.Object, _mockFactory.Object);
    }

    private static Bookmark BuildBookmark(Guid userId, Guid configurationId, string configName = "Config")
    {
        return new Bookmark
        {
            Id = userId,
            IdConfigurations = configurationId,
            CreationTime = DateTime.UtcNow,
            IdConfigurationsNavigation = new Configuration
            {
                Id = configurationId,
                Name = configName,
                Objective = "Relaxation",
                GuidanceType = "Visual",
                CreationTime = DateTime.UtcNow
            }
        };
    }

    [Fact]
    public async Task GetUserBookmarksAsync_ReturnsMappedDtos()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var bookmarks = new List<Bookmark> { BuildBookmark(userId, Guid.NewGuid()) };
        _mockRepository.Setup(r => r.GetUserBookmarksAsync(userId)).ReturnsAsync(bookmarks);

        // Act
        var result = (await _sut.GetUserBookmarksAsync(userId)).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].ConfigurationName.Should().Be("Config");
    }

    [Fact]
    public async Task CreateBookmarkAsync_NoExistingBookmark_CreatesNewBookmark()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = new CreateBookmarkDto { ConfigurationId = Guid.NewGuid() };
        var createdBookmark = BuildBookmark(userId, dto.ConfigurationId);

        _mockRepository.Setup(r => r.GetUserBookmarkIncludingDeletedAsync(userId, dto.ConfigurationId))
            .ReturnsAsync((Bookmark?)null);
        _mockFactory.Setup(f => f.Create(userId, dto.ConfigurationId)).Returns(createdBookmark);

        // Act
        var result = await _sut.CreateBookmarkAsync(userId, dto);

        // Assert
        result.Should().NotBeNull();
        _mockRepository.Verify(r => r.AddAsync(createdBookmark, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateBookmarkAsync_ActiveBookmarkAlreadyExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = new CreateBookmarkDto { ConfigurationId = Guid.NewGuid() };
        var existing = BuildBookmark(userId, dto.ConfigurationId);

        _mockRepository.Setup(r => r.GetUserBookmarkIncludingDeletedAsync(userId, dto.ConfigurationId))
            .ReturnsAsync(existing);

        // Act
        var act = () => _sut.CreateBookmarkAsync(userId, dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Bookmark>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateBookmarkAsync_SoftDeletedBookmarkExists_RestoresIt()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = new CreateBookmarkDto { ConfigurationId = Guid.NewGuid() };
        var existing = BuildBookmark(userId, dto.ConfigurationId);
        existing.DeletionTime = DateTime.UtcNow;

        _mockRepository.Setup(r => r.GetUserBookmarkIncludingDeletedAsync(userId, dto.ConfigurationId))
            .ReturnsAsync(existing);

        // Act
        var result = await _sut.CreateBookmarkAsync(userId, dto);

        // Assert
        result.Should().NotBeNull();
        existing.DeletionTime.Should().BeNull();
        _mockRepository.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Bookmark>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockFactory.Verify(f => f.Create(It.IsAny<object[]>()), Times.Never);
    }

    [Fact]
    public async Task DeleteBookmarkAsync_ExistingBookmark_SoftDeletesAndReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var configId = Guid.NewGuid();
        var bookmark = BuildBookmark(userId, configId);

        _mockRepository.Setup(r => r.GetUserBookmarkAsync(userId, configId)).ReturnsAsync(bookmark);

        // Act
        var result = await _sut.DeleteBookmarkAsync(userId, configId);

        // Assert
        result.Should().BeTrue();
        bookmark.DeletionTime.Should().NotBeNull();
        _mockRepository.Verify(r => r.SoftDeleteAsync(bookmark, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteBookmarkAsync_NonExistentBookmark_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var configId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetUserBookmarkAsync(userId, configId)).ReturnsAsync((Bookmark?)null);

        // Act
        var result = await _sut.DeleteBookmarkAsync(userId, configId);

        // Assert
        result.Should().BeFalse();
    }
}
