using PantryCloud.SharedKernel.Entities;
using Shouldly;

namespace PantryCloud.SharedKernel.UnitTests.Entities;

public class BaseEntityTests
{
    [Fact]
    public void Constructor_ShouldGenerateUniqueId()
    {
        // Act
        var entity = new TestEntity();

        // Assert
        entity.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_ShouldGenerateDifferentIds_ForDifferentInstances()
    {
        // Act
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();

        // Assert
        entity1.Id.ShouldNotBe(entity2.Id);
    }

    [Fact]
    public void Id_ShouldBeInitOnly()
    {
        // Arrange
        var customId = Guid.NewGuid();

        // Act
        var entity = new TestEntity { Id = customId };

        // Assert
        entity.Id.ShouldBe(customId);
    }

    [Fact]
    public void Entity_ShouldImplementIEntity()
    {
        // Arrange
        var entity = new TestEntity();

        // Assert
        entity.ShouldBeAssignableTo<IEntity>();
    }

    private class TestEntity : BaseEntity
    {
    }
}

