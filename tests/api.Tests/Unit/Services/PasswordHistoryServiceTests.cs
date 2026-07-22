using FluentAssertions;
using Moq;
using api.CZ.Features.PasswordHistories.DTOs;
using api.CZ.Features.PasswordHistories.Models;
using api.CZ.Features.PasswordHistories.Repositories;
using api.CZ.Features.PasswordHistories.Services;
using System.Linq.Expressions;

namespace api.Tests.Unit.Services;

public class PasswordHistoryServiceTests
{
    private readonly Mock<IPasswordHistoryRepository> _mockRepository;
    private readonly PasswordHistoryService _sut;

    public PasswordHistoryServiceTests()
    {
        _mockRepository = new Mock<IPasswordHistoryRepository>();
        _sut = new PasswordHistoryService(_mockRepository.Object);
    }

    private static PasswordHistory BuildHistory(Guid? passwordsInfoId = null, DateTime? changedAt = null)
    {
        return new PasswordHistory
        {
            Id = Guid.NewGuid(),
            PasswordHash = "hash",
            ChangedAt = changedAt ?? DateTime.UtcNow,
            IdPasswordsInfos = passwordsInfoId ?? Guid.NewGuid(),
            CreationTime = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedDtos()
    {
        // Arrange
        var histories = new List<PasswordHistory> { BuildHistory(), BuildHistory() };
        _mockRepository.Setup(r => r.ListAsync(
                It.IsAny<Expression<Func<PasswordHistory, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(histories);

        // Act
        var result = (await _sut.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Select(r => r.Id).Should().BeEquivalentTo(histories.Select(h => h.Id));
    }

    [Fact]
    public async Task GetByPasswordInfoIdAsync_ReturnsHistoriesOrderedByChangedAtDescending()
    {
        // Arrange
        var passwordsInfoId = Guid.NewGuid();
        var older = BuildHistory(passwordsInfoId, DateTime.UtcNow.AddDays(-2));
        var newer = BuildHistory(passwordsInfoId, DateTime.UtcNow);

        _mockRepository.Setup(r => r.ListAsync(
                It.IsAny<Expression<Func<PasswordHistory, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PasswordHistory> { older, newer });

        // Act
        var result = (await _sut.GetByPasswordInfoIdAsync(passwordsInfoId)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].Id.Should().Be(newer.Id);
        result[1].Id.Should().Be(older.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingNonDeletedHistory_ReturnsDto()
    {
        // Arrange
        var history = BuildHistory();
        _mockRepository.Setup(r => r.FindAsync(history.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);

        // Act
        var result = await _sut.GetByIdAsync(history.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(history.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentHistory_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.FindAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PasswordHistory?)null);

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_SoftDeletedHistory_ReturnsNull()
    {
        // Arrange
        var history = BuildHistory();
        history.DeletionTime = DateTime.UtcNow;
        _mockRepository.Setup(r => r.FindAsync(history.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);

        // Act
        var result = await _sut.GetByIdAsync(history.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ValidDto_AddsHistoryAndReturnsDto()
    {
        // Arrange
        var dto = new CreatePasswordHistoryDto
        {
            PasswordHash = "new-hash",
            IdPasswordsInfos = Guid.NewGuid()
        };

        PasswordHistory? added = null;
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<PasswordHistory>(), It.IsAny<CancellationToken>()))
            .Callback<PasswordHistory, CancellationToken>((h, _) => added = h)
            .ReturnsAsync((PasswordHistory h, CancellationToken _) => h);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result!.IdPasswordsInfos.Should().Be(dto.IdPasswordsInfos);
        added.Should().NotBeNull();
        added!.PasswordHash.Should().Be("new-hash");
    }

    [Fact]
    public async Task DeleteAsync_ExistingNonDeletedHistory_SoftDeletesAndReturnsTrue()
    {
        // Arrange
        var history = BuildHistory();
        _mockRepository.Setup(r => r.FindAsync(history.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);
        _mockRepository.Setup(r => r.SoftDeleteAsync(history, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.DeleteAsync(history.Id);

        // Assert
        result.Should().BeTrue();
        history.DeletionTime.Should().NotBeNull();
        _mockRepository.Verify(r => r.SoftDeleteAsync(history, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentHistory_ReturnsFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.FindAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PasswordHistory?)null);

        // Act
        var result = await _sut.DeleteAsync(id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_AlreadyDeletedHistory_ReturnsFalse()
    {
        // Arrange
        var history = BuildHistory();
        history.DeletionTime = DateTime.UtcNow;
        _mockRepository.Setup(r => r.FindAsync(history.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);

        // Act
        var result = await _sut.DeleteAsync(history.Id);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.SoftDeleteAsync(It.IsAny<PasswordHistory>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
