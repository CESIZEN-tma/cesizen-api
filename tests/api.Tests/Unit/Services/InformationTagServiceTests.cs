using FluentAssertions;
using Moq;
using api.CZ.Features.AdminLogs.Services;
using api.CZ.Features.InformationTags.DTOs;
using api.CZ.Features.InformationTags.Models;
using api.CZ.Features.InformationTags.Repositories;
using api.CZ.Features.InformationTags.Services;
using System.Linq.Expressions;

namespace api.Tests.Unit.Services;

public class InformationTagServiceTests
{
    private readonly Mock<IInformationTagRepository> _mockRepository;
    private readonly Mock<IAdminActionLogger> _mockActionLogger;
    private readonly InformationTagService _sut;

    public InformationTagServiceTests()
    {
        _mockRepository = new Mock<IInformationTagRepository>();
        _mockActionLogger = new Mock<IAdminActionLogger>();
        _sut = new InformationTagService(_mockRepository.Object, _mockActionLogger.Object);
    }

    private static InformationTag BuildTag(Guid? id = null, string label = "Tag")
    {
        return new InformationTag { Id = id ?? Guid.NewGuid(), Label = label, CreationTime = DateTime.UtcNow };
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedDtos()
    {
        // Arrange
        var tags = new List<InformationTag> { BuildTag(), BuildTag() };
        _mockRepository.Setup(r => r.ListAsync(
                It.IsAny<Expression<Func<InformationTag, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tags);

        // Act
        var result = (await _sut.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingTag_ReturnsDto()
    {
        // Arrange
        var tag = BuildTag();
        _mockRepository.Setup(r => r.FindAsync(tag.Id, It.IsAny<CancellationToken>())).ReturnsAsync(tag);

        // Act
        var result = await _sut.GetByIdAsync(tag.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Label.Should().Be(tag.Label);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentTag_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.FindAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((InformationTag?)null);

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_SoftDeletedTag_ReturnsNull()
    {
        // Arrange
        var tag = BuildTag();
        tag.DeletionTime = DateTime.UtcNow;
        _mockRepository.Setup(r => r.FindAsync(tag.Id, It.IsAny<CancellationToken>())).ReturnsAsync(tag);

        // Act
        var result = await _sut.GetByIdAsync(tag.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ValidDto_CreatesTagAndLogsAction()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var dto = new CreateInformationTagDto { Label = "New Tag" };

        // Act
        var result = await _sut.CreateAsync(dto, adminId);

        // Assert
        result.Should().NotBeNull();
        result!.Label.Should().Be("New Tag");
        _mockActionLogger.Verify(l => l.LogCreateAsync(adminId, "InformationTag", result.Id, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ExistingTag_UpdatesLabelAndLogsAction()
    {
        // Arrange
        var tag = BuildTag();
        var adminId = Guid.NewGuid();
        var dto = new UpdateInformationTagDto { Label = "Renamed" };
        _mockRepository.Setup(r => r.FindAsync(tag.Id, It.IsAny<CancellationToken>())).ReturnsAsync(tag);

        // Act
        var result = await _sut.UpdateAsync(tag.Id, dto, adminId);

        // Assert
        result.Should().NotBeNull();
        tag.Label.Should().Be("Renamed");
        _mockActionLogger.Verify(l => l.LogUpdateAsync(adminId, "InformationTag", tag.Id, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentTag_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.FindAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((InformationTag?)null);

        // Act
        var result = await _sut.UpdateAsync(id, new UpdateInformationTagDto { Label = "x" }, Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ExistingTag_SoftDeletesAndLogsAction()
    {
        // Arrange
        var tag = BuildTag();
        var adminId = Guid.NewGuid();
        _mockRepository.Setup(r => r.FindAsync(tag.Id, It.IsAny<CancellationToken>())).ReturnsAsync(tag);

        // Act
        var result = await _sut.DeleteAsync(tag.Id, adminId);

        // Assert
        result.Should().BeTrue();
        tag.DeletionTime.Should().NotBeNull();
        _mockActionLogger.Verify(l => l.LogDeleteAsync(adminId, "InformationTag", tag.Id, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentTag_ReturnsFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.FindAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((InformationTag?)null);

        // Act
        var result = await _sut.DeleteAsync(id, Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_AlreadyDeletedTag_ReturnsFalse()
    {
        // Arrange
        var tag = BuildTag();
        tag.DeletionTime = DateTime.UtcNow;
        _mockRepository.Setup(r => r.FindAsync(tag.Id, It.IsAny<CancellationToken>())).ReturnsAsync(tag);

        // Act
        var result = await _sut.DeleteAsync(tag.Id, Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }
}
