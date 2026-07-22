using FluentAssertions;
using Moq;
using api.CZ.Features.AdminLogs.Services;
using api.CZ.Features.Administrators.DTOs;
using api.CZ.Features.Administrators.Factories;
using api.CZ.Features.Administrators.Models;
using api.CZ.Features.Administrators.Repositories;
using api.CZ.Features.Administrators.Services;
using api.Tests.Builders;
using Simply.Auth.Core.Abstractions;
using System.Linq.Expressions;

namespace api.Tests.Unit.Services;

public class AdministratorServiceTests
{
    private readonly Mock<IAdministratorRepository> _mockRepository;
    private readonly Mock<IAdministratorFactory> _mockFactory;
    private readonly Mock<ISimplyAuthService> _mockAuthService;
    private readonly Mock<IAdminActionLogger> _mockActionLogger;
    private readonly AdministratorService _sut;

    public AdministratorServiceTests()
    {
        _mockRepository = new Mock<IAdministratorRepository>();
        _mockFactory = new Mock<IAdministratorFactory>();
        _mockAuthService = new Mock<ISimplyAuthService>();
        _mockActionLogger = new Mock<IAdminActionLogger>();

        _sut = new AdministratorService(
            _mockRepository.Object, _mockFactory.Object, _mockAuthService.Object, _mockActionLogger.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedDtosForNonDeletedAdmins()
    {
        // Arrange
        var admins = TestDataBuilder.Administrators.BuildMany(2);
        _mockRepository.Setup(r => r.ListAsync(
                It.IsAny<Expression<Func<Administrator, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(admins);

        // Act
        var result = (await _sut.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Select(r => r.Id).Should().BeEquivalentTo(admins.Select(a => a.Id));
    }

    [Fact]
    public async Task GetByIdAsync_ExistingAdmin_ReturnsDto()
    {
        // Arrange
        var admin = TestDataBuilder.Administrators.Build();
        _mockRepository.Setup(r => r.FindAsync(admin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        // Act
        var result = await _sut.GetByIdAsync(admin.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(admin.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentAdmin_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.FindAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Administrator?)null);

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_SoftDeletedAdmin_ReturnsNull()
    {
        // Arrange
        var admin = TestDataBuilder.Administrators.Build(a => a.DeletionTime = DateTime.UtcNow);
        _mockRepository.Setup(r => r.FindAsync(admin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        // Act
        var result = await _sut.GetByIdAsync(admin.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_NewEmail_CreatesAdminAndLogsAction()
    {
        // Arrange
        var creatorId = Guid.NewGuid();
        var dto = new CreateAdministratorDto
        {
            Email = "new.admin@example.com",
            Password = "Password1234!",
            FirstName = "Jean",
            LastName = "Martin"
        };
        var createdAdmin = TestDataBuilder.Administrators.Build(a =>
        {
            a.Email = dto.Email;
            a.FirstName = dto.FirstName;
            a.LastName = dto.LastName;
        });

        _mockRepository.Setup(r => r.AnyAsync(
                It.IsAny<Expression<Func<Administrator, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _mockAuthService.Setup(a => a.HashPassword(dto.Password)).Returns("hashed-password");
        _mockFactory.Setup(f => f.Create(It.IsAny<object[]>())).Returns(createdAdmin);

        // Act
        var result = await _sut.CreateAsync(dto, creatorId);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(dto.Email);
        _mockRepository.Verify(r => r.AddAsync(createdAdmin, It.IsAny<CancellationToken>()), Times.Once);
        _mockActionLogger.Verify(l => l.LogCreateAsync(
            creatorId, "Administrator", createdAdmin.Id, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_EmailAlreadyExists_ReturnsNullAndDoesNotCreate()
    {
        // Arrange
        var dto = new CreateAdministratorDto
        {
            Email = "existing@example.com",
            Password = "Password1234!",
            FirstName = "Jean",
            LastName = "Martin"
        };

        _mockRepository.Setup(r => r.AnyAsync(
                It.IsAny<Expression<Func<Administrator, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.CreateAsync(dto, Guid.NewGuid());

        // Assert
        result.Should().BeNull();
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Administrator>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockActionLogger.Verify(l => l.LogCreateAsync(
            It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ExistingAdmin_UpdatesFieldsAndLogsAction()
    {
        // Arrange
        var admin = TestDataBuilder.Administrators.Build();
        var adminId = Guid.NewGuid();
        var dto = new UpdateAdministratorDto
        {
            FirstName = "Updated",
            LastName = "Name",
            ThumbnailUrl = "https://example.com/avatar.png"
        };

        _mockRepository.Setup(r => r.FindAsync(admin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        // Act
        var result = await _sut.UpdateAsync(admin.Id, dto, adminId);

        // Assert
        result.Should().NotBeNull();
        admin.FirstName.Should().Be("Updated");
        admin.LastName.Should().Be("Name");
        admin.ThumbnailUrl.Should().Be(dto.ThumbnailUrl);
        admin.UpdateTime.Should().NotBeNull();
        _mockRepository.Verify(r => r.UpdateAsync(admin, It.IsAny<CancellationToken>()), Times.Once);
        _mockActionLogger.Verify(l => l.LogUpdateAsync(
            adminId, "Administrator", admin.Id, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentAdmin_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.FindAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Administrator?)null);

        // Act
        var result = await _sut.UpdateAsync(id, new UpdateAdministratorDto { FirstName = "A", LastName = "B" }, Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ExistingAdmin_SoftDeletesAndLogsAction()
    {
        // Arrange
        var admin = TestDataBuilder.Administrators.Build();
        var adminId = Guid.NewGuid();

        _mockRepository.Setup(r => r.FindAsync(admin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        // Act
        var result = await _sut.DeleteAsync(admin.Id, adminId);

        // Assert
        result.Should().BeTrue();
        admin.DeletionTime.Should().NotBeNull();
        _mockRepository.Verify(r => r.SoftDeleteAsync(admin, It.IsAny<CancellationToken>()), Times.Once);
        _mockActionLogger.Verify(l => l.LogDeleteAsync(
            adminId, "Administrator", admin.Id, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_SelfDeletion_ThrowsInvalidOperationException()
    {
        // Arrange
        var adminId = Guid.NewGuid();

        // Act
        var act = () => _sut.DeleteAsync(adminId, adminId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        _mockRepository.Verify(r => r.FindAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentAdmin_ReturnsFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.FindAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Administrator?)null);

        // Act
        var result = await _sut.DeleteAsync(id, Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
        _mockActionLogger.Verify(l => l.LogDeleteAsync(
            It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_AlreadyDeletedAdmin_ReturnsFalse()
    {
        // Arrange
        var admin = TestDataBuilder.Administrators.Build(a => a.DeletionTime = DateTime.UtcNow);
        _mockRepository.Setup(r => r.FindAsync(admin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        // Act
        var result = await _sut.DeleteAsync(admin.Id, Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }
}
