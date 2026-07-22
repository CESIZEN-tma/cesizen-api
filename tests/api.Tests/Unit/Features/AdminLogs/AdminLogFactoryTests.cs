using FluentAssertions;
using api.CZ.Features.AdminLogs.Enums;
using api.CZ.Features.AdminLogs.Factories;

namespace api.Tests.Unit.Features.AdminLogs;

public class AdminLogFactoryTests
{
    private readonly AdminLogFactory _sut = new();

    [Fact]
    public void Create_ValidParameters_PopulatesAllFields()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var entityId = Guid.NewGuid();

        // Act
        var log = _sut.Create(adminId, AdminActionCode.ADMIN_CREATED, "Administrator", entityId, "created admin");

        // Assert
        log.Id.Should().NotBeEmpty();
        log.IdAdministrator.Should().Be(adminId);
        log.ActionCode.Should().Be("ADMIN_CREATED");
        log.EntityType.Should().Be("Administrator");
        log.TargetedEntityId.Should().Be(entityId);
        log.Description.Should().Be("created admin");
        log.CreationTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
