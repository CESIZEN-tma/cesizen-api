using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using api.CZ.Features.AdminSessions.Factories;
using api.CZ.Features.AdminSessions.Models;
using api.CZ.Features.AdminSessions.Repositories;
using api.CZ.Features.AdminSessions.Services;
using api.Tests.Builders;
using System.Linq.Expressions;

namespace api.Tests.Unit.Services;

public class AdminSessionServiceTests
{
    private readonly Mock<IAdminSessionRepository> _mockRepository;
    private readonly Mock<IAdminSessionFactory> _mockFactory;
    private readonly Mock<ILogger<AdminSessionService>> _mockLogger;
    private readonly AdminSessionService _sut;

    public AdminSessionServiceTests()
    {
        _mockRepository = new Mock<IAdminSessionRepository>();
        _mockFactory = new Mock<IAdminSessionFactory>();
        _mockLogger = new Mock<ILogger<AdminSessionService>>();

        _sut = new AdminSessionService(_mockRepository.Object, _mockFactory.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByRefreshToken_ValidToken_ReturnsSession()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var refreshToken = "valid-refresh-token";
        var expectedSession = TestDataBuilder.AdminSessions.BuildValid(adminId);
        expectedSession.Token = refreshToken;

        _mockRepository.Setup(r => r.ListAsync(
                It.IsAny<Expression<Func<AdminSession, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AdminSession> { expectedSession });

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

        _mockRepository.Setup(r => r.ListAsync(
                It.IsAny<Expression<Func<AdminSession, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AdminSession>());

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

        _mockRepository.Setup(r => r.ListAsync(
                It.IsAny<Expression<Func<AdminSession, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AdminSession>());

        // Act
        var result = await _sut.GetByRefreshToken(refreshToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateSession_ValidParameters_CreatesAndReturnsSession()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var refreshToken = "new-refresh-token";
        var expiresAt = DateTime.UtcNow.AddDays(30);
        var expectedSession = TestDataBuilder.AdminSessions.Build(adminId);

        _mockFactory.Setup(f => f.Create(It.IsAny<object[]>()))
            .Returns(expectedSession);

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<AdminSession>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSession);

        // Act
        var result = await _sut.CreateSession(adminId, refreshToken, expiresAt);

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
        var session = TestDataBuilder.AdminSessions.BuildValid();
        session.Token = refreshToken;

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<AdminSession, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<AdminSession>(), It.IsAny<CancellationToken>()))
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
                It.IsAny<Expression<Func<AdminSession, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AdminSession?)null);

        // Act
        var result = await _sut.ConsumeSession(refreshToken);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<AdminSession>(), default), Times.Never);
    }

    [Fact]
    public async Task ConsumeSession_AlreadyConsumed_ReturnsFalse()
    {
        // Arrange
        var refreshToken = "consumed-token";
        var session = TestDataBuilder.AdminSessions.BuildConsumed();
        session.Token = refreshToken;

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<AdminSession, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var result = await _sut.ConsumeSession(refreshToken);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<AdminSession>(), default), Times.Never);
    }

    [Fact]
    public async Task ConsumeSession_ExpiredToken_ReturnsFalse()
    {
        // Arrange
        var refreshToken = "expired-token";
        var session = TestDataBuilder.AdminSessions.BuildExpired();
        session.Token = refreshToken;

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<AdminSession, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var result = await _sut.ConsumeSession(refreshToken);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<AdminSession>(), default), Times.Never);
    }

    [Fact]
    public async Task RevokeSession_ValidSessionId_ReturnsTrue()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = TestDataBuilder.AdminSessions.BuildValid();
        session.Id = sessionId;

        _mockRepository.Setup(r => r.FindAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<AdminSession>(), It.IsAny<CancellationToken>()))
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
            .ReturnsAsync((AdminSession?)null);

        // Act
        var result = await _sut.RevokeSession(sessionId);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<AdminSession>(), default), Times.Never);
    }

    [Fact]
    public async Task RevokeSession_AlreadyConsumed_ReturnsTrue()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = TestDataBuilder.AdminSessions.BuildConsumed();
        session.Id = sessionId;

        _mockRepository.Setup(r => r.FindAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var result = await _sut.RevokeSession(sessionId);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<AdminSession>(), default), Times.Never);
    }

    [Fact]
    public async Task RevokeAllAdminSessions_MultipleActiveSessions_RevokesAll()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var sessions = TestDataBuilder.AdminSessions.BuildMany(3, adminId);

        _mockRepository.Setup(r => r.ListAsync(
                It.IsAny<Expression<Func<AdminSession, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessions);

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<AdminSession>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.RevokeAllAdminSessions(adminId);

        // Assert
        result.Should().BeTrue();
        sessions.Should().AllSatisfy(s =>
        {
            s.Consumed.Should().BeTrue();
            s.UpdateTime.Should().NotBeNull();
        });

        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<AdminSession>(), default), Times.Exactly(3));
    }

    [Fact]
    public async Task RevokeAllAdminSessions_NoActiveSessions_ReturnsTrue()
    {
        // Arrange
        var adminId = Guid.NewGuid();

        _mockRepository.Setup(r => r.ListAsync(
                It.IsAny<Expression<Func<AdminSession, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AdminSession>());

        // Act
        var result = await _sut.RevokeAllAdminSessions(adminId);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<AdminSession>(), default), Times.Never);
    }

    [Fact]
    public async Task CleanupExpiredSessions_HasExpiredSessions_MarksThemAsConsumed()
    {
        // Arrange
        var expiredSessions = new List<AdminSession>
        {
            TestDataBuilder.AdminSessions.BuildExpired(),
            TestDataBuilder.AdminSessions.BuildExpired(),
            TestDataBuilder.AdminSessions.BuildExpired()
        };

        _mockRepository.Setup(r => r.ListAsync(
                It.IsAny<Expression<Func<AdminSession, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredSessions);

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<AdminSession>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.CleanupExpiredSessions();

        // Assert
        expiredSessions.Should().AllSatisfy(s =>
        {
            s.Consumed.Should().BeTrue();
            s.UpdateTime.Should().NotBeNull();
        });

        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<AdminSession>(), default), Times.Exactly(3));
    }

    [Fact]
    public async Task CleanupExpiredSessions_NoExpiredSessions_DoesNothing()
    {
        // Arrange
        _mockRepository.Setup(r => r.ListAsync(
                It.IsAny<Expression<Func<AdminSession, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AdminSession>());

        // Act
        await _sut.CleanupExpiredSessions();

        // Assert
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<AdminSession>(), default), Times.Never);
    }
}
