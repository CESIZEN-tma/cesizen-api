using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using api.CZ.Features.Administrators;
using api.CZ.Features.Administrators.DTOs;
using api.CZ.Features.Administrators.Services;

namespace api.Tests.Unit.Features.Administrators;

public class AdministratorControllerTests
{
    private readonly Mock<IAdministratorService> _mockService;
    private readonly AdministratorController _controller;
    private readonly Guid _testAdminId;

    public AdministratorControllerTests()
    {
        _mockService = new Mock<IAdministratorService>();
        var mockLogger = new Mock<ILogger<AdministratorController>>();
        _controller = new AdministratorController(_mockService.Object, mockLogger.Object);
        _testAdminId = Guid.NewGuid();

        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, _testAdminId.ToString()) }, "TestAuth");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    private static GetAdministratorDto BuildAdmin(Guid? id = null)
    {
        return new GetAdministratorDto { Id = id ?? Guid.NewGuid(), Email = "a@b.com", FirstName = "A", LastName = "B", MemberSince = DateTime.UtcNow };
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithAdmins()
    {
        // Arrange
        _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<GetAdministratorDto> { BuildAdmin() });

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ExistingAdmin_ReturnsOk()
    {
        // Arrange
        var admin = BuildAdmin();
        _mockService.Setup(s => s.GetByIdAsync(admin.Id)).ReturnsAsync(admin);

        // Act
        var result = await _controller.GetById(admin.Id);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_NonExistentAdmin_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((GetAdministratorDto?)null);

        // Act
        var result = await _controller.GetById(id);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_ValidDto_ReturnsCreatedAtAction()
    {
        // Arrange
        var dto = new CreateAdministratorDto { Email = "new@b.com", Password = "Password1!", FirstName = "A", LastName = "B" };
        var created = BuildAdmin();
        _mockService.Setup(s => s.CreateAsync(dto, _testAdminId)).ReturnsAsync(created);

        // Act
        var result = await _controller.Create(dto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Create_EmailAlreadyExists_ReturnsBadRequest()
    {
        // Arrange
        var dto = new CreateAdministratorDto { Email = "existing@b.com", Password = "Password1!", FirstName = "A", LastName = "B" };
        _mockService.Setup(s => s.CreateAsync(dto, _testAdminId)).ReturnsAsync((GetAdministratorDto?)null);

        // Act
        var result = await _controller.Create(dto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ExistingAdmin_ReturnsOk()
    {
        // Arrange
        var admin = BuildAdmin();
        var dto = new UpdateAdministratorDto { FirstName = "Updated", LastName = "Name" };
        _mockService.Setup(s => s.UpdateAsync(admin.Id, dto, _testAdminId)).ReturnsAsync(admin);

        // Act
        var result = await _controller.Update(admin.Id, dto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_NonExistentAdmin_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateAdministratorDto { FirstName = "x", LastName = "y" };
        _mockService.Setup(s => s.UpdateAsync(id, dto, _testAdminId)).ReturnsAsync((GetAdministratorDto?)null);

        // Act
        var result = await _controller.Update(id, dto);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_ExistingAdmin_ReturnsOk()
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
    public async Task Delete_NonExistentAdmin_ReturnsNotFound()
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
    public async Task Delete_SelfDeletion_ReturnsBadRequest()
    {
        // Arrange
        _mockService.Setup(s => s.DeleteAsync(_testAdminId, _testAdminId))
            .ThrowsAsync(new InvalidOperationException("You cannot delete your own administrator account"));

        // Act
        var result = await _controller.Delete(_testAdminId);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
