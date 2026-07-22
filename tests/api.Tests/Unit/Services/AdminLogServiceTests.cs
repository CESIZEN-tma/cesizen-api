using FluentAssertions;
using Moq;
using api.CZ.Features.AdminLogs.DTOs;
using api.CZ.Features.AdminLogs.Enums;
using api.CZ.Features.AdminLogs.Factories;
using api.CZ.Features.AdminLogs.Models;
using api.CZ.Features.AdminLogs.Repositories;
using api.CZ.Features.AdminLogs.Services;
using api.CZ.Features.Administrators.Models;
using api.Tests.Builders;

namespace api.Tests.Unit.Services;

public class AdminLogServiceTests
{
    private readonly Mock<IAdminLogRepository> _mockRepository;
    private readonly Mock<IAdminLogFactory> _mockFactory;
    private readonly AdminLogService _sut;

    public AdminLogServiceTests()
    {
        _mockRepository = new Mock<IAdminLogRepository>();
        _mockFactory = new Mock<IAdminLogFactory>();
        _sut = new AdminLogService(_mockRepository.Object, _mockFactory.Object);
    }

    private static AdminLog BuildLog(Administrator admin, AdminActionCode actionCode = AdminActionCode.ADMIN_CREATED)
    {
        return new AdminLog
        {
            Id = Guid.NewGuid(),
            ActionCode = actionCode.ToString(),
            EntityType = "Administrator",
            TargetedEntityId = Guid.NewGuid(),
            Description = "did something",
            CreationTime = DateTime.UtcNow,
            IdAdministrator = admin.Id,
            IdAdministratorNavigation = admin
        };
    }

    [Fact]
    public async Task LogActionAsync_CreatesLogViaFactoryAndAddsToRepository()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var createdLog = new AdminLog
        {
            Id = Guid.NewGuid(),
            ActionCode = AdminActionCode.ADMIN_CREATED.ToString(),
            EntityType = "Administrator",
            TargetedEntityId = entityId,
            Description = "created",
            CreationTime = DateTime.UtcNow,
            IdAdministrator = adminId
        };

        _mockFactory
            .Setup(f => f.Create(adminId, AdminActionCode.ADMIN_CREATED, "Administrator", entityId, "created"))
            .Returns(createdLog);

        // Act
        await _sut.LogActionAsync(adminId, AdminActionCode.ADMIN_CREATED, "Administrator", entityId, "created");

        // Assert
        _mockRepository.Verify(r => r.AddAsync(createdLog, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetFilteredLogsAsync_ReturnsMappedDtosWithAdministratorInfo()
    {
        // Arrange
        var admin = TestDataBuilder.Administrators.Build();
        var log = BuildLog(admin);
        var filter = new AdminLogFilterDto { AdministratorId = admin.Id };

        _mockRepository.Setup(r => r.GetFilteredLogsAsync(filter))
            .ReturnsAsync(new List<AdminLog> { log });

        // Act
        var result = (await _sut.GetFilteredLogsAsync(filter)).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(log.Id);
        result[0].AdministratorId.Should().Be(admin.Id);
        result[0].AdministratorEmail.Should().Be(admin.Email);
        result[0].AdministratorName.Should().Be($"{admin.FirstName} {admin.LastName}");
    }

    [Fact]
    public async Task GetRecentLogsAsync_PassesCountToRepositoryAndMapsResults()
    {
        // Arrange
        var admin = TestDataBuilder.Administrators.Build();
        var log = BuildLog(admin);

        _mockRepository.Setup(r => r.GetRecentLogsAsync(10))
            .ReturnsAsync(new List<AdminLog> { log });

        // Act
        var result = (await _sut.GetRecentLogsAsync(10)).ToList();

        // Assert
        result.Should().HaveCount(1);
        _mockRepository.Verify(r => r.GetRecentLogsAsync(10), Times.Once);
    }

    [Fact]
    public async Task GetLogsByAdministratorAsync_ReturnsMappedDtos()
    {
        // Arrange
        var admin = TestDataBuilder.Administrators.Build();
        var log = BuildLog(admin);

        _mockRepository.Setup(r => r.GetLogsByAdministratorAsync(admin.Id))
            .ReturnsAsync(new List<AdminLog> { log });

        // Act
        var result = (await _sut.GetLogsByAdministratorAsync(admin.Id)).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].AdministratorId.Should().Be(admin.Id);
    }

    [Fact]
    public async Task GetLogsByEntityAsync_ReturnsMappedDtos()
    {
        // Arrange
        var admin = TestDataBuilder.Administrators.Build();
        var entityId = Guid.NewGuid();
        var log = BuildLog(admin);
        log.EntityType = "Quiz";
        log.TargetedEntityId = entityId;

        _mockRepository.Setup(r => r.GetLogsByEntityAsync("Quiz", entityId))
            .ReturnsAsync(new List<AdminLog> { log });

        // Act
        var result = (await _sut.GetLogsByEntityAsync("Quiz", entityId)).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].EntityType.Should().Be("Quiz");
        result[0].TargetedEntityId.Should().Be(entityId);
    }
}
