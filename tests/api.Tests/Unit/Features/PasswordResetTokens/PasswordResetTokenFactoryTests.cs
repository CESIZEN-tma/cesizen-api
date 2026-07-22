using FluentAssertions;
using api.CZ.Features.PasswordResetTokens.Factories;

namespace api.Tests.Unit.Features.PasswordResetTokens;

public class PasswordResetTokenFactoryTests
{
    private readonly PasswordResetTokenFactory _sut = new();

    [Fact]
    public void Create_WithUserId_SetsDefaultFifteenMinuteExpiration()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var before = DateTime.UtcNow;

        // Act
        var token = _sut.Create(userId);

        // Assert
        token.IdUsers.Should().Be(userId);
        token.Consumed.Should().BeFalse();
        token.Token.Should().NotBeNullOrEmpty();
        token.ExpiresAt.Should().BeCloseTo(before.AddMinutes(15), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithUserIdAndValidity_UsesCustomExpiration()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var before = DateTime.UtcNow;

        // Act
        var token = _sut.Create(userId, TimeSpan.FromHours(2));

        // Assert
        token.ExpiresAt.Should().BeCloseTo(before.AddHours(2), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_TwoTokens_GeneratesDifferentTokenStrings()
    {
        // Act
        var token1 = _sut.Create(Guid.NewGuid());
        var token2 = _sut.Create(Guid.NewGuid());

        // Assert
        token1.Token.Should().NotBe(token2.Token);
    }

    [Fact]
    public void Create_InvalidParameters_ThrowsArgumentException()
    {
        // Act
        var act = () => _sut.Create("not-a-guid");

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
