using FluentAssertions;
using api.CZ.Features.Administrators.Factories;

namespace api.Tests.Unit.Features.Administrators;

public class AdministratorFactoryTests
{
    private readonly AdministratorFactory _sut = new();

    [Fact]
    public void Create_NoParameters_ReturnsInstanceWithNewId()
    {
        // Act
        var admin = _sut.Create();

        // Assert
        admin.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_WithEmail_SetsEmail()
    {
        // Act
        var admin = _sut.Create("admin@example.com");

        // Assert
        admin.Email.Should().Be("admin@example.com");
    }

    [Fact]
    public void Create_WithFullDetails_SetsAllFields()
    {
        // Act
        var admin = _sut.Create("admin@example.com", "First", "Last", "hashed-password");

        // Assert
        admin.Email.Should().Be("admin@example.com");
        admin.FirstName.Should().Be("First");
        admin.LastName.Should().Be("Last");
        admin.PasswordHash.Should().Be("hashed-password");
    }

    [Fact]
    public void Create_InvalidParameterCount_ThrowsArgumentException()
    {
        // Act
        var act = () => _sut.Create(1, 2, 3);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
