using FluentAssertions;
using api.CZ.Features.AdminEmailConfirmationTokens.Factories;

namespace api.Tests.Unit.Features.AdminEmailConfirmationTokens;

public class AdminEmailConfirmationTokenFactoryTests
{
    private readonly AdminEmailConfirmationTokenFactory _sut = new();

    [Fact]
    public void Create_WithAdminId_SetsDefaultTwentyFourHourExpiration()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var before = DateTime.UtcNow;

        // Act
        var token = _sut.Create(adminId);

        // Assert
        token.IdAdministrators.Should().Be(adminId);
        token.Consumed.Should().BeFalse();
        token.ExpiresAt.Should().BeCloseTo(before.AddHours(24), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithAdminIdAndValidity_UsesCustomExpiration()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var before = DateTime.UtcNow;

        // Act
        var token = _sut.Create(adminId, TimeSpan.FromMinutes(30));

        // Assert
        token.ExpiresAt.Should().BeCloseTo(before.AddMinutes(30), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_InvalidParameters_ThrowsArgumentException()
    {
        // Act
        var act = () => _sut.Create("wrong-type");

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
