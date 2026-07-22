using FluentAssertions;
using api.CZ.Features.Users.Factories;

namespace api.Tests.Unit.Features.Users;

public class UserFactoryTests
{
    private readonly UserFactory _sut = new();

    [Fact]
    public void Create_NoParameters_ReturnsActiveUserWithNewId()
    {
        // Act
        var user = _sut.Create();

        // Assert
        user.Id.Should().NotBeEmpty();
        user.Active.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmail_SetsEmailAndDefaults()
    {
        // Act
        var user = _sut.Create("test@example.com");

        // Assert
        user.Email.Should().Be("test@example.com");
        user.Active.Should().BeTrue();
        user.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_WithFullDetails_SetsAllFields()
    {
        // Act
        var user = _sut.Create("test@example.com", "First", "Last", "hashed-password");

        // Assert
        user.Email.Should().Be("test@example.com");
        user.FirstName.Should().Be("First");
        user.LastName.Should().Be("Last");
        user.PasswordHash.Should().Be("hashed-password");
        user.Active.Should().BeTrue();
    }

    [Fact]
    public void Create_InvalidParameterCount_ThrowsArgumentException()
    {
        // Act
        var act = () => _sut.Create(1, 2, 3);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateMany_PositiveCount_ReturnsThatManyDistinctInstances()
    {
        // Act
        var users = _sut.CreateMany(3).ToList();

        // Assert
        users.Should().HaveCount(3);
        users.Select(u => u.Id).Distinct().Should().HaveCount(3);
    }

    [Fact]
    public void CreateMany_ZeroOrNegativeCount_ThrowsArgumentException()
    {
        // Act
        var act = () => _sut.CreateMany(0);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
