using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api.CZ.Features.InformationPages.Services;
using api.CZ.Features.InformationTags.Services;
using api.CZ.Features.NavigationMenus.Services;

namespace api.CZ.Features.PublicContent;

[ApiController]
[Route("api/content")]
[AllowAnonymous]
public class PublicContentController : ControllerBase
{
    private readonly IInformationPageService _pageService;
    private readonly IInformationTagService _tagService;
    private readonly INavigationMenuService _menuService;
    private readonly ILogger<PublicContentController> _logger;

    public PublicContentController(
        IInformationPageService pageService,
        IInformationTagService tagService,
        INavigationMenuService menuService,
        ILogger<PublicContentController> logger)
    {
        _pageService = pageService;
        _tagService = tagService;
        _menuService = menuService;
        _logger = logger;
    }

    // Information Pages
    [HttpGet("pages")]
    public async Task<IActionResult> GetAllPages()
    {
        var pages = await _pageService.GetAllAsync();
        // Filter to only active/published pages
        var activePages = pages.Where(p => p.Active && p.Status == "Published");
        return Ok(activePages);
    }

    [HttpGet("pages/{id:guid}")]
    public async Task<IActionResult> GetPageById(Guid id)
    {
        var page = await _pageService.GetByIdAsync(id);

        if (page == null || !page.Active || page.Status != "Published")
            return NotFound(new { error = "Information page not found or not published" });

        return Ok(page);
    }

    // Information Tags
    [HttpGet("tags")]
    public async Task<IActionResult> GetAllTags()
    {
        var tags = await _tagService.GetAllAsync();
        return Ok(tags);
    }

    [HttpGet("tags/{id:guid}")]
    public async Task<IActionResult> GetTagById(Guid id)
    {
        var tag = await _tagService.GetByIdAsync(id);

        if (tag == null)
            return NotFound(new { error = "Information tag not found" });

        return Ok(tag);
    }

    // Navigation Menus
    [HttpGet("menus")]
    public async Task<IActionResult> GetAllMenus()
    {
        var menus = await _menuService.GetAllAsync();
        return Ok(menus);
    }

    [HttpGet("menus/{id:guid}")]
    public async Task<IActionResult> GetMenuById(Guid id)
    {
        var menu = await _menuService.GetByIdAsync(id);

        if (menu == null)
            return NotFound(new { error = "Navigation menu not found" });

        return Ok(menu);
    }
}
