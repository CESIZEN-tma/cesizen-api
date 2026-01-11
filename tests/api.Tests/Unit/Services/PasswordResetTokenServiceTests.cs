using FluentAssertions;
using Moq;
using api.CZ.Features.PasswordResetTokens.Factories;
using api.CZ.Features.PasswordResetTokens.Models;
using api.CZ.Features.PasswordResetTokens.Repositories;
using api.CZ.Features.PasswordResetTokens.Services;
using api.Tests.Builders;
using System.Linq.Expressions;

namespace api.Tests.Unit.Services;

public class PasswordResetTokenServiceTests
{
    private readonly Mock<IPasswordResetTokenRepository> _mockRepository;
    private readonly Mock<IPasswordResetTokenFactory> _mockFactory;
    private readonly PasswordResetTokenService _sut;

    public PasswordResetTokenServiceTests()
    {
        _mockRepository = new Mock<IPasswordResetTokenRepository>();
        _mockFactory = new Mock<IPasswordResetTokenFactory>();

        _sut = new PasswordResetTokenService(_mockRepository.Object, _mockFactory.Object);
    }

    [Fact]
    public async Task GetEntityByToken_ValidUnconsumedToken_ReturnsToken()
    {
        // Arrange
        var tokenString = "valid-reset-token";
        var token = TestDataBuilder.PasswordResetTokens.BuildValid();
        token.Token = tokenString;

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<PasswordResetToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        // Act
        var result = await _sut.GetEntityByToken(tokenString);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(token);
        result!.Token.Should().Be(tokenString);
    }

    [Fact]
    public async Task GetEntityByToken_ConsumedToken_ReturnsNull()
    {
        // Arrange
        var tokenString = "consumed-reset-token";
        var token = TestDataBuilder.PasswordResetTokens.BuildConsumed();
        token.Token = tokenString;

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<PasswordResetToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        // Act
        var result = await _sut.GetEntityByToken(tokenString);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetEntityByToken_NonExistentToken_ReturnsNull()
    {
        // Arrange
        var tokenString = "non-existent-reset-token";

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<PasswordResetToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PasswordResetToken?)null);

        // Act
        var result = await _sut.GetEntityByToken(tokenString);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task NewToken_NoExistingToken_CreatesNewToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newToken = TestDataBuilder.PasswordResetTokens.Build(userId);

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<PasswordResetToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PasswordResetToken?)null);

        _mockFactory.Setup(f => f.Create(It.IsAny<object[]>()))
            .Returns(newToken);

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<PasswordResetToken>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newToken);

        // Act
        var result = await _sut.NewToken(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(newToken);

        _mockFactory.Verify(f => f.Create(It.IsAny<object[]>()), Times.Once);
        _mockRepository.Verify(r => r.AddAsync(newToken, default), Times.Once);
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<PasswordResetToken>(), default), Times.Never);
    }

    [Fact]
    public async Task NewToken_ExistingValidToken_DeletesOldAndCreatesNew()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingToken = TestDataBuilder.PasswordResetTokens.BuildValid(userId);
        var newToken = TestDataBuilder.PasswordResetTokens.Build(userId);

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<PasswordResetToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingToken);

        _mockFactory.Setup(f => f.Create(It.IsAny<object[]>()))
            .Returns(newToken);

        _mockRepository.Setup(r => r.DeleteAsync(It.IsAny<PasswordResetToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<PasswordResetToken>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newToken);

        // Act
        var result = await _sut.NewToken(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(newToken);

        _mockRepository.Verify(r => r.DeleteAsync(existingToken, default), Times.Once);
        _mockRepository.Verify(r => r.AddAsync(newToken, default), Times.Once);
    }

    [Fact]
    public async Task NewToken_ExistingExpiredToken_CreatesNewWithoutDeletion()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newToken = TestDataBuilder.PasswordResetTokens.Build(userId);

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<PasswordResetToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PasswordResetToken?)null); // Expired tokens won't be returned by the query

        _mockFactory.Setup(f => f.Create(It.IsAny<object[]>()))
            .Returns(newToken);

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<PasswordResetToken>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newToken);

        // Act
        var result = await _sut.NewToken(userId);

        // Assert
        result.Should().NotBeNull();

        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<PasswordResetToken>(), default), Times.Never);
        _mockRepository.Verify(r => r.AddAsync(newToken, default), Times.Once);
    }

    [Fact]
    public async Task Consume_ValidToken_ReturnsTrue()
    {
        // Arrange
        var tokenString = "valid-reset-token";
        var token = TestDataBuilder.PasswordResetTokens.BuildValid();
        token.Token = tokenString;

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<PasswordResetToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<PasswordResetToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Consume(tokenString);

        // Assert
        result.Should().BeTrue();
        token.Consumed.Should().BeTrue();
        token.ConsumedAt.Should().NotBeNull();
        token.UpdateTime.Should().NotBeNull();

        _mockRepository.Verify(r => r.UpdateAsync(token, default), Times.Once);
    }

    [Fact]
    public async Task Consume_NonExistentToken_ReturnsFalse()
    {
        // Arrange
        var tokenString = "non-existent-reset-token";

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<PasswordResetToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PasswordResetToken?)null);

        // Act
        var result = await _sut.Consume(tokenString);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<PasswordResetToken>(), default), Times.Never);
    }

    [Fact]
    public async Task Consume_AlreadyConsumedToken_ReturnsFalse()
    {
        // Arrange
        var tokenString = "consumed-reset-token";
        var token = TestDataBuilder.PasswordResetTokens.BuildConsumed();
        token.Token = tokenString;

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<PasswordResetToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        // Act
        var result = await _sut.Consume(tokenString);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<PasswordResetToken>(), default), Times.Never);
    }

    [Fact]
    public async Task Consume_ExpiredToken_ReturnsFalse()
    {
        // Arrange
        var tokenString = "expired-reset-token";
        var token = TestDataBuilder.PasswordResetTokens.BuildExpired();
        token.Token = tokenString;

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<PasswordResetToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        // Act
        var result = await _sut.Consume(tokenString);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<PasswordResetToken>(), default), Times.Never);
    }
}
