using FluentAssertions;
using Moq;
using api.CZ.Features.AdminLogs.Services;
using api.CZ.Features.NavigationMenus.DTOs;
using api.CZ.Features.NavigationMenus.Models;
using api.CZ.Features.NavigationMenus.Repositories;
using api.CZ.Features.NavigationMenus.Services;
using System.Linq.Expressions;

namespace api.Tests.Unit.Services;

public class NavigationMenuServiceTests
{
    private readonly Mock<INavigationMenuRepository> _mockRepository;
    private readonly Mock<IAdminActionLogger> _mockActionLogger;
    private readonly NavigationMenuService _sut;

    public NavigationMenuServiceTests()
    {
        _mockRepository = new Mock<INavigationMenuRepository>();
        _mockActionLogger = new Mock<IAdminActionLogger>();
        _sut = new NavigationMenuService(_mockRepository.Object, _mockActionLogger.Object);
    }

    private static NavigationMenu BuildMenu(Guid? id = null, Guid? parentId = null, int position = 0, string label = "Menu", string? url = "/menu")
    {
        return new NavigationMenu
        {
            Id = id ?? Guid.NewGuid(),
            ParentId = parentId,
            Position = position,
            Label = label,
            Url = url,
            CurrentlyEditing = false,
            CreationTime = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task GetAllAsync_BuildsTreeWithChildrenNestedUnderParents()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var parent = BuildMenu(parentId, null, 1, "Parent");
        var child = BuildMenu(Guid.NewGuid(), parentId, 1, "Child");

        _mockRepository.Setup(r => r.ListAsync(
                It.IsAny<Expression<Func<NavigationMenu, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<NavigationMenu> { child, parent });

        // Act
        var result = (await _sut.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(parentId);
        result[0].Children.Should().HaveCount(1);
        result[0].Children[0].Id.Should().Be(child.Id);
    }

    [Fact]
    public async Task GetAllAsync_OrphanedChildWithMissingParent_TreatedAsNotIncluded()
    {
        // Arrange
        var child = BuildMenu(Guid.NewGuid(), Guid.NewGuid(), 1, "Orphan");

        _mockRepository.Setup(r => r.ListAsync(
                It.IsAny<Expression<Func<NavigationMenu, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<NavigationMenu> { child });

        // Act
        var result = (await _sut.GetAllAsync()).ToList();

        // Assert: not a root (has ParentId) and parent not found in lookup -> excluded entirely
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingMenu_ReturnsDto()
    {
        // Arrange
        var menu = BuildMenu();
        _mockRepository.Setup(r => r.FindAsync(menu.Id, It.IsAny<CancellationToken>())).ReturnsAsync(menu);

        // Act
        var result = await _sut.GetByIdAsync(menu.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(menu.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentMenu_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.FindAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((NavigationMenu?)null);

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_RootMenu_KeepsUrl()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var dto = new CreateNavigationMenuDto { ParentId = null, Position = 1, Label = "Root", Url = "/root" };

        // Act
        var result = await _sut.CreateAsync(dto, adminId);

        // Assert
        result.Should().NotBeNull();
        result!.Url.Should().Be("/root");
        _mockActionLogger.Verify(l => l.LogCreateAsync(adminId, "NavigationMenu", result.Id, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_SubMenu_UrlIsAlwaysNullRegardlessOfDto()
    {
        // Arrange
        var parent = BuildMenu(Guid.NewGuid(), null, 1, "Parent", url: null);
        _mockRepository.Setup(r => r.FindAsync(parent.Id, It.IsAny<CancellationToken>())).ReturnsAsync(parent);
        var dto = new CreateNavigationMenuDto { ParentId = parent.Id, Position = 1, Label = "Child", Url = "/should-be-ignored" };

        // Act
        var result = await _sut.CreateAsync(dto, Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result!.Url.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_SubMenuUnderParentWithUrl_ClearsParentUrl()
    {
        // Arrange
        var parent = BuildMenu(Guid.NewGuid(), null, 1, "Parent", url: "/parent-url");
        _mockRepository.Setup(r => r.FindAsync(parent.Id, It.IsAny<CancellationToken>())).ReturnsAsync(parent);
        var dto = new CreateNavigationMenuDto { ParentId = parent.Id, Position = 1, Label = "Child", Url = null };

        // Act
        await _sut.CreateAsync(dto, Guid.NewGuid());

        // Assert
        parent.Url.Should().BeNull();
        _mockRepository.Verify(r => r.UpdateAsync(parent, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ExistingMenu_UpdatesFields()
    {
        // Arrange
        var menu = BuildMenu();
        var adminId = Guid.NewGuid();
        var dto = new UpdateNavigationMenuDto
        {
            ParentId = null, Position = 5, Label = "Updated", Url = "/updated", CurrentlyEditing = true
        };
        _mockRepository.Setup(r => r.FindAsync(menu.Id, It.IsAny<CancellationToken>())).ReturnsAsync(menu);

        // Act
        var result = await _sut.UpdateAsync(menu.Id, dto, adminId);

        // Assert
        result.Should().NotBeNull();
        menu.Label.Should().Be("Updated");
        menu.Position.Should().Be(5);
        menu.CurrentlyEditing.Should().BeTrue();
        _mockActionLogger.Verify(l => l.LogUpdateAsync(adminId, "NavigationMenu", menu.Id, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentMenu_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.FindAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((NavigationMenu?)null);

        // Act
        var result = await _sut.UpdateAsync(id, new UpdateNavigationMenuDto { Position = 1, Label = "x" }, Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_MenuWithChildren_CascadesSoftDeleteToChildren()
    {
        // Arrange
        var menu = BuildMenu();
        var adminId = Guid.NewGuid();
        var children = new List<NavigationMenu> { BuildMenu(Guid.NewGuid(), menu.Id), BuildMenu(Guid.NewGuid(), menu.Id) };

        _mockRepository.Setup(r => r.FindAsync(menu.Id, It.IsAny<CancellationToken>())).ReturnsAsync(menu);
        _mockRepository.Setup(r => r.ListAsync(
                It.IsAny<Expression<Func<NavigationMenu, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(children);

        // Act
        var result = await _sut.DeleteAsync(menu.Id, adminId);

        // Assert
        result.Should().BeTrue();
        menu.DeletionTime.Should().NotBeNull();
        children.Should().AllSatisfy(c => c.DeletionTime.Should().NotBeNull());
        _mockRepository.Verify(r => r.SoftDeleteAsync(It.IsAny<NavigationMenu>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
        _mockActionLogger.Verify(l => l.LogDeleteAsync(adminId, "NavigationMenu", menu.Id, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentMenu_ReturnsFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.FindAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((NavigationMenu?)null);

        // Act
        var result = await _sut.DeleteAsync(id, Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdatePositionsAsync_ValidMenus_UpdatesAllPositionsAndLogsOnce()
    {
        // Arrange
        var menu1 = BuildMenu(Guid.NewGuid(), null, 1);
        var menu2 = BuildMenu(Guid.NewGuid(), null, 2);
        var adminId = Guid.NewGuid();

        _mockRepository.Setup(r => r.FindAsync(menu1.Id, It.IsAny<CancellationToken>())).ReturnsAsync(menu1);
        _mockRepository.Setup(r => r.FindAsync(menu2.Id, It.IsAny<CancellationToken>())).ReturnsAsync(menu2);

        var positions = new List<UpdateMenuPositionDto>
        {
            new() { Id = menu1.Id, Position = 10 },
            new() { Id = menu2.Id, Position = 20 }
        };

        // Act
        await _sut.UpdatePositionsAsync(positions, adminId);

        // Assert
        menu1.Position.Should().Be(10);
        menu2.Position.Should().Be(20);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<NavigationMenu>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _mockActionLogger.Verify(l => l.LogUpdateAsync(adminId, "NavigationMenu", Guid.Empty, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePositionsAsync_NonExistentMenuInList_SkipsItSilently()
    {
        // Arrange
        var missingId = Guid.NewGuid();
        _mockRepository.Setup(r => r.FindAsync(missingId, It.IsAny<CancellationToken>())).ReturnsAsync((NavigationMenu?)null);

        var positions = new List<UpdateMenuPositionDto> { new() { Id = missingId, Position = 10 } };

        // Act
        await _sut.UpdatePositionsAsync(positions, Guid.NewGuid());

        // Assert
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<NavigationMenu>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
