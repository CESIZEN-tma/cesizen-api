using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using api.CZ.Features.InformationPages.DTOs;
using api.CZ.Features.InformationPages.Services;
using api.CZ.Features.InformationTags.DTOs;
using api.CZ.Features.InformationTags.Services;
using api.CZ.Features.NavigationMenus.DTOs;
using api.CZ.Features.NavigationMenus.Services;
using api.CZ.Features.PublicContent;

namespace api.Tests.Unit.Features.PublicContent;

public class PublicContentControllerTests
{
    private readonly Mock<IInformationPageService> _mockPageService;
    private readonly Mock<IInformationTagService> _mockTagService;
    private readonly Mock<INavigationMenuService> _mockMenuService;
    private readonly PublicContentController _controller;

    public PublicContentControllerTests()
    {
        _mockPageService = new Mock<IInformationPageService>();
        _mockTagService = new Mock<IInformationTagService>();
        _mockMenuService = new Mock<INavigationMenuService>();
        var mockLogger = new Mock<ILogger<PublicContentController>>();

        _controller = new PublicContentController(
            _mockPageService.Object, _mockTagService.Object, _mockMenuService.Object, mockLogger.Object);
    }

    private static GetInformationPageDto BuildPage(bool active = true, string status = "published")
    {
        return new GetInformationPageDto
        {
            Id = Guid.NewGuid(), Title = "Page", Description = "D", Content = "C",
            ContentType = "html", Status = status, Active = active, CreationTime = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task GetAllPages_OnlyReturnsActivePublishedPages()
    {
        // Arrange
        var published = BuildPage(active: true, status: "Published");
        var draft = BuildPage(active: true, status: "draft");
        var inactive = BuildPage(active: false, status: "published");

        _mockPageService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<GetInformationPageDto> { published, draft, inactive });

        // Act
        var result = await _controller.GetAllPages();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var pages = ((IEnumerable<GetInformationPageDto>)ok.Value!).ToList();
        pages.Should().ContainSingle();
        pages[0].Id.Should().Be(published.Id);
    }

    [Fact]
    public async Task GetPageById_ExistingPublishedActivePage_ReturnsOk()
    {
        // Arrange
        var page = BuildPage();
        _mockPageService.Setup(s => s.GetByIdAsync(page.Id)).ReturnsAsync(page);

        // Act
        var result = await _controller.GetPageById(page.Id);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetPageById_NonExistentPage_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockPageService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((GetInformationPageDto?)null);

        // Act
        var result = await _controller.GetPageById(id);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetPageById_DraftPage_ReturnsNotFound()
    {
        // Arrange
        var page = BuildPage(active: true, status: "draft");
        _mockPageService.Setup(s => s.GetByIdAsync(page.Id)).ReturnsAsync(page);

        // Act
        var result = await _controller.GetPageById(page.Id);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetPageById_InactivePage_ReturnsNotFound()
    {
        // Arrange
        var page = BuildPage(active: false);
        _mockPageService.Setup(s => s.GetByIdAsync(page.Id)).ReturnsAsync(page);

        // Act
        var result = await _controller.GetPageById(page.Id);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetAllTags_ReturnsOkWithTags()
    {
        // Arrange
        var tags = new List<GetInformationTagDto> { new() { Id = Guid.NewGuid(), Label = "Tag" } };
        _mockTagService.Setup(s => s.GetAllAsync()).ReturnsAsync(tags);

        // Act
        var result = await _controller.GetAllTags();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetTagById_ExistingTag_ReturnsOk()
    {
        // Arrange
        var tag = new GetInformationTagDto { Id = Guid.NewGuid(), Label = "Tag" };
        _mockTagService.Setup(s => s.GetByIdAsync(tag.Id)).ReturnsAsync(tag);

        // Act
        var result = await _controller.GetTagById(tag.Id);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetTagById_NonExistentTag_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockTagService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((GetInformationTagDto?)null);

        // Act
        var result = await _controller.GetTagById(id);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetAllMenus_FiltersOutSubMenusLinkingToUnpublishedPages()
    {
        // Arrange
        var publishedPage = BuildPage(active: true, status: "published");
        var draftPage = BuildPage(active: true, status: "draft");

        var validChild = new GetNavigationMenuDto
        {
            Id = Guid.NewGuid(), Label = "Valid", Position = 1,
            Url = $"cesizen://info-page/{publishedPage.Id}"
        };
        var invalidChild = new GetNavigationMenuDto
        {
            Id = Guid.NewGuid(), Label = "Invalid", Position = 2,
            Url = $"cesizen://info-page/{draftPage.Id}"
        };
        var noLinkChild = new GetNavigationMenuDto
        {
            Id = Guid.NewGuid(), Label = "NoLink", Position = 3, Url = "/static"
        };
        var root = new GetNavigationMenuDto
        {
            Id = Guid.NewGuid(), Label = "Root", Position = 1,
            Children = new List<GetNavigationMenuDto> { validChild, invalidChild, noLinkChild }
        };

        _mockMenuService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<GetNavigationMenuDto> { root });
        _mockPageService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<GetInformationPageDto> { publishedPage, draftPage });

        // Act
        var result = await _controller.GetAllMenus();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var menus = ((IEnumerable<GetNavigationMenuDto>)ok.Value!).ToList();
        menus.Should().HaveCount(1);
        menus[0].Children.Should().HaveCount(2);
        menus[0].Children.Should().Contain(c => c.Id == validChild.Id);
        menus[0].Children.Should().Contain(c => c.Id == noLinkChild.Id);
        menus[0].Children.Should().NotContain(c => c.Id == invalidChild.Id);
    }

    [Fact]
    public async Task GetMenuById_ExistingMenu_ReturnsOk()
    {
        // Arrange
        var menu = new GetNavigationMenuDto { Id = Guid.NewGuid(), Label = "Menu", Position = 1 };
        _mockMenuService.Setup(s => s.GetByIdAsync(menu.Id)).ReturnsAsync(menu);

        // Act
        var result = await _controller.GetMenuById(menu.Id);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetMenuById_NonExistentMenu_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockMenuService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((GetNavigationMenuDto?)null);

        // Act
        var result = await _controller.GetMenuById(id);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
