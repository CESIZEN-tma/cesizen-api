using FluentAssertions;
using Moq;
using api.CZ.Features.AdminLogs.Services;
using api.CZ.Features.InformationPages.DTOs;
using api.CZ.Features.InformationPages.Models;
using api.CZ.Features.InformationPages.Repositories;
using api.CZ.Features.InformationPages.Services;
using api.CZ.Features.InformationTags.Models;
using api.CZ.Features.InformationTags.Repositories;
using System.Linq.Expressions;

namespace api.Tests.Unit.Services;

public class InformationPageServiceTests
{
    private readonly Mock<IInformationPageRepository> _mockRepository;
    private readonly Mock<IInformationTagRepository> _mockTagRepository;
    private readonly Mock<IAdminActionLogger> _mockActionLogger;
    private readonly InformationPageService _sut;

    public InformationPageServiceTests()
    {
        _mockRepository = new Mock<IInformationPageRepository>();
        _mockTagRepository = new Mock<IInformationTagRepository>();
        _mockActionLogger = new Mock<IAdminActionLogger>();
        _sut = new InformationPageService(_mockRepository.Object, _mockTagRepository.Object, _mockActionLogger.Object);
    }

    private static InformationPage BuildPage(Guid? id = null, Guid? adminId = null)
    {
        return new InformationPage
        {
            Id = id ?? Guid.NewGuid(),
            Title = "Page",
            Description = "Desc",
            Content = "Content",
            ContentType = "html",
            Status = "draft",
            Active = true,
            IdAdministrators = adminId ?? Guid.NewGuid(),
            CreationTime = DateTime.UtcNow
        };
    }

    private static CreateInformationPageDto BuildCreateDto(List<Guid>? tagIds = null)
    {
        return new CreateInformationPageDto
        {
            Title = "New Page",
            Description = "Desc",
            Content = "Content",
            ContentType = "html",
            Status = "draft",
            Active = false,
            TagIds = tagIds ?? new List<Guid>()
        };
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedDtos()
    {
        // Arrange
        var pages = new List<InformationPage> { BuildPage(), BuildPage() };
        _mockRepository.Setup(r => r.ListWithTagsAsync()).ReturnsAsync(pages);

        // Act
        var result = (await _sut.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingPage_ReturnsDto()
    {
        // Arrange
        var page = BuildPage();
        _mockRepository.Setup(r => r.FindWithTagsAsync(page.Id)).ReturnsAsync(page);

        // Act
        var result = await _sut.GetByIdAsync(page.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(page.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentPage_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.FindWithTagsAsync(id)).ReturnsAsync((InformationPage?)null);

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_NoTags_CreatesPageWithoutTags()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var dto = BuildCreateDto();

        // Act
        var result = await _sut.CreateAsync(dto, adminId);

        // Assert
        result.Should().NotBeNull();
        result!.TagIds.Should().BeEmpty();
        result.IdAdministrators.Should().Be(adminId);
        _mockActionLogger.Verify(l => l.LogCreateAsync(adminId, "InformationPage", result.Id, It.IsAny<string>()), Times.Once);
        _mockTagRepository.Verify(r => r.ListAsync(
            It.IsAny<Expression<Func<InformationTag, bool>>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithTagIds_AttachesMatchingTags()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var tag = new InformationTag { Id = Guid.NewGuid(), Label = "Tag1", CreationTime = DateTime.UtcNow };
        var dto = BuildCreateDto(new List<Guid> { tag.Id });

        _mockTagRepository.Setup(r => r.ListAsync(
                It.IsAny<Expression<Func<InformationTag, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InformationTag> { tag });

        InformationPage? added = null;
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<InformationPage>(), It.IsAny<CancellationToken>()))
            .Callback<InformationPage, CancellationToken>((p, _) => added = p)
            .ReturnsAsync((InformationPage p, CancellationToken _) => p);

        // Act
        await _sut.CreateAsync(dto, adminId);

        // Assert
        added.Should().NotBeNull();
        added!.IdInformationTags.Should().ContainSingle(t => t.Id == tag.Id);
    }

    [Fact]
    public async Task UpdateAsync_ExistingPage_UpdatesFieldsAndReplacesTags()
    {
        // Arrange
        var page = BuildPage();
        var oldTag = new InformationTag { Id = Guid.NewGuid(), Label = "Old", CreationTime = DateTime.UtcNow };
        page.IdInformationTags.Add(oldTag);
        var newTag = new InformationTag { Id = Guid.NewGuid(), Label = "New", CreationTime = DateTime.UtcNow };
        var adminId = Guid.NewGuid();

        var dto = new UpdateInformationPageDto
        {
            Title = "Updated",
            Description = "Updated Desc",
            Content = "Updated Content",
            ContentType = "markdown",
            Status = "published",
            Active = true,
            CurrentlyEditing = false,
            TagIds = new List<Guid> { newTag.Id }
        };

        _mockRepository.Setup(r => r.FindWithTagsAsync(page.Id)).ReturnsAsync(page);
        _mockTagRepository.Setup(r => r.ListAsync(
                It.IsAny<Expression<Func<InformationTag, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InformationTag> { newTag });

        // Act
        var result = await _sut.UpdateAsync(page.Id, dto, adminId);

        // Assert
        result.Should().NotBeNull();
        page.Title.Should().Be("Updated");
        page.Status.Should().Be("published");
        page.IdInformationTags.Should().ContainSingle(t => t.Id == newTag.Id);
        page.IdInformationTags.Should().NotContain(t => t.Id == oldTag.Id);
        _mockActionLogger.Verify(l => l.LogUpdateAsync(adminId, "InformationPage", page.Id, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_EmptyTagIds_ClearsAllTags()
    {
        // Arrange
        var page = BuildPage();
        page.IdInformationTags.Add(new InformationTag { Id = Guid.NewGuid(), Label = "Old", CreationTime = DateTime.UtcNow });
        var dto = new UpdateInformationPageDto
        {
            Title = "T", Description = "D", Content = "C", ContentType = "html", Status = "draft",
            Active = true, CurrentlyEditing = false, TagIds = new List<Guid>()
        };

        _mockRepository.Setup(r => r.FindWithTagsAsync(page.Id)).ReturnsAsync(page);

        // Act
        await _sut.UpdateAsync(page.Id, dto, Guid.NewGuid());

        // Assert
        page.IdInformationTags.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_NonExistentPage_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.FindWithTagsAsync(id)).ReturnsAsync((InformationPage?)null);

        // Act
        var result = await _sut.UpdateAsync(id, new UpdateInformationPageDto
        {
            Title = "T", Description = "D", Content = "C", ContentType = "html", Status = "draft", Active = true
        }, Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ExistingPage_SoftDeletesAndDeactivates()
    {
        // Arrange
        var page = BuildPage();
        var adminId = Guid.NewGuid();
        _mockRepository.Setup(r => r.FindAsync(page.Id, It.IsAny<CancellationToken>())).ReturnsAsync(page);

        // Act
        var result = await _sut.DeleteAsync(page.Id, adminId);

        // Assert
        result.Should().BeTrue();
        page.DeletionTime.Should().NotBeNull();
        page.Active.Should().BeFalse();
        _mockActionLogger.Verify(l => l.LogDeleteAsync(adminId, "InformationPage", page.Id, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentPage_ReturnsFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.FindAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((InformationPage?)null);

        // Act
        var result = await _sut.DeleteAsync(id, Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_AlreadyDeletedPage_ReturnsFalse()
    {
        // Arrange
        var page = BuildPage();
        page.DeletionTime = DateTime.UtcNow;
        _mockRepository.Setup(r => r.FindAsync(page.Id, It.IsAny<CancellationToken>())).ReturnsAsync(page);

        // Act
        var result = await _sut.DeleteAsync(page.Id, Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }
}
