using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Simply.Auth.Core.Abstractions;
using Simply.Auth.Core.Enums;
using api.CZ.Features.PasswordHistories.Models;
using api.CZ.Features.PasswordHistories.Repositories;
using api.CZ.Features.PasswordHistories.Services;
using api.CZ.Features.PasswordsInfos.Models;
using api.CZ.Features.PasswordsInfos.Repositories;
using api.CZ.Features.Users.Repositories;
using api.Tests.Builders;
using System.Linq.Expressions;

namespace api.Tests.Unit.Services;

public class PasswordHistoryManagerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IPasswordsInfoRepository> _mockPasswordsInfoRepository;
    private readonly Mock<IPasswordHistoryRepository> _mockPasswordHistoryRepository;
    private readonly Mock<ISimplyAuthService> _mockAuthService;
    private readonly Mock<ILogger<PasswordHistoryManager>> _mockLogger;
    private readonly PasswordHistoryManager _sut;

    public PasswordHistoryManagerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockPasswordsInfoRepository = new Mock<IPasswordsInfoRepository>();
        _mockPasswordHistoryRepository = new Mock<IPasswordHistoryRepository>();
        _mockAuthService = new Mock<ISimplyAuthService>();
        _mockLogger = new Mock<ILogger<PasswordHistoryManager>>();

        _sut = new PasswordHistoryManager(
            _mockUserRepository.Object,
            _mockPasswordsInfoRepository.Object,
            _mockPasswordHistoryRepository.Object,
            _mockAuthService.Object,
            _mockLogger.Object);
    }

    private static PasswordHistory BuildHistory(Guid passwordsInfoId, DateTime changedAt, string hash = "hash")
    {
        return new PasswordHistory
        {
            Id = Guid.NewGuid(),
            PasswordHash = hash,
            ChangedAt = changedAt,
            IdPasswordsInfos = passwordsInfoId,
            CreationTime = changedAt
        };
    }

    [Fact]
    public async Task IsPasswordReusedAsync_UserNotFound_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserRepository.Setup(r => r.FindAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((api.CZ.Features.Users.Models.User?)null);

        // Act
        var result = await _sut.IsPasswordReusedAsync(userId, "new-password");

        // Assert
        result.Should().BeFalse();
        _mockPasswordHistoryRepository.Verify(
            r => r.ListAsync(It.IsAny<Expression<Func<PasswordHistory, bool>>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task IsPasswordReusedAsync_UserHasNoPasswordsInfo_ReturnsFalse()
    {
        // Arrange
        var user = TestDataBuilder.Users.Build(u => u.IdPasswordsInfos = null);
        _mockUserRepository.Setup(r => r.FindAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.IsPasswordReusedAsync(user.Id, "new-password");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsPasswordReusedAsync_NoHistoryEntries_ReturnsFalse()
    {
        // Arrange
        var passwordsInfoId = Guid.NewGuid();
        var user = TestDataBuilder.Users.Build(u => u.IdPasswordsInfos = passwordsInfoId);
        _mockUserRepository.Setup(r => r.FindAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockPasswordHistoryRepository.Setup(r => r.ListAsync(
                It.IsAny<Expression<Func<PasswordHistory, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PasswordHistory>());

        // Act
        var result = await _sut.IsPasswordReusedAsync(user.Id, "new-password");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsPasswordReusedAsync_PasswordMatchesRecentHistory_ReturnsTrue()
    {
        // Arrange
        var passwordsInfoId = Guid.NewGuid();
        var user = TestDataBuilder.Users.Build(u => u.IdPasswordsInfos = passwordsInfoId);
        var history = BuildHistory(passwordsInfoId, DateTime.UtcNow, "old-hash");

        _mockUserRepository.Setup(r => r.FindAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockPasswordHistoryRepository.Setup(r => r.ListAsync(
                It.IsAny<Expression<Func<PasswordHistory, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PasswordHistory> { history });
        _mockAuthService.Setup(a => a.VerifyPassword("new-password", "old-hash"))
            .Returns(SimplyVerificationResult.Success);

        // Act
        var result = await _sut.IsPasswordReusedAsync(user.Id, "new-password");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsPasswordReusedAsync_PasswordMatchesWithRehashNeeded_StillReturnsTrue()
    {
        // Arrange
        var passwordsInfoId = Guid.NewGuid();
        var user = TestDataBuilder.Users.Build(u => u.IdPasswordsInfos = passwordsInfoId);
        var history = BuildHistory(passwordsInfoId, DateTime.UtcNow, "old-hash");

        _mockUserRepository.Setup(r => r.FindAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockPasswordHistoryRepository.Setup(r => r.ListAsync(
                It.IsAny<Expression<Func<PasswordHistory, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PasswordHistory> { history });
        _mockAuthService.Setup(a => a.VerifyPassword("new-password", "old-hash"))
            .Returns(SimplyVerificationResult.SuccessRehashNeeded);

        // Act
        var result = await _sut.IsPasswordReusedAsync(user.Id, "new-password");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsPasswordReusedAsync_PasswordDoesNotMatchAnyHistory_ReturnsFalse()
    {
        // Arrange
        var passwordsInfoId = Guid.NewGuid();
        var user = TestDataBuilder.Users.Build(u => u.IdPasswordsInfos = passwordsInfoId);
        var history = BuildHistory(passwordsInfoId, DateTime.UtcNow, "old-hash");

        _mockUserRepository.Setup(r => r.FindAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockPasswordHistoryRepository.Setup(r => r.ListAsync(
                It.IsAny<Expression<Func<PasswordHistory, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PasswordHistory> { history });
        _mockAuthService.Setup(a => a.VerifyPassword("new-password", "old-hash"))
            .Returns(SimplyVerificationResult.Failed);

        // Act
        var result = await _sut.IsPasswordReusedAsync(user.Id, "new-password");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsPasswordReusedAsync_OnlyChecksFiveMostRecentEntries()
    {
        // Arrange
        var passwordsInfoId = Guid.NewGuid();
        var user = TestDataBuilder.Users.Build(u => u.IdPasswordsInfos = passwordsInfoId);
        var now = DateTime.UtcNow;

        // 6 histories: the oldest one matches, but only the 5 most recent should be checked
        var histories = new List<PasswordHistory>
        {
            BuildHistory(passwordsInfoId, now.AddDays(-1), "hash-1"),
            BuildHistory(passwordsInfoId, now.AddDays(-2), "hash-2"),
            BuildHistory(passwordsInfoId, now.AddDays(-3), "hash-3"),
            BuildHistory(passwordsInfoId, now.AddDays(-4), "hash-4"),
            BuildHistory(passwordsInfoId, now.AddDays(-5), "hash-5"),
            BuildHistory(passwordsInfoId, now.AddDays(-6), "oldest-matching-hash"),
        };

        _mockUserRepository.Setup(r => r.FindAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockPasswordHistoryRepository.Setup(r => r.ListAsync(
                It.IsAny<Expression<Func<PasswordHistory, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(histories);
        _mockAuthService.Setup(a => a.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(SimplyVerificationResult.Failed);
        _mockAuthService.Setup(a => a.VerifyPassword("new-password", "oldest-matching-hash"))
            .Returns(SimplyVerificationResult.Success);

        // Act
        var result = await _sut.IsPasswordReusedAsync(user.Id, "new-password");

        // Assert
        result.Should().BeFalse();
        _mockAuthService.Verify(a => a.VerifyPassword("new-password", "oldest-matching-hash"), Times.Never);
    }

    [Fact]
    public async Task AddPasswordToHistoryAsync_UserNotFound_DoesNotAddHistory()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserRepository.Setup(r => r.FindAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((api.CZ.Features.Users.Models.User?)null);

        // Act
        await _sut.AddPasswordToHistoryAsync(userId, "new-hash");

        // Assert
        _mockPasswordHistoryRepository.Verify(
            r => r.AddAsync(It.IsAny<PasswordHistory>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddPasswordToHistoryAsync_UserWithExistingPasswordsInfo_AddsHistoryEntry()
    {
        // Arrange
        var passwordsInfoId = Guid.NewGuid();
        var user = TestDataBuilder.Users.Build(u => u.IdPasswordsInfos = passwordsInfoId);
        var existingInfo = new PasswordsInfo { Id = passwordsInfoId, CreationTime = DateTime.UtcNow };

        _mockUserRepository.Setup(r => r.FindAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockPasswordsInfoRepository.Setup(r => r.FindAsync(passwordsInfoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingInfo);
        _mockPasswordHistoryRepository.Setup(r => r.ListAsync(
                It.IsAny<Expression<Func<PasswordHistory, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PasswordHistory>());

        PasswordHistory? addedHistory = null;
        _mockPasswordHistoryRepository
            .Setup(r => r.AddAsync(It.IsAny<PasswordHistory>(), It.IsAny<CancellationToken>()))
            .Callback<PasswordHistory, CancellationToken>((h, _) => addedHistory = h)
            .ReturnsAsync((PasswordHistory h, CancellationToken _) => h);

        // Act
        await _sut.AddPasswordToHistoryAsync(user.Id, "new-hash");

        // Assert
        addedHistory.Should().NotBeNull();
        addedHistory!.PasswordHash.Should().Be("new-hash");
        addedHistory.IdPasswordsInfos.Should().Be(passwordsInfoId);
    }

    [Fact]
    public async Task AddPasswordToHistoryAsync_MoreThanFiveHistories_SoftDeletesOldestBeyondLimit()
    {
        // Arrange
        var passwordsInfoId = Guid.NewGuid();
        var user = TestDataBuilder.Users.Build(u => u.IdPasswordsInfos = passwordsInfoId);
        var existingInfo = new PasswordsInfo { Id = passwordsInfoId, CreationTime = DateTime.UtcNow };
        var now = DateTime.UtcNow;

        var histories = Enumerable.Range(0, 5)
            .Select(i => BuildHistory(passwordsInfoId, now.AddDays(-i)))
            .ToList();

        _mockUserRepository.Setup(r => r.FindAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockPasswordsInfoRepository.Setup(r => r.FindAsync(passwordsInfoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingInfo);
        _mockPasswordHistoryRepository.Setup(r => r.AddAsync(It.IsAny<PasswordHistory>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PasswordHistory h, CancellationToken _) => h);
        // After adding the new password, the cleanup step lists histories again: now there are 6 (5 old + 1 new)
        _mockPasswordHistoryRepository.Setup(r => r.ListAsync(
                It.IsAny<Expression<Func<PasswordHistory, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => histories.Concat(new[] { BuildHistory(passwordsInfoId, now.AddDays(1), "new-hash") }).ToList());

        var deletedHistories = new List<PasswordHistory>();
        _mockPasswordHistoryRepository
            .Setup(r => r.SoftDeleteAsync(It.IsAny<PasswordHistory>(), It.IsAny<CancellationToken>()))
            .Callback<PasswordHistory, CancellationToken>((h, _) => deletedHistories.Add(h))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.AddPasswordToHistoryAsync(user.Id, "new-hash");

        // Assert: 6 total, max 5 kept -> exactly 1 soft-deleted (the oldest)
        deletedHistories.Should().HaveCount(1);
        deletedHistories[0].ChangedAt.Should().Be(histories.OrderByDescending(h => h.ChangedAt).Last().ChangedAt);
    }

    [Fact]
    public async Task EnsurePasswordsInfoExistsAsync_UserNotFound_DoesNothing()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserRepository.Setup(r => r.FindAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((api.CZ.Features.Users.Models.User?)null);

        // Act
        await _sut.EnsurePasswordsInfoExistsAsync(userId);

        // Assert
        _mockPasswordsInfoRepository.Verify(
            r => r.AddAsync(It.IsAny<PasswordsInfo>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EnsurePasswordsInfoExistsAsync_UserHasValidExistingInfo_DoesNotCreateNew()
    {
        // Arrange
        var passwordsInfoId = Guid.NewGuid();
        var user = TestDataBuilder.Users.Build(u => u.IdPasswordsInfos = passwordsInfoId);
        var existingInfo = new PasswordsInfo { Id = passwordsInfoId, CreationTime = DateTime.UtcNow };

        _mockUserRepository.Setup(r => r.FindAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockPasswordsInfoRepository.Setup(r => r.FindAsync(passwordsInfoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingInfo);

        // Act
        await _sut.EnsurePasswordsInfoExistsAsync(user.Id);

        // Assert
        _mockPasswordsInfoRepository.Verify(
            r => r.AddAsync(It.IsAny<PasswordsInfo>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<api.CZ.Features.Users.Models.User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EnsurePasswordsInfoExistsAsync_ExistingInfoWasSoftDeleted_CreatesNewOne()
    {
        // Arrange
        var oldPasswordsInfoId = Guid.NewGuid();
        var user = TestDataBuilder.Users.Build(u => u.IdPasswordsInfos = oldPasswordsInfoId);
        var deletedInfo = new PasswordsInfo
        {
            Id = oldPasswordsInfoId,
            CreationTime = DateTime.UtcNow,
            DeletionTime = DateTime.UtcNow
        };

        _mockUserRepository.Setup(r => r.FindAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockPasswordsInfoRepository.Setup(r => r.FindAsync(oldPasswordsInfoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(deletedInfo);
        _mockPasswordsInfoRepository
            .Setup(r => r.AddAsync(It.IsAny<PasswordsInfo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PasswordsInfo p, CancellationToken _) => p);

        // Act
        await _sut.EnsurePasswordsInfoExistsAsync(user.Id);

        // Assert
        _mockPasswordsInfoRepository.Verify(
            r => r.AddAsync(It.Is<PasswordsInfo>(p => p.Id != oldPasswordsInfoId), It.IsAny<CancellationToken>()), Times.Once);
        _mockUserRepository.Verify(r => r.UpdateAsync(
            It.Is<api.CZ.Features.Users.Models.User>(u => u.IdPasswordsInfos != oldPasswordsInfoId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnsurePasswordsInfoExistsAsync_UserHasNoPasswordsInfo_CreatesNewOneAndLinksUser()
    {
        // Arrange
        var user = TestDataBuilder.Users.Build(u => u.IdPasswordsInfos = null);

        _mockUserRepository.Setup(r => r.FindAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockPasswordsInfoRepository
            .Setup(r => r.AddAsync(It.IsAny<PasswordsInfo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PasswordsInfo p, CancellationToken _) => p);

        api.CZ.Features.Users.Models.User? updatedUser = null;
        _mockUserRepository
            .Setup(r => r.UpdateAsync(It.IsAny<api.CZ.Features.Users.Models.User>(), It.IsAny<CancellationToken>()))
            .Callback<api.CZ.Features.Users.Models.User, CancellationToken>((u, _) => updatedUser = u)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.EnsurePasswordsInfoExistsAsync(user.Id);

        // Assert
        _mockPasswordsInfoRepository.Verify(
            r => r.AddAsync(It.IsAny<PasswordsInfo>(), It.IsAny<CancellationToken>()), Times.Once);
        updatedUser.Should().NotBeNull();
        updatedUser!.IdPasswordsInfos.Should().NotBeNull();
    }
}
