using FluentAssertions;
using Moq;
using api.CZ.Features.AdminLogs.Services;
using api.CZ.Features.Configurations.DTOs;
using api.CZ.Features.Configurations.Models;
using api.CZ.Features.Configurations.Repositories;
using api.CZ.Features.Configurations.Services;
using System.Linq.Expressions;

namespace api.Tests.Unit.Services;

public class ConfigurationServiceTests
{
    private readonly Mock<IConfigurationRepository> _mockRepository;
    private readonly Mock<IAdminActionLogger> _mockActionLogger;
    private readonly ConfigurationService _sut;

    public ConfigurationServiceTests()
    {
        _mockRepository = new Mock<IConfigurationRepository>();
        _mockActionLogger = new Mock<IAdminActionLogger>();
        _sut = new ConfigurationService(_mockRepository.Object, _mockActionLogger.Object);
    }

    private static Configuration BuildConfig(Guid? id = null, Guid? adminId = null)
    {
        return new Configuration
        {
            Id = id ?? Guid.NewGuid(),
            Name = "Config",
            Inhalation = 4,
            Retention1 = 4,
            Exhalation = 4,
            Retention2 = 4,
            DurationMinutes = 5,
            Difficulty = 1,
            Objective = "Relaxation",
            GuidanceType = "Visual",
            IdAdministrators = adminId ?? Guid.NewGuid(),
            CreationTime = DateTime.UtcNow
        };
    }

    private static CreateConfigurationDto BuildCreateDto()
    {
        return new CreateConfigurationDto
        {
            Name = "New Config",
            Inhalation = 4,
            Retention1 = 4,
            Exhalation = 4,
            Retention2 = 4,
            DurationMinutes = 5,
            Difficulty = 1,
            Objective = "Relaxation",
            GuidanceType = "Visual"
        };
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedDtos()
    {
        // Arrange
        var configs = new List<Configuration> { BuildConfig(), BuildConfig() };
        _mockRepository.Setup(r => r.ListAsync(
                It.IsAny<Expression<Func<Configuration, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(configs);

        // Act
        var result = (await _sut.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingConfig_ReturnsDto()
    {
        // Arrange
        var config = BuildConfig();
        _mockRepository.Setup(r => r.FindAsync(config.Id, It.IsAny<CancellationToken>())).ReturnsAsync(config);

        // Act
        var result = await _sut.GetByIdAsync(config.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(config.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentConfig_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.FindAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Configuration?)null);

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_SoftDeletedConfig_ReturnsNull()
    {
        // Arrange
        var config = BuildConfig();
        config.DeletionTime = DateTime.UtcNow;
        _mockRepository.Setup(r => r.FindAsync(config.Id, It.IsAny<CancellationToken>())).ReturnsAsync(config);

        // Act
        var result = await _sut.GetByIdAsync(config.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ValidDto_CreatesConfigAndLogsAction()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var dto = BuildCreateDto();

        // Act
        var result = await _sut.CreateAsync(dto, adminId);

        // Assert
        result.Should().NotBeNull();
        result!.IdAdministrators.Should().Be(adminId);
        _mockActionLogger.Verify(l => l.LogCreateAsync(adminId, "Configuration", result.Id, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ExistingConfig_UpdatesFieldsAndLogsAction()
    {
        // Arrange
        var config = BuildConfig();
        var adminId = Guid.NewGuid();
        var dto = new UpdateConfigurationDto
        {
            Name = "Updated",
            Inhalation = 6,
            Retention1 = 2,
            Exhalation = 6,
            Retention2 = 2,
            DurationMinutes = 10,
            Difficulty = 5,
            Objective = "Focus",
            GuidanceType = "Audio"
        };
        _mockRepository.Setup(r => r.FindAsync(config.Id, It.IsAny<CancellationToken>())).ReturnsAsync(config);

        // Act
        var result = await _sut.UpdateAsync(config.Id, dto, adminId);

        // Assert
        result.Should().NotBeNull();
        config.Name.Should().Be("Updated");
        _mockActionLogger.Verify(l => l.LogUpdateAsync(adminId, "Configuration", config.Id, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentConfig_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.FindAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Configuration?)null);

        // Act
        var result = await _sut.UpdateAsync(id, new UpdateConfigurationDto
        {
            Name = "x", Objective = "x", GuidanceType = "x"
        }, Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ExistingConfig_SoftDeletesAndLogsAction()
    {
        // Arrange
        var config = BuildConfig();
        var adminId = Guid.NewGuid();
        _mockRepository.Setup(r => r.FindAsync(config.Id, It.IsAny<CancellationToken>())).ReturnsAsync(config);

        // Act
        var result = await _sut.DeleteAsync(config.Id, adminId);

        // Assert
        result.Should().BeTrue();
        config.DeletionTime.Should().NotBeNull();
        _mockActionLogger.Verify(l => l.LogDeleteAsync(adminId, "Configuration", config.Id, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentConfig_ReturnsFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.FindAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Configuration?)null);

        // Act
        var result = await _sut.DeleteAsync(id, Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }
}
