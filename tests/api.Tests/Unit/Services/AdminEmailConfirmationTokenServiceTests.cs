using FluentAssertions;
using Moq;
using api.CZ.Features.AdminEmailConfirmationTokens.Factories;
using api.CZ.Features.AdminEmailConfirmationTokens.Models;
using api.CZ.Features.AdminEmailConfirmationTokens.Repositories;
using api.CZ.Features.AdminEmailConfirmationTokens.Services;
using api.Tests.Builders;
using System.Linq.Expressions;

namespace api.Tests.Unit.Services;

public class AdminEmailConfirmationTokenServiceTests
{
    private readonly Mock<IAdminEmailConfirmationTokenRepository> _mockRepository;
    private readonly Mock<IAdminEmailConfirmationTokenFactory> _mockFactory;
    private readonly AdminEmailConfirmationTokenService _sut;

    public AdminEmailConfirmationTokenServiceTests()
    {
        _mockRepository = new Mock<IAdminEmailConfirmationTokenRepository>();
        _mockFactory = new Mock<IAdminEmailConfirmationTokenFactory>();

        _sut = new AdminEmailConfirmationTokenService(_mockRepository.Object, _mockFactory.Object);
    }

    [Fact]
    public async Task GetEntityByToken_ValidUnconsumedToken_ReturnsToken()
    {
        // Arrange
        var tokenString = "valid-token";
        var token = TestDataBuilder.AdminEmailConfirmationTokens.BuildValid();
        token.Token = tokenString;

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<AdminEmailConfirmationToken, bool>>>(),
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
        var token = TestDataBuilder.AdminEmailConfirmationTokens.BuildConsumed();
        token.Token = tokenString;

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<AdminEmailConfirmationToken, bool>>>(),
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
                It.IsAny<Expression<Func<AdminEmailConfirmationToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AdminEmailConfirmationToken?)null);

        // Act
        var result = await _sut.GetEntityByToken(tokenString);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task NewToken_NoExistingToken_CreatesNewToken()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var newToken = TestDataBuilder.AdminEmailConfirmationTokens.Build(adminId);

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<AdminEmailConfirmationToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AdminEmailConfirmationToken?)null);

        _mockFactory.Setup(f => f.Create(It.IsAny<object[]>()))
            .Returns(newToken);

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<AdminEmailConfirmationToken>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newToken);

        // Act
        var result = await _sut.NewToken(adminId);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(newToken);

        _mockFactory.Verify(f => f.Create(It.IsAny<object[]>()), Times.Once);
        _mockRepository.Verify(r => r.AddAsync(newToken, default), Times.Once);
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<AdminEmailConfirmationToken>(), default), Times.Never);
    }

    [Fact]
    public async Task NewToken_ExistingExpiredToken_DeletesOldAndCreatesNew()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var existingToken = TestDataBuilder.AdminEmailConfirmationTokens.BuildExpired(adminId);
        var newToken = TestDataBuilder.AdminEmailConfirmationTokens.Build(adminId);

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<AdminEmailConfirmationToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingToken);

        _mockFactory.Setup(f => f.Create(It.IsAny<object[]>()))
            .Returns(newToken);

        _mockRepository.Setup(r => r.DeleteAsync(It.IsAny<AdminEmailConfirmationToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<AdminEmailConfirmationToken>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newToken);

        // Act
        var result = await _sut.NewToken(adminId);

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
        var adminId = Guid.NewGuid();
        var existingToken = TestDataBuilder.AdminEmailConfirmationTokens.BuildValid(adminId);
        var newToken = TestDataBuilder.AdminEmailConfirmationTokens.Build(adminId);

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<AdminEmailConfirmationToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingToken);

        _mockFactory.Setup(f => f.Create(It.IsAny<object[]>()))
            .Returns(newToken);

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<AdminEmailConfirmationToken>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newToken);

        // Act
        var result = await _sut.NewToken(adminId);

        // Assert
        result.Should().NotBeNull();

        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<AdminEmailConfirmationToken>(), default), Times.Never);
        _mockRepository.Verify(r => r.AddAsync(newToken, default), Times.Once);
    }

    [Fact]
    public async Task Consume_ValidToken_ReturnsTrue()
    {
        // Arrange
        var tokenString = "valid-token";
        var token = TestDataBuilder.AdminEmailConfirmationTokens.BuildValid();
        token.Token = tokenString;

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<AdminEmailConfirmationToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<AdminEmailConfirmationToken>(), It.IsAny<CancellationToken>()))
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
                It.IsAny<Expression<Func<AdminEmailConfirmationToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AdminEmailConfirmationToken?)null);

        // Act
        var result = await _sut.Consume(tokenString);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<AdminEmailConfirmationToken>(), default), Times.Never);
    }

    [Fact]
    public async Task Consume_AlreadyConsumedToken_ReturnsFalse()
    {
        // Arrange
        var tokenString = "consumed-token";
        var token = TestDataBuilder.AdminEmailConfirmationTokens.BuildConsumed();
        token.Token = tokenString;

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<AdminEmailConfirmationToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        // Act
        var result = await _sut.Consume(tokenString);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<AdminEmailConfirmationToken>(), default), Times.Never);
    }

    [Fact]
    public async Task Consume_ExpiredToken_ReturnsFalse()
    {
        // Arrange
        var tokenString = "expired-token";
        var token = TestDataBuilder.AdminEmailConfirmationTokens.BuildExpired();
        token.Token = tokenString;

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<AdminEmailConfirmationToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        // Act
        var result = await _sut.Consume(tokenString);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<AdminEmailConfirmationToken>(), default), Times.Never);
    }
}
