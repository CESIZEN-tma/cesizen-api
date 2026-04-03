using System.Security.Claims;
using api.CZ.Features.InformationPages;
using api.CZ.Features.InformationPages.DTOs;
using api.CZ.Features.InformationPages.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace api.Tests.Unit.Features.InformationPages;

public class InformationPageControllerTests
{
    private readonly Mock<IInformationPageService> _mockService;
    private readonly Mock<ILogger<InformationPageController>> _mockLogger;
    private readonly InformationPageController _controller;
    private readonly Guid _testAdminId;

    public InformationPageControllerTests()
    {
        _mockService = new Mock<IInformationPageService>();
        _mockLogger = new Mock<ILogger<InformationPageController>>();
        _controller = new InformationPageController(_mockService.Object, _mockLogger.Object);
        _testAdminId = Guid.NewGuid();

        // Setup admin claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testAdminId.ToString()),
            new Claim(ClaimTypes.Role, "Administrator")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_ShouldReturnOkWithPages()
    {
        // Arrange
        var pages = new List<GetInformationPageDto>
        {
            new GetInformationPageDto
            {
                Id = Guid.NewGuid(),
                Title = "Page 1",
                Description = "Desc 1",
                Content = "Content 1",
                ContentType = "HTML",
                CurrentlyEditing = false,
                Status = "Published",
                Active = true,
                CreationTime = DateTime.UtcNow,
                IdAdministrators = Guid.NewGuid()
            },
            new GetInformationPageDto
            {
                Id = Guid.NewGuid(),
                Title = "Page 2",
                Description = "Desc 2",
                Content = "Content 2",
                ContentType = "HTML",
                CurrentlyEditing = false,
                Status = "Published",
                Active = true,
                CreationTime = DateTime.UtcNow,
                IdAdministrators = Guid.NewGuid()
            }
        };

        _mockService
            .Setup(s => s.GetAllAsync())
            .ReturnsAsync(pages);

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(pages);

        _mockService.Verify(s => s.GetAllAsync(), Times.Once);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WhenPageExists_ShouldReturnOkWithPage()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var page = new GetInformationPageDto
        {
            Id = pageId,
            Title = "Test Page",
            Description = "Test Description",
            Content = "Test Content",
            ContentType = "HTML",
            CurrentlyEditing = false,
            Status = "Published",
            Active = true,
            CreationTime = DateTime.UtcNow,
            IdAdministrators = Guid.NewGuid()
        };

        _mockService
            .Setup(s => s.GetByIdAsync(pageId))
            .ReturnsAsync(page);

        // Act
        var result = await _controller.GetById(pageId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(page);

        _mockService.Verify(s => s.GetByIdAsync(pageId), Times.Once);
    }

    [Fact]
    public async Task GetById_WhenPageDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var pageId = Guid.NewGuid();

        _mockService
            .Setup(s => s.GetByIdAsync(pageId))
            .ReturnsAsync((GetInformationPageDto?)null);

        // Act
        var result = await _controller.GetById(pageId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        _mockService.Verify(s => s.GetByIdAsync(pageId), Times.Once);
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WhenSuccessful_ShouldReturnCreatedWithPage()
    {
        // Arrange
        var createDto = new CreateInformationPageDto
        {
            Title = "New Page",
            Description = "Description",
            Content = "Content",
            ContentType = "HTML",
            Status = "Draft"
        };
        var createdPage = new GetInformationPageDto
        {
            Id = Guid.NewGuid(),
            Title = "New Page",
            Description = "Description",
            Content = "Content",
            ContentType = "HTML",
            CurrentlyEditing = false,
            Status = "Draft",
            Active = false,
            CreationTime = DateTime.UtcNow,
            IdAdministrators = _testAdminId
        };

        _mockService
            .Setup(s => s.CreateAsync(createDto, _testAdminId))
            .ReturnsAsync(createdPage);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        createdResult!.Value.Should().BeEquivalentTo(createdPage);

        _mockService.Verify(s => s.CreateAsync(createDto, _testAdminId), Times.Once);
    }

    [Fact]
    public async Task Create_WhenFails_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreateInformationPageDto
        {
            Title = "New Page",
            Description = "Description",
            Content = "Content",
            ContentType = "HTML",
            Status = "Draft"
        };

        _mockService
            .Setup(s => s.CreateAsync(createDto, _testAdminId))
            .ReturnsAsync((GetInformationPageDto?)null);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _mockService.Verify(s => s.CreateAsync(createDto, _testAdminId), Times.Once);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WhenSuccessful_ShouldReturnOkWithUpdatedPage()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var updateDto = new UpdateInformationPageDto
        {
            Title = "Updated Page",
            Description = "Updated Description",
            Content = "Updated Content",
            ContentType = "HTML",
            Status = "Published"
        };
        var updatedPage = new GetInformationPageDto
        {
            Id = pageId,
            Title = "Updated Page",
            Description = "Updated Description",
            Content = "Updated Content",
            ContentType = "HTML",
            CurrentlyEditing = false,
            Status = "Published",
            Active = true,
            CreationTime = DateTime.UtcNow,
            UpdateTime = DateTime.UtcNow,
            IdAdministrators = _testAdminId
        };

        _mockService
            .Setup(s => s.UpdateAsync(pageId, updateDto, _testAdminId))
            .ReturnsAsync(updatedPage);

        // Act
        var result = await _controller.Update(pageId, updateDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(updatedPage);

        _mockService.Verify(s => s.UpdateAsync(pageId, updateDto, _testAdminId), Times.Once);
    }

    [Fact]
    public async Task Update_WhenPageDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var updateDto = new UpdateInformationPageDto
        {
            Title = "Updated Page",
            Description = "Updated Description",
            Content = "Updated Content",
            ContentType = "HTML",
            Status = "Published"
        };

        _mockService
            .Setup(s => s.UpdateAsync(pageId, updateDto, _testAdminId))
            .ReturnsAsync((GetInformationPageDto?)null);

        // Act
        var result = await _controller.Update(pageId, updateDto);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        _mockService.Verify(s => s.UpdateAsync(pageId, updateDto, _testAdminId), Times.Once);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WhenSuccessful_ShouldReturnOkWithMessage()
    {
        // Arrange
        var pageId = Guid.NewGuid();

        _mockService
            .Setup(s => s.DeleteAsync(pageId, _testAdminId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(pageId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockService.Verify(s => s.DeleteAsync(pageId, _testAdminId), Times.Once);
    }

    [Fact]
    public async Task Delete_WhenPageDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var pageId = Guid.NewGuid();

        _mockService
            .Setup(s => s.DeleteAsync(pageId, _testAdminId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(pageId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        _mockService.Verify(s => s.DeleteAsync(pageId, _testAdminId), Times.Once);
    }

    #endregion
}
