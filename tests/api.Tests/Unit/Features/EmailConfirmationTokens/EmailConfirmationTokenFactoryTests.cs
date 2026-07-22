using FluentAssertions;
using api.CZ.Features.EmailConfirmationTokens.Factories;

namespace api.Tests.Unit.Features.EmailConfirmationTokens;

public class EmailConfirmationTokenFactoryTests
{
    private readonly EmailConfirmationTokenFactory _sut = new();

    [Fact]
    public void Create_WithUserId_GeneratesSixDigitNumericToken()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var token = _sut.Create(userId);

        // Assert
        token.IdUsers.Should().Be(userId);
        token.Token.Should().MatchRegex("^[0-9]{6}$");
        int.Parse(token.Token).Should().BeInRange(100000, 999999);
    }

    [Fact]
    public void Create_WithUserId_SetsDefaultTwentyFourHourExpiration()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var token = _sut.Create(Guid.NewGuid());

        // Assert
        token.ExpiresAt.Should().BeCloseTo(before.AddHours(24), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithUserIdAndValidity_UsesCustomExpiration()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var token = _sut.Create(Guid.NewGuid(), TimeSpan.FromMinutes(10));

        // Assert
        token.ExpiresAt.Should().BeCloseTo(before.AddMinutes(10), TimeSpan.FromSeconds(5));
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
