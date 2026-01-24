using PantryCloud.SharedKernel.Entities;
using Shouldly;

namespace PantryCloud.SharedKernel.UnitTests.Entities;

public class AuditableEntityTests
{
    [Fact]
    public void Constructor_ShouldInheritFromBaseEntity()
    {
        // Act
        var entity = new TestAuditableEntity();

        // Assert
        entity.ShouldBeAssignableTo<BaseEntity>();
        entity.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void CreatedBy_ShouldBeSettable()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entity = new TestAuditableEntity();

        // Act
        entity.CreatedBy = userId;

        // Assert
        entity.CreatedBy.ShouldBe(userId);
    }

    [Fact]
    public void CreatedAt_ShouldBeSettable()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;
        var entity = new TestAuditableEntity();

        // Act
        entity.CreatedAt = timestamp;

        // Assert
        entity.CreatedAt.ShouldBe(timestamp);
    }

    [Fact]
    public void ModifiedBy_ShouldBeNullable()
    {
        // Arrange
        var entity = new TestAuditableEntity();

        // Assert
        entity.ModifiedBy.ShouldBeNull();
    }

    [Fact]
    public void ModifiedBy_ShouldBeSettable()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entity = new TestAuditableEntity();

        // Act
        entity.ModifiedBy = userId;

        // Assert
        entity.ModifiedBy.ShouldBe(userId);
    }

    [Fact]
    public void ModifiedAt_ShouldBeNullable()
    {
        // Arrange
        var entity = new TestAuditableEntity();

        // Assert
        entity.ModifiedAt.ShouldBeNull();
    }

    [Fact]
    public void ModifiedAt_ShouldBeSettable()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;
        var entity = new TestAuditableEntity();

        // Act
        entity.ModifiedAt = timestamp;

        // Assert
        entity.ModifiedAt.ShouldBe(timestamp);
    }

    [Fact]
    public void AuditableEntity_ShouldHaveAllAuditProperties()
    {
        // Arrange
        var createdBy = Guid.NewGuid();
        var modifiedBy = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var modifiedAt = DateTime.UtcNow;

        // Act
        var entity = new TestAuditableEntity
        {
            CreatedBy = createdBy,
            CreatedAt = createdAt,
            ModifiedBy = modifiedBy,
            ModifiedAt = modifiedAt
        };

        // Assert
        entity.CreatedBy.ShouldBe(createdBy);
        entity.CreatedAt.ShouldBe(createdAt);
        entity.ModifiedBy.ShouldBe(modifiedBy);
        entity.ModifiedAt.ShouldBe(modifiedAt);
    }

    private class TestAuditableEntity : AuditableEntity
    {
    }
}

