using PantryCloud.SharedKernel.Persistence;
using Shouldly;

namespace PantryCloud.SharedKernel.UnitTests.Persistence;

public class QueryableExtensionsTests
{
    [Fact]
    public void WhereContainsCaseInsensitive_ShouldReturnMatchingItems_CaseInsensitive()
    {
        // Arrange
        var items = new List<TestEntity>
        {
            new() { Name = "Apple" },
            new() { Name = "BANANA" },
            new() { Name = "cherry" },
            new() { Name = "Pineapple" }  // Changed: "Pineapple" contains "app"
        }.AsQueryable();

        // Act
        var result = items.WhereContainsCaseInsensitive(x => x.Name, "app").ToList();

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldContain(x => x.Name == "Apple");
        result.ShouldContain(x => x.Name == "Pineapple");  // Both "Apple" and "Pineapple" contain "app"
    }

    [Fact]
    public void WhereContainsCaseInsensitive_ShouldReturnAllItems_WhenSearchTermIsNull()
    {
        // Arrange
        var items = new List<TestEntity>
        {
            new() { Name = "Apple" },
            new() { Name = "Banana" },
            new() { Name = "Cherry" }
        }.AsQueryable();

        // Act
        var result = items.WhereContainsCaseInsensitive(x => x.Name, null!).ToList();

        // Assert
        result.Count.ShouldBe(3);
    }

    [Fact]
    public void WhereContainsCaseInsensitive_ShouldReturnAllItems_WhenSearchTermIsEmpty()
    {
        // Arrange
        var items = new List<TestEntity>
        {
            new() { Name = "Apple" },
            new() { Name = "Banana" },
            new() { Name = "Cherry" }
        }.AsQueryable();

        // Act
        var result = items.WhereContainsCaseInsensitive(x => x.Name, "").ToList();

        // Assert
        result.Count.ShouldBe(3);
    }

    [Fact]
    public void WhereContainsCaseInsensitive_ShouldReturnAllItems_WhenSearchTermIsWhitespace()
    {
        // Arrange
        var items = new List<TestEntity>
        {
            new() { Name = "Apple" },
            new() { Name = "Banana" },
            new() { Name = "Cherry" }
        }.AsQueryable();

        // Act
        var result = items.WhereContainsCaseInsensitive(x => x.Name, "   ").ToList();

        // Assert
        result.Count.ShouldBe(3);
    }

    [Fact]
    public void WhereContainsCaseInsensitive_ShouldReturnEmpty_WhenNoMatches()
    {
        // Arrange
        var items = new List<TestEntity>
        {
            new() { Name = "Apple" },
            new() { Name = "Banana" },
            new() { Name = "Cherry" }
        }.AsQueryable();

        // Act
        var result = items.WhereContainsCaseInsensitive(x => x.Name, "xyz").ToList();

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void WhereContainsCaseInsensitive_ShouldMatchUpperCase_WhenSearchTermIsLowerCase()
    {
        // Arrange
        var items = new List<TestEntity>
        {
            new() { Name = "APPLE" },
            new() { Name = "BANANA" }
        }.AsQueryable();

        // Act
        var result = items.WhereContainsCaseInsensitive(x => x.Name, "apple").ToList();

        // Assert
        result.Count.ShouldBe(1);
        result.First().Name.ShouldBe("APPLE");
    }

    [Fact]
    public void WhereContainsCaseInsensitive_ShouldMatchLowerCase_WhenSearchTermIsUpperCase()
    {
        // Arrange
        var items = new List<TestEntity>
        {
            new() { Name = "apple" },
            new() { Name = "banana" }
        }.AsQueryable();

        // Act
        var result = items.WhereContainsCaseInsensitive(x => x.Name, "APPLE").ToList();

        // Assert
        result.Count.ShouldBe(1);
        result.First().Name.ShouldBe("apple");
    }

    [Fact]
    public void WhereContainsCaseInsensitive_ShouldMatchPartialString()
    {
        // Arrange
        var items = new List<TestEntity>
        {
            new() { Name = "Pineapple" },
            new() { Name = "Apple" },
            new() { Name = "Grapple" }
        }.AsQueryable();

        // Act
        var result = items.WhereContainsCaseInsensitive(x => x.Name, "app").ToList();

        // Assert
        result.Count.ShouldBe(3);
    }

    private class TestEntity
    {
        public string Name { get; init; } = string.Empty;
    }
}

