using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using api.CZ.Features.Configurations;
using api.CZ.Features.Configurations.DTOs;
using api.CZ.Features.Configurations.Services;

namespace api.Tests.Unit.Features.Configurations;

public class ConfigurationControllerTests
{
    private readonly Mock<IConfigurationService> _mockService;
    private readonly ConfigurationController _controller;
    private readonly Guid _testAdminId;

    public ConfigurationControllerTests()
    {
        _mockService = new Mock<IConfigurationService>();
        var mockLogger = new Mock<ILogger<ConfigurationController>>();
        _controller = new ConfigurationController(_mockService.Object, mockLogger.Object);
        _testAdminId = Guid.NewGuid();

        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, _testAdminId.ToString()) }, "TestAuth");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    private static GetConfigurationDto BuildConfig(Guid? id = null)
    {
        return new GetConfigurationDto
        {
            Id = id ?? Guid.NewGuid(), Name = "Config", Objective = "Relaxation", GuidanceType = "Visual"
        };
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithConfigurations()
    {
        // Arrange
        _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<GetConfigurationDto> { BuildConfig() });

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ExistingConfig_ReturnsOk()
    {
        // Arrange
        var config = BuildConfig();
        _mockService.Setup(s => s.GetByIdAsync(config.Id)).ReturnsAsync(config);

        // Act
        var result = await _controller.GetById(config.Id);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_NonExistentConfig_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((GetConfigurationDto?)null);

        // Act
        var result = await _controller.GetById(id);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_ValidDto_ReturnsCreatedAtAction()
    {
        // Arrange
        var dto = new CreateConfigurationDto { Name = "New", Objective = "Relaxation", GuidanceType = "Visual" };
        var created = BuildConfig();
        _mockService.Setup(s => s.CreateAsync(dto, _testAdminId)).ReturnsAsync(created);

        // Act
        var result = await _controller.Create(dto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Create_ServiceReturnsNull_ReturnsBadRequest()
    {
        // Arrange
        var dto = new CreateConfigurationDto { Name = "New", Objective = "x", GuidanceType = "y" };
        _mockService.Setup(s => s.CreateAsync(dto, _testAdminId)).ReturnsAsync((GetConfigurationDto?)null);

        // Act
        var result = await _controller.Create(dto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ExistingConfig_ReturnsOk()
    {
        // Arrange
        var config = BuildConfig();
        var dto = new UpdateConfigurationDto { Name = "Updated", Objective = "Focus", GuidanceType = "Audio" };
        _mockService.Setup(s => s.UpdateAsync(config.Id, dto, _testAdminId)).ReturnsAsync(config);

        // Act
        var result = await _controller.Update(config.Id, dto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_NonExistentConfig_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateConfigurationDto { Name = "x", Objective = "y", GuidanceType = "z" };
        _mockService.Setup(s => s.UpdateAsync(id, dto, _testAdminId)).ReturnsAsync((GetConfigurationDto?)null);

        // Act
        var result = await _controller.Update(id, dto);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_ExistingConfig_ReturnsOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.DeleteAsync(id, _testAdminId)).ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(id);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_NonExistentConfig_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.DeleteAsync(id, _testAdminId)).ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(id);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
