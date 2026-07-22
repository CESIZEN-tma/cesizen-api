using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using api.CZ.Features.InformationTags;
using api.CZ.Features.InformationTags.DTOs;
using api.CZ.Features.InformationTags.Services;

namespace api.Tests.Unit.Features.InformationTags;

public class InformationTagControllerTests
{
    private readonly Mock<IInformationTagService> _mockService;
    private readonly InformationTagController _controller;
    private readonly Guid _testAdminId;

    public InformationTagControllerTests()
    {
        _mockService = new Mock<IInformationTagService>();
        var mockLogger = new Mock<ILogger<InformationTagController>>();
        _controller = new InformationTagController(_mockService.Object, mockLogger.Object);
        _testAdminId = Guid.NewGuid();

        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, _testAdminId.ToString()) }, "TestAuth");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    private static GetInformationTagDto BuildTag(Guid? id = null)
    {
        return new GetInformationTagDto { Id = id ?? Guid.NewGuid(), Label = "Tag" };
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithTags()
    {
        // Arrange
        _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<GetInformationTagDto> { BuildTag() });

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ExistingTag_ReturnsOk()
    {
        // Arrange
        var tag = BuildTag();
        _mockService.Setup(s => s.GetByIdAsync(tag.Id)).ReturnsAsync(tag);

        // Act
        var result = await _controller.GetById(tag.Id);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_NonExistentTag_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((GetInformationTagDto?)null);

        // Act
        var result = await _controller.GetById(id);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_ValidDto_ReturnsCreatedAtAction()
    {
        // Arrange
        var dto = new CreateInformationTagDto { Label = "New" };
        var created = BuildTag();
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
        var dto = new CreateInformationTagDto { Label = "New" };
        _mockService.Setup(s => s.CreateAsync(dto, _testAdminId)).ReturnsAsync((GetInformationTagDto?)null);

        // Act
        var result = await _controller.Create(dto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ExistingTag_ReturnsOk()
    {
        // Arrange
        var tag = BuildTag();
        var dto = new UpdateInformationTagDto { Label = "Updated" };
        _mockService.Setup(s => s.UpdateAsync(tag.Id, dto, _testAdminId)).ReturnsAsync(tag);

        // Act
        var result = await _controller.Update(tag.Id, dto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_NonExistentTag_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateInformationTagDto { Label = "x" };
        _mockService.Setup(s => s.UpdateAsync(id, dto, _testAdminId)).ReturnsAsync((GetInformationTagDto?)null);

        // Act
        var result = await _controller.Update(id, dto);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_ExistingTag_ReturnsOk()
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
    public async Task Delete_NonExistentTag_ReturnsNotFound()
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
