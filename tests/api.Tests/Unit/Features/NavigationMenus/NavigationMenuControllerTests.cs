using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using api.CZ.Features.NavigationMenus;
using api.CZ.Features.NavigationMenus.DTOs;
using api.CZ.Features.NavigationMenus.Services;

namespace api.Tests.Unit.Features.NavigationMenus;

public class NavigationMenuControllerTests
{
    private readonly Mock<INavigationMenuService> _mockService;
    private readonly NavigationMenuController _controller;
    private readonly Guid _testAdminId;

    public NavigationMenuControllerTests()
    {
        _mockService = new Mock<INavigationMenuService>();
        var mockLogger = new Mock<ILogger<NavigationMenuController>>();
        _controller = new NavigationMenuController(_mockService.Object, mockLogger.Object);
        _testAdminId = Guid.NewGuid();

        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, _testAdminId.ToString()) }, "TestAuth");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    private static GetNavigationMenuDto BuildMenu(Guid? id = null)
    {
        return new GetNavigationMenuDto { Id = id ?? Guid.NewGuid(), Label = "Menu", Position = 1 };
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithMenus()
    {
        // Arrange
        _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<GetNavigationMenuDto> { BuildMenu() });

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ExistingMenu_ReturnsOk()
    {
        // Arrange
        var menu = BuildMenu();
        _mockService.Setup(s => s.GetByIdAsync(menu.Id)).ReturnsAsync(menu);

        // Act
        var result = await _controller.GetById(menu.Id);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_NonExistentMenu_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((GetNavigationMenuDto?)null);

        // Act
        var result = await _controller.GetById(id);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_ValidDto_ReturnsCreatedAtAction()
    {
        // Arrange
        var dto = new CreateNavigationMenuDto { Label = "New", Position = 1 };
        var created = BuildMenu();
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
        var dto = new CreateNavigationMenuDto { Label = "New", Position = 1 };
        _mockService.Setup(s => s.CreateAsync(dto, _testAdminId)).ReturnsAsync((GetNavigationMenuDto?)null);

        // Act
        var result = await _controller.Create(dto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ExistingMenu_ReturnsOk()
    {
        // Arrange
        var menu = BuildMenu();
        var dto = new UpdateNavigationMenuDto { Label = "Updated", Position = 2 };
        _mockService.Setup(s => s.UpdateAsync(menu.Id, dto, _testAdminId)).ReturnsAsync(menu);

        // Act
        var result = await _controller.Update(menu.Id, dto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_NonExistentMenu_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateNavigationMenuDto { Label = "x", Position = 1 };
        _mockService.Setup(s => s.UpdateAsync(id, dto, _testAdminId)).ReturnsAsync((GetNavigationMenuDto?)null);

        // Act
        var result = await _controller.Update(id, dto);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_ExistingMenu_ReturnsOk()
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
    public async Task Delete_NonExistentMenu_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.DeleteAsync(id, _testAdminId)).ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(id);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdatePositions_ValidList_ReturnsOk()
    {
        // Arrange
        var positions = new List<UpdateMenuPositionDto> { new() { Id = Guid.NewGuid(), Position = 1 } };

        // Act
        var result = await _controller.UpdatePositions(positions);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockService.Verify(s => s.UpdatePositionsAsync(positions, _testAdminId), Times.Once);
    }
}
