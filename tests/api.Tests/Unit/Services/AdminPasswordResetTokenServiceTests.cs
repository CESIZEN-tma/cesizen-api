using FluentAssertions;
using Moq;
using api.CZ.Features.AdminPasswordResetTokens.Factories;
using api.CZ.Features.AdminPasswordResetTokens.Models;
using api.CZ.Features.AdminPasswordResetTokens.Repositories;
using api.CZ.Features.AdminPasswordResetTokens.Services;
using api.Tests.Builders;
using System.Linq.Expressions;

namespace api.Tests.Unit.Services;

public class AdminPasswordResetTokenServiceTests
{
    private readonly Mock<IAdminPasswordResetTokenRepository> _mockRepository;
    private readonly Mock<IAdminPasswordResetTokenFactory> _mockFactory;
    private readonly AdminPasswordResetTokenService _sut;

    public AdminPasswordResetTokenServiceTests()
    {
        _mockRepository = new Mock<IAdminPasswordResetTokenRepository>();
        _mockFactory = new Mock<IAdminPasswordResetTokenFactory>();

        _sut = new AdminPasswordResetTokenService(_mockRepository.Object, _mockFactory.Object);
    }

    [Fact]
    public async Task GetEntityByToken_ValidUnconsumedToken_ReturnsToken()
    {
        // Arrange
        var tokenString = "valid-reset-token";
        var token = TestDataBuilder.AdminPasswordResetTokens.BuildValid();
        token.Token = tokenString;

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<AdminPasswordResetToken, bool>>>(),
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
        var token = TestDataBuilder.AdminPasswordResetTokens.BuildConsumed();
        token.Token = tokenString;

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<AdminPasswordResetToken, bool>>>(),
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
                It.IsAny<Expression<Func<AdminPasswordResetToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AdminPasswordResetToken?)null);

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
        var newToken = TestDataBuilder.AdminPasswordResetTokens.Build(adminId);

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<AdminPasswordResetToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AdminPasswordResetToken?)null);

        _mockFactory.Setup(f => f.Create(It.IsAny<object[]>()))
            .Returns(newToken);

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<AdminPasswordResetToken>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newToken);

        // Act
        var result = await _sut.NewToken(adminId);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(newToken);

        _mockFactory.Verify(f => f.Create(It.IsAny<object[]>()), Times.Once);
        _mockRepository.Verify(r => r.AddAsync(newToken, default), Times.Once);
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<AdminPasswordResetToken>(), default), Times.Never);
    }

    [Fact]
    public async Task NewToken_ExistingValidToken_DeletesOldAndCreatesNew()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var existingToken = TestDataBuilder.AdminPasswordResetTokens.BuildValid(adminId);
        var newToken = TestDataBuilder.AdminPasswordResetTokens.Build(adminId);

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<AdminPasswordResetToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingToken);

        _mockFactory.Setup(f => f.Create(It.IsAny<object[]>()))
            .Returns(newToken);

        _mockRepository.Setup(r => r.DeleteAsync(It.IsAny<AdminPasswordResetToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<AdminPasswordResetToken>(), It.IsAny<CancellationToken>()))
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
    public async Task NewToken_ExistingExpiredToken_CreatesNewWithoutDeletion()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var newToken = TestDataBuilder.AdminPasswordResetTokens.Build(adminId);

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<AdminPasswordResetToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AdminPasswordResetToken?)null); // Expired tokens won't be returned by the query

        _mockFactory.Setup(f => f.Create(It.IsAny<object[]>()))
            .Returns(newToken);

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<AdminPasswordResetToken>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newToken);

        // Act
        var result = await _sut.NewToken(adminId);

        // Assert
        result.Should().NotBeNull();

        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<AdminPasswordResetToken>(), default), Times.Never);
        _mockRepository.Verify(r => r.AddAsync(newToken, default), Times.Once);
    }

    [Fact]
    public async Task Consume_ValidToken_ReturnsTrue()
    {
        // Arrange
        var tokenString = "valid-reset-token";
        var token = TestDataBuilder.AdminPasswordResetTokens.BuildValid();
        token.Token = tokenString;

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<AdminPasswordResetToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<AdminPasswordResetToken>(), It.IsAny<CancellationToken>()))
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
                It.IsAny<Expression<Func<AdminPasswordResetToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AdminPasswordResetToken?)null);

        // Act
        var result = await _sut.Consume(tokenString);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<AdminPasswordResetToken>(), default), Times.Never);
    }

    [Fact]
    public async Task Consume_AlreadyConsumedToken_ReturnsFalse()
    {
        // Arrange
        var tokenString = "consumed-reset-token";
        var token = TestDataBuilder.AdminPasswordResetTokens.BuildConsumed();
        token.Token = tokenString;

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<AdminPasswordResetToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        // Act
        var result = await _sut.Consume(tokenString);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<AdminPasswordResetToken>(), default), Times.Never);
    }

    [Fact]
    public async Task Consume_ExpiredToken_ReturnsFalse()
    {
        // Arrange
        var tokenString = "expired-reset-token";
        var token = TestDataBuilder.AdminPasswordResetTokens.BuildExpired();
        token.Token = tokenString;

        _mockRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<AdminPasswordResetToken, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        // Act
        var result = await _sut.Consume(tokenString);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<AdminPasswordResetToken>(), default), Times.Never);
    }
}
