using FluentAssertions;
using api.CZ.Features.AdminPasswordResetTokens.Factories;

namespace api.Tests.Unit.Features.AdminPasswordResetTokens;

public class AdminPasswordResetTokenFactoryTests
{
    private readonly AdminPasswordResetTokenFactory _sut = new();

    [Fact]
    public void Create_WithAdminId_SetsDefaultFifteenMinuteExpiration()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var before = DateTime.UtcNow;

        // Act
        var token = _sut.Create(adminId);

        // Assert
        token.IdAdministrators.Should().Be(adminId);
        token.Consumed.Should().BeFalse();
        token.ExpiresAt.Should().BeCloseTo(before.AddMinutes(15), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithAdminIdAndValidity_UsesCustomExpiration()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var before = DateTime.UtcNow;

        // Act
        var token = _sut.Create(adminId, TimeSpan.FromHours(1));

        // Assert
        token.ExpiresAt.Should().BeCloseTo(before.AddHours(1), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_InvalidParameters_ThrowsArgumentException()
    {
        // Act
        var act = () => _sut.Create();

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
