using FluentAssertions;
using api.CZ.Features.Sessions.Factories;

namespace api.Tests.Unit.Features.Sessions;

public class SessionFactoryTests
{
    private readonly SessionFactory _sut = new();

    [Fact]
    public void Create_WithExplicitExpiresAt_UsesProvidedDate()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var session = _sut.Create(userId, "refresh-token", expiresAt);

        // Assert
        session.IdUsers.Should().Be(userId);
        session.Token.Should().Be("refresh-token");
        session.Consumed.Should().BeFalse();
        session.ExpiresAt.Should().Be(expiresAt);
    }

    [Fact]
    public void Create_WithValidityTimeSpan_ComputesExpiresAt()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var before = DateTime.UtcNow;

        // Act
        var session = _sut.Create(userId, "refresh-token", TimeSpan.FromDays(30));

        // Assert
        session.ExpiresAt.Should().BeCloseTo(before.AddDays(30), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_InvalidParameters_ThrowsArgumentException()
    {
        // Act
        var act = () => _sut.Create(Guid.NewGuid());

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
