using FluentAssertions;
using api.CZ.Features.Bookmarks.Factories;

namespace api.Tests.Unit.Features.Bookmarks;

public class BookmarkFactoryTests
{
    private readonly BookmarkFactory _sut = new();

    [Fact]
    public void Create_NoParameters_ReturnsInstanceWithCreationTimeSet()
    {
        // Act
        var bookmark = _sut.Create();

        // Assert
        bookmark.CreationTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithUserIdAndConfigurationId_SetsIdAndConfigurationId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var configId = Guid.NewGuid();

        // Act
        var bookmark = _sut.Create(userId, configId);

        // Assert
        bookmark.Id.Should().Be(userId);
        bookmark.IdConfigurations.Should().Be(configId);
    }

    [Fact]
    public void Create_InvalidParameterCount_ThrowsArgumentException()
    {
        // Act
        var act = () => _sut.Create(Guid.NewGuid());

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
