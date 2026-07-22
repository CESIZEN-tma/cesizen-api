using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using api.CZ.Features.UserSavedConfigurations;
using api.CZ.Features.UserSavedConfigurations.DTOs;
using api.CZ.Features.UserSavedConfigurations.Services;

namespace api.Tests.Unit.Features.UserSavedConfigurations;

public class UserSavedConfigurationControllerTests
{
    private readonly Mock<IUserSavedConfigurationService> _mockService;
    private readonly UserSavedConfigurationController _controller;
    private readonly Guid _testUserId;

    public UserSavedConfigurationControllerTests()
    {
        _mockService = new Mock<IUserSavedConfigurationService>();
        var mockLogger = new Mock<ILogger<UserSavedConfigurationController>>();
        _controller = new UserSavedConfigurationController(_mockService.Object, mockLogger.Object);
        _testUserId = Guid.NewGuid();

        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()) }, "TestAuth");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    private static GetUserSavedConfigurationDto BuildConfig(Guid? id = null)
    {
        return new GetUserSavedConfigurationDto
        {
            Id = id ?? Guid.NewGuid(), Name = "Config", Objective = "Relaxation", GuidanceType = "Visual"
        };
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithCurrentUsersConfigurations()
    {
        // Arrange
        _mockService.Setup(s => s.GetByUserAsync(_testUserId)).ReturnsAsync(new List<GetUserSavedConfigurationDto> { BuildConfig() });

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockService.Verify(s => s.GetByUserAsync(_testUserId), Times.Once);
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
        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((GetUserSavedConfigurationDto?)null);

        // Act
        var result = await _controller.GetById(id);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_ValidDto_ReturnsCreatedAtAction()
    {
        // Arrange
        var dto = new CreateUserSavedConfigurationDto { Name = "New", Objective = "Relaxation", GuidanceType = "Visual" };
        var created = BuildConfig();
        _mockService.Setup(s => s.CreateAsync(dto, _testUserId)).ReturnsAsync(created);

        // Act
        var result = await _controller.Create(dto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Create_ServiceReturnsNull_ReturnsBadRequest()
    {
        // Arrange
        var dto = new CreateUserSavedConfigurationDto { Name = "New", Objective = "x", GuidanceType = "y" };
        _mockService.Setup(s => s.CreateAsync(dto, _testUserId)).ReturnsAsync((GetUserSavedConfigurationDto?)null);

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
        var dto = new UpdateUserSavedConfigurationDto { Name = "Updated", Objective = "Focus", GuidanceType = "Audio" };
        _mockService.Setup(s => s.UpdateAsync(config.Id, dto)).ReturnsAsync(config);

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
        var dto = new UpdateUserSavedConfigurationDto { Name = "x", Objective = "y", GuidanceType = "z" };
        _mockService.Setup(s => s.UpdateAsync(id, dto)).ReturnsAsync((GetUserSavedConfigurationDto?)null);

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
        _mockService.Setup(s => s.DeleteAsync(id)).ReturnsAsync(true);

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
        _mockService.Setup(s => s.DeleteAsync(id)).ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(id);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CreateFromQuizResponses_ValidResponses_ReturnsCreatedAtAction()
    {
        // Arrange
        var dto = new QuizResponsesDto { QuizId = Guid.NewGuid() };
        var created = BuildConfig();
        _mockService.Setup(s => s.CreateFromQuizResponsesAsync(_testUserId, dto)).ReturnsAsync(created);

        // Act
        var result = await _controller.CreateFromQuizResponses(dto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task CreateFromQuizResponses_ServiceReturnsNull_ReturnsBadRequest()
    {
        // Arrange
        var dto = new QuizResponsesDto { QuizId = Guid.NewGuid() };
        _mockService.Setup(s => s.CreateFromQuizResponsesAsync(_testUserId, dto)).ReturnsAsync((GetUserSavedConfigurationDto?)null);

        // Act
        var result = await _controller.CreateFromQuizResponses(dto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateFromQuizResponses_InvalidQuizOrResponses_ReturnsBadRequestWithErrorMessage()
    {
        // Arrange
        var dto = new QuizResponsesDto { QuizId = Guid.NewGuid() };
        _mockService.Setup(s => s.CreateFromQuizResponsesAsync(_testUserId, dto))
            .ThrowsAsync(new InvalidOperationException("Quiz not found."));

        // Act
        var result = await _controller.CreateFromQuizResponses(dto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
