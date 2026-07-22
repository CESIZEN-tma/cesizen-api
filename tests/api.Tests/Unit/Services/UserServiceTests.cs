using FluentAssertions;
using Moq;
using api.CZ.Features.AdminLogs.Enums;
using api.CZ.Features.AdminLogs.Services;
using api.CZ.Features.Users.DTOs;
using api.CZ.Features.Users.Models;
using api.CZ.Features.Users.Repositories;
using api.CZ.Features.Users.Services;
using api.Tests.Builders;
using System.Linq.Expressions;

namespace api.Tests.Unit.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockRepository;
    private readonly Mock<IAdminActionLogger> _mockActionLogger;
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _mockRepository = new Mock<IUserRepository>();
        _mockActionLogger = new Mock<IAdminActionLogger>();
        _sut = new UserService(_mockRepository.Object, _mockActionLogger.Object);
    }

    [Fact]
    public async Task GetProfileAsync_ExistingActiveUser_ReturnsProfileDto()
    {
        // Arrange
        var user = TestDataBuilder.Users.Build();
        _mockRepository.Setup(r => r.FindAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.GetProfileAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task GetProfileAsync_NonExistentUser_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.FindAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.GetProfileAsync(id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProfileAsync_SoftDeletedUser_ReturnsNull()
    {
        // Arrange
        var user = TestDataBuilder.Users.Build(u => u.DeletionTime = DateTime.UtcNow);
        _mockRepository.Setup(r => r.FindAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.GetProfileAsync(user.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateProfileAsync_ExistingUser_UpdatesAndReturnsDto()
    {
        // Arrange
        var user = TestDataBuilder.Users.Build();
        var dto = new UpdateUserProfileDto
        {
            FirstName = "Updated",
            LastName = "Name",
            ThumbnailUrl = "https://example.com/avatar.png"
        };

        _mockRepository.Setup(r => r.FindAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.UpdateProfileAsync(user.Id, dto);

        // Assert
        result.Should().NotBeNull();
        user.FirstName.Should().Be("Updated");
        user.LastName.Should().Be("Name");
        user.ThumbnailUrl.Should().Be(dto.ThumbnailUrl);
        user.UpdateTime.Should().NotBeNull();
        _mockRepository.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_NonExistentUser_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.FindAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.UpdateProfileAsync(id, new UpdateUserProfileDto { FirstName = "A", LastName = "B" });

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAccountAsync_ExistingUser_SoftDeletesAndDeactivates()
    {
        // Arrange
        var user = TestDataBuilder.Users.Build(u => u.Active = true);
        _mockRepository.Setup(r => r.FindAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.DeleteAccountAsync(user.Id);

        // Assert
        result.Should().BeTrue();
        user.DeletionTime.Should().NotBeNull();
        user.Active.Should().BeFalse();
        _mockRepository.Verify(r => r.SoftDeleteAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAccountAsync_NonExistentUser_ReturnsFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.FindAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.DeleteAccountAsync(id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAccountAsync_AlreadyDeletedUser_ReturnsFalse()
    {
        // Arrange
        var user = TestDataBuilder.Users.Build(u => u.DeletionTime = DateTime.UtcNow);
        _mockRepository.Setup(r => r.FindAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.DeleteAccountAsync(user.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllForAdminAsync_ReturnsMappedAdminDtos()
    {
        // Arrange
        var users = TestDataBuilder.Users.BuildMany(3);
        _mockRepository.Setup(r => r.ListAsync(
                It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act
        var result = (await _sut.GetAllForAdminAsync()).ToList();

        // Assert
        result.Should().HaveCount(3);
        result.Select(r => r.Id).Should().BeEquivalentTo(users.Select(u => u.Id));
    }

    [Fact]
    public async Task UpdateUserStatusAsync_EnablingDisabledUser_UpdatesAndLogsEnabled()
    {
        // Arrange
        var user = TestDataBuilder.Users.Build(u => u.Active = false);
        var adminId = Guid.NewGuid();

        _mockRepository.Setup(r => r.FindAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.UpdateUserStatusAsync(user.Id, true, adminId);

        // Assert
        result.Should().BeTrue();
        user.Active.Should().BeTrue();
        _mockRepository.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _mockActionLogger.Verify(l => l.LogCustomActionAsync(
            adminId, AdminActionCode.USER_ENABLED, "User", user.Id, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserStatusAsync_DisablingActiveUser_UpdatesAndLogsDisabled()
    {
        // Arrange
        var user = TestDataBuilder.Users.Build(u => u.Active = true);
        var adminId = Guid.NewGuid();

        _mockRepository.Setup(r => r.FindAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.UpdateUserStatusAsync(user.Id, false, adminId);

        // Assert
        result.Should().BeTrue();
        user.Active.Should().BeFalse();
        _mockActionLogger.Verify(l => l.LogCustomActionAsync(
            adminId, AdminActionCode.USER_DISABLED, "User", user.Id, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserStatusAsync_NoStatusChange_ReturnsTrueWithoutUpdatingOrLogging()
    {
        // Arrange
        var user = TestDataBuilder.Users.Build(u => u.Active = true);
        _mockRepository.Setup(r => r.FindAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.UpdateUserStatusAsync(user.Id, true, Guid.NewGuid());

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockActionLogger.Verify(l => l.LogCustomActionAsync(
            It.IsAny<Guid>(), It.IsAny<AdminActionCode>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateUserStatusAsync_NonExistentUser_ReturnsFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.FindAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.UpdateUserStatusAsync(id, true, Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }
}
