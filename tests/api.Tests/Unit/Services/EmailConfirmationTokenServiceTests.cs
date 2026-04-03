using FluentAssertions;
using Moq;
using api.CZ.Features.EmailConfirmationTokens.Factories;
using api.CZ.Features.EmailConfirmationTokens.Models;
using api.CZ.Features.EmailConfirmationTokens.Repositories;
using api.CZ.Features.EmailConfirmationTokens.Services;
using api.Tests.Builders;
using System.Linq.Expressions;

namespace api.Tests.Unit.Services;

public class EmailConfirmationTokenServiceTests
{
    private readonly Mock<IEmailConfirmationTokenRepository> _mockRepository;
    private readonly Mock<IEmailConfirmationTokenFactory> _mockFactory;
    private readonly EmailConfirmationTokenService _sut;

    public EmailConfirmationTokenServiceTests()
    {
        _mockRepository = new Mock<IEmailConfirmationTokenRepository>();
        _mockFactory = new Mock<IEmailConfirmationTokenFactory>();

        _sut = new EmailConfirmationTokenService(_mockRepository.Object, _mockFactory.Object);
    }

    [Fact]
    public async Task GetEntityByToken_ValidUnconsumedToken_ReturnsToken()
    {
        // Arrange
        var tokenString = "valid-token";
        var token = TestDataBuilder.EmailConfirmationTokens.BuildValid();
        token.Token = tokenString;

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<EmailConfirmationToken, bool>>>(),
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
        var tokenString = "consumed-token";
        var token = TestDataBuilder.EmailConfirmationTokens.BuildConsumed();
        token.Token = tokenString;

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<EmailConfirmationToken, bool>>>(),
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
        var tokenString = "non-existent";

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<EmailConfirmationToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmailConfirmationToken?)null);

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
        var newToken = TestDataBuilder.EmailConfirmationTokens.Build(userId);

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<EmailConfirmationToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmailConfirmationToken?)null);

        _mockFactory.Setup(f => f.Create(It.IsAny<object[]>()))
            .Returns(newToken);

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<EmailConfirmationToken>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newToken);

        // Act
        var result = await _sut.NewToken(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(newToken);

        _mockFactory.Verify(f => f.Create(It.IsAny<object[]>()), Times.Once);
        _mockRepository.Verify(r => r.AddAsync(newToken, default), Times.Once);
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<EmailConfirmationToken>(), default), Times.Never);
    }

    [Fact]
    public async Task NewToken_ExistingExpiredToken_DeletesOldAndCreatesNew()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingToken = TestDataBuilder.EmailConfirmationTokens.BuildExpired(userId);
        var newToken = TestDataBuilder.EmailConfirmationTokens.Build(userId);

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<EmailConfirmationToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingToken);

        _mockFactory.Setup(f => f.Create(It.IsAny<object[]>()))
            .Returns(newToken);

        _mockRepository.Setup(r => r.DeleteAsync(It.IsAny<EmailConfirmationToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<EmailConfirmationToken>(), It.IsAny<CancellationToken>()))
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
    public async Task NewToken_ExistingValidToken_CreatesNewWithoutDeletion()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingToken = TestDataBuilder.EmailConfirmationTokens.BuildValid(userId);
        var newToken = TestDataBuilder.EmailConfirmationTokens.Build(userId);

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<EmailConfirmationToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingToken);

        _mockFactory.Setup(f => f.Create(It.IsAny<object[]>()))
            .Returns(newToken);

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<EmailConfirmationToken>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newToken);

        // Act
        var result = await _sut.NewToken(userId);

        // Assert
        result.Should().NotBeNull();

        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<EmailConfirmationToken>(), default), Times.Never);
        _mockRepository.Verify(r => r.AddAsync(newToken, default), Times.Once);
    }

    [Fact]
    public async Task Consume_ValidToken_ReturnsTrue()
    {
        // Arrange
        var tokenString = "valid-token";
        var token = TestDataBuilder.EmailConfirmationTokens.BuildValid();
        token.Token = tokenString;

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<EmailConfirmationToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<EmailConfirmationToken>(), It.IsAny<CancellationToken>()))
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
        var tokenString = "non-existent";

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<EmailConfirmationToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmailConfirmationToken?)null);

        // Act
        var result = await _sut.Consume(tokenString);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<EmailConfirmationToken>(), default), Times.Never);
    }

    [Fact]
    public async Task Consume_AlreadyConsumedToken_ReturnsFalse()
    {
        // Arrange
        var tokenString = "consumed-token";
        var token = TestDataBuilder.EmailConfirmationTokens.BuildConsumed();
        token.Token = tokenString;

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<EmailConfirmationToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        // Act
        var result = await _sut.Consume(tokenString);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<EmailConfirmationToken>(), default), Times.Never);
    }

    [Fact]
    public async Task Consume_ExpiredToken_ReturnsFalse()
    {
        // Arrange
        var tokenString = "expired-token";
        var token = TestDataBuilder.EmailConfirmationTokens.BuildExpired();
        token.Token = tokenString;

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<EmailConfirmationToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        // Act
        var result = await _sut.Consume(tokenString);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<EmailConfirmationToken>(), default), Times.Never);
    }
}
