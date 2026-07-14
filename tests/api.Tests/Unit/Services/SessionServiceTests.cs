using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using api.CZ.Features.Sessions.Factories;
using api.CZ.Features.Sessions.Models;
using api.CZ.Features.Sessions.Repositories;
using api.CZ.Features.Sessions.Services;
using api.Tests.Builders;
using System.Linq.Expressions;

namespace api.Tests.Unit.Services;

public class SessionServiceTests
{
    private readonly Mock<ISessionRepository> _mockRepository;
    private readonly Mock<ISessionFactory> _mockFactory;
    private readonly Mock<ILogger<SessionService>> _mockLogger;
    private readonly SessionService _sut; // System Under Test

    public SessionServiceTests()
    {
        _mockRepository = new Mock<ISessionRepository>();
        _mockFactory = new Mock<ISessionFactory>();
        _mockLogger = new Mock<ILogger<SessionService>>();

        _sut = new SessionService(_mockRepository.Object, _mockFactory.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByRefreshToken_ValidToken_ReturnsSession()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = "valid-refresh-token";
        var expectedSession = TestDataBuilder.Sessions.BuildValid(userId);
        expectedSession.Token = refreshToken;

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Session, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSession);

        // Act
        var result = await _sut.GetByRefreshToken(refreshToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(expectedSession);
        result!.Token.Should().Be(refreshToken);
    }

    [Fact]
    public async Task GetByRefreshToken_ExpiredToken_ReturnsNull()
    {
        // Arrange
        var refreshToken = "expired-token";

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Session, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Session?)null);

        // Act
        var result = await _sut.GetByRefreshToken(refreshToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByRefreshToken_ConsumedToken_ReturnsNull()
    {
        // Arrange
        var refreshToken = "consumed-token";

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Session, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Session?)null);

        // Act
        var result = await _sut.GetByRefreshToken(refreshToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateSession_ValidParameters_CreatesAndReturnsSession()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = "new-refresh-token";
        var expiresAt = DateTime.UtcNow.AddDays(30);
        var expectedSession = TestDataBuilder.Sessions.Build(userId);

        _mockFactory.Setup(f => f.Create(It.IsAny<object[]>()))
            .Returns(expectedSession);

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSession);

        // Act
        var result = await _sut.CreateSession(userId, refreshToken, expiresAt);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(expectedSession);

        _mockFactory.Verify(f => f.Create(It.IsAny<object[]>()), Times.Once);
        _mockRepository.Verify(r => r.AddAsync(expectedSession, default), Times.Once);
    }

    [Fact]
    public async Task ConsumeSession_ValidToken_ReturnsTrue()
    {
        // Arrange
        var refreshToken = "valid-token";
        var session = TestDataBuilder.Sessions.BuildValid();
        session.Token = refreshToken;

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Session, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ConsumeSession(refreshToken);

        // Assert
        result.Should().BeTrue();
        session.Consumed.Should().BeTrue();
        session.UpdateTime.Should().NotBeNull();

        _mockRepository.Verify(r => r.UpdateAsync(session, default), Times.Once);
    }

    [Fact]
    public async Task ConsumeSession_NonExistentToken_ReturnsFalse()
    {
        // Arrange
        var refreshToken = "non-existent-token";

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Session, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Session?)null);

        // Act
        var result = await _sut.ConsumeSession(refreshToken);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Session>(), default), Times.Never);
    }

    [Fact]
    public async Task ConsumeSession_AlreadyConsumed_ReturnsFalse()
    {
        // Arrange
        var refreshToken = "consumed-token";
        var session = TestDataBuilder.Sessions.BuildConsumed();
        session.Token = refreshToken;

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Session, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var result = await _sut.ConsumeSession(refreshToken);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Session>(), default), Times.Never);
    }

    [Fact]
    public async Task ConsumeSession_ExpiredToken_ReturnsFalse()
    {
        // Arrange
        var refreshToken = "expired-token";
        var session = TestDataBuilder.Sessions.BuildExpired();
        session.Token = refreshToken;

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Session, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var result = await _sut.ConsumeSession(refreshToken);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Session>(), default), Times.Never);
    }

    [Fact]
    public async Task RevokeSession_ValidSessionId_ReturnsTrue()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = TestDataBuilder.Sessions.BuildValid();
        session.Id = sessionId;

        _mockRepository.Setup(r => r.FindAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.RevokeSession(sessionId);

        // Assert
        result.Should().BeTrue();
        session.Consumed.Should().BeTrue();
        session.UpdateTime.Should().NotBeNull();

        _mockRepository.Verify(r => r.UpdateAsync(session, default), Times.Once);
    }

    [Fact]
    public async Task RevokeSession_NonExistentSession_ReturnsFalse()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        _mockRepository.Setup(r => r.FindAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Session?)null);

        // Act
        var result = await _sut.RevokeSession(sessionId);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Session>(), default), Times.Never);
    }

    [Fact]
    public async Task RevokeSession_AlreadyConsumed_ReturnsTrue()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = TestDataBuilder.Sessions.BuildConsumed();
        session.Id = sessionId;

        _mockRepository.Setup(r => r.FindAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var result = await _sut.RevokeSession(sessionId);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Session>(), default), Times.Never);
    }

    [Fact]
    public async Task RevokeAllUserSessions_MultipleActiveSessions_RevokesAll()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessions = TestDataBuilder.Sessions.BuildMany(3, userId);

        _mockRepository.Setup(r => r.ListAsync(
                It.IsAny<Expression<Func<Session, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessions);

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.RevokeAllUserSessions(userId);

        // Assert
        result.Should().BeTrue();
        sessions.Should().AllSatisfy(s =>
        {
            s.Consumed.Should().BeTrue();
            s.UpdateTime.Should().NotBeNull();
        });

        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Session>(), default), Times.Exactly(3));
    }

    [Fact]
    public async Task RevokeAllUserSessions_NoActiveSessions_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockRepository.Setup(r => r.ListAsync(
                It.IsAny<Expression<Func<Session, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Session>());

        // Act
        var result = await _sut.RevokeAllUserSessions(userId);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Session>(), default), Times.Never);
    }

    [Fact]
    public async Task CleanupExpiredSessions_HasExpiredSessions_MarksThemAsConsumed()
    {
        // Arrange
        var expiredSessions = new List<Session>
        {
            TestDataBuilder.Sessions.BuildExpired(),
            TestDataBuilder.Sessions.BuildExpired(),
            TestDataBuilder.Sessions.BuildExpired()
        };

        _mockRepository.Setup(r => r.ListAsync(
                It.IsAny<Expression<Func<Session, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredSessions);

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.CleanupExpiredSessions();

        // Assert
        expiredSessions.Should().AllSatisfy(s =>
        {
            s.Consumed.Should().BeTrue();
            s.UpdateTime.Should().NotBeNull();
        });

        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Session>(), default), Times.Exactly(3));
    }

    [Fact]
    public async Task CleanupExpiredSessions_NoExpiredSessions_DoesNothing()
    {
        // Arrange
        _mockRepository.Setup(r => r.ListAsync(
                It.IsAny<Expression<Func<Session, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Session>());

        // Act
        await _sut.CleanupExpiredSessions();

        // Assert
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Session>(), default), Times.Never);
    }
}
