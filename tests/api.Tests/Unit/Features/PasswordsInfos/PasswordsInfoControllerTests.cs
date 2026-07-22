using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using api.CZ.Features.PasswordsInfos;
using api.CZ.Features.PasswordsInfos.DTOs;
using api.CZ.Features.PasswordsInfos.Services;

namespace api.Tests.Unit.Features.PasswordsInfos;

public class PasswordsInfoControllerTests
{
    private readonly Mock<IPasswordsInfoService> _mockService;
    private readonly PasswordsInfoController _controller;

    public PasswordsInfoControllerTests()
    {
        _mockService = new Mock<IPasswordsInfoService>();
        var mockLogger = new Mock<ILogger<PasswordsInfoController>>();
        _controller = new PasswordsInfoController(_mockService.Object, mockLogger.Object);
    }

    private static GetPasswordsInfoDto BuildInfo(Guid? id = null)
    {
        return new GetPasswordsInfoDto { Id = id ?? Guid.NewGuid(), LastLogin = DateTime.UtcNow, LastReset = DateTime.UtcNow };
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithInfos()
    {
        // Arrange
        _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<GetPasswordsInfoDto> { BuildInfo() });

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ExistingInfo_ReturnsOk()
    {
        // Arrange
        var info = BuildInfo();
        _mockService.Setup(s => s.GetByIdAsync(info.Id)).ReturnsAsync(info);

        // Act
        var result = await _controller.GetById(info.Id);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_NonExistentInfo_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((GetPasswordsInfoDto?)null);

        // Act
        var result = await _controller.GetById(id);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_ValidDto_ReturnsCreatedAtAction()
    {
        // Arrange
        var dto = new CreatePasswordsInfoDto { AttemptCount = 0 };
        var created = BuildInfo();
        _mockService.Setup(s => s.CreateAsync(dto)).ReturnsAsync(created);

        // Act
        var result = await _controller.Create(dto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Create_ServiceReturnsNull_ReturnsBadRequest()
    {
        // Arrange
        var dto = new CreatePasswordsInfoDto { AttemptCount = 0 };
        _mockService.Setup(s => s.CreateAsync(dto)).ReturnsAsync((GetPasswordsInfoDto?)null);

        // Act
        var result = await _controller.Create(dto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ExistingInfo_ReturnsOk()
    {
        // Arrange
        var info = BuildInfo();
        var dto = new UpdatePasswordsInfoDto { AttemptCount = 2 };
        _mockService.Setup(s => s.UpdateAsync(info.Id, dto)).ReturnsAsync(info);

        // Act
        var result = await _controller.Update(info.Id, dto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_NonExistentInfo_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdatePasswordsInfoDto { AttemptCount = 2 };
        _mockService.Setup(s => s.UpdateAsync(id, dto)).ReturnsAsync((GetPasswordsInfoDto?)null);

        // Act
        var result = await _controller.Update(id, dto);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_ExistingInfo_ReturnsOk()
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
    public async Task Delete_NonExistentInfo_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.DeleteAsync(id)).ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(id);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
