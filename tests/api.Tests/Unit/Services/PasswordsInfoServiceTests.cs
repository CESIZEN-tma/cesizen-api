using FluentAssertions;
using Moq;
using api.CZ.Features.PasswordsInfos.DTOs;
using api.CZ.Features.PasswordsInfos.Models;
using api.CZ.Features.PasswordsInfos.Repositories;
using api.CZ.Features.PasswordsInfos.Services;
using System.Linq.Expressions;

namespace api.Tests.Unit.Services;

public class PasswordsInfoServiceTests
{
    private readonly Mock<IPasswordsInfoRepository> _mockRepository;
    private readonly PasswordsInfoService _sut;

    public PasswordsInfoServiceTests()
    {
        _mockRepository = new Mock<IPasswordsInfoRepository>();
        _sut = new PasswordsInfoService(_mockRepository.Object);
    }

    private static PasswordsInfo BuildInfo(Guid? id = null)
    {
        return new PasswordsInfo
        {
            Id = id ?? Guid.NewGuid(),
            AttemptCount = 0,
            LastLogin = DateTime.UtcNow,
            LastReset = DateTime.UtcNow,
            CreationTime = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedDtos()
    {
        // Arrange
        var infos = new List<PasswordsInfo> { BuildInfo(), BuildInfo() };
        _mockRepository.Setup(r => r.ListAsync(
                It.IsAny<Expression<Func<PasswordsInfo, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(infos);

        // Act
        var result = (await _sut.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingInfo_ReturnsDto()
    {
        // Arrange
        var info = BuildInfo();
        _mockRepository.Setup(r => r.FindAsync(info.Id, It.IsAny<CancellationToken>())).ReturnsAsync(info);

        // Act
        var result = await _sut.GetByIdAsync(info.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(info.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentInfo_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.FindAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((PasswordsInfo?)null);

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ValidDto_CreatesInfoWithCurrentTimestamps()
    {
        // Arrange
        var dto = new CreatePasswordsInfoDto { AttemptCount = 2 };

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result!.AttemptCount.Should().Be(2);
    }

    [Fact]
    public async Task UpdateAsync_ExistingInfoWithProvidedDates_UpdatesAllFields()
    {
        // Arrange
        var info = BuildInfo();
        var lastLogin = DateTime.UtcNow.AddDays(-1);
        var lastReset = DateTime.UtcNow.AddDays(-2);
        var dto = new UpdatePasswordsInfoDto { AttemptCount = 3, LastLogin = lastLogin, LastReset = lastReset };
        _mockRepository.Setup(r => r.FindAsync(info.Id, It.IsAny<CancellationToken>())).ReturnsAsync(info);

        // Act
        var result = await _sut.UpdateAsync(info.Id, dto);

        // Assert
        result.Should().NotBeNull();
        info.AttemptCount.Should().Be(3);
        info.LastLogin.Should().Be(lastLogin);
        info.LastReset.Should().Be(lastReset);
        info.UpdateTime.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_NoDatesProvided_KeepsExistingLastLoginAndLastReset()
    {
        // Arrange
        var info = BuildInfo();
        var originalLastLogin = info.LastLogin;
        var originalLastReset = info.LastReset;
        var dto = new UpdatePasswordsInfoDto { AttemptCount = 5 };
        _mockRepository.Setup(r => r.FindAsync(info.Id, It.IsAny<CancellationToken>())).ReturnsAsync(info);

        // Act
        await _sut.UpdateAsync(info.Id, dto);

        // Assert
        info.LastLogin.Should().Be(originalLastLogin);
        info.LastReset.Should().Be(originalLastReset);
        info.AttemptCount.Should().Be(5);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentInfo_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.FindAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((PasswordsInfo?)null);

        // Act
        var result = await _sut.UpdateAsync(id, new UpdatePasswordsInfoDto { AttemptCount = 1 });

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ExistingInfo_SoftDeletesAndReturnsTrue()
    {
        // Arrange
        var info = BuildInfo();
        _mockRepository.Setup(r => r.FindAsync(info.Id, It.IsAny<CancellationToken>())).ReturnsAsync(info);

        // Act
        var result = await _sut.DeleteAsync(info.Id);

        // Assert
        result.Should().BeTrue();
        info.DeletionTime.Should().NotBeNull();
        _mockRepository.Verify(r => r.SoftDeleteAsync(info, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentInfo_ReturnsFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.FindAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((PasswordsInfo?)null);

        // Act
        var result = await _sut.DeleteAsync(id);

        // Assert
        result.Should().BeFalse();
    }
}
