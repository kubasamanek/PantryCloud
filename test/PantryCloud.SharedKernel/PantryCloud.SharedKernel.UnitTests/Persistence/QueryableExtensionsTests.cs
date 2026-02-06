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
            new() { Name = Constants.Queryable.Apple },
            new() { Name = "BANANA" },
            new() { Name = Constants.Queryable.Cherry },
            new() { Name = Constants.Queryable.Pineapple }
        }.AsQueryable();

        // Act
        var result = items.WhereContainsCaseInsensitive(x => x.Name, Constants.Queryable.AppSearchTerm).ToList();

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldContain(x => x.Name == Constants.Queryable.Apple);
        result.ShouldContain(x => x.Name == Constants.Queryable.Pineapple);
    }

    [Fact]
    public void WhereContainsCaseInsensitive_ShouldReturnAllItems_WhenSearchTermIsNull()
    {
        // Arrange
        var items = new List<TestEntity>
        {
            new() { Name = Constants.Queryable.Apple },
            new() { Name = Constants.Queryable.Banana },
            new() { Name = Constants.Queryable.Cherry }
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
            new() { Name = Constants.Queryable.Apple },
            new() { Name = Constants.Queryable.Banana },
            new() { Name = Constants.Queryable.Cherry }
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
            new() { Name = Constants.Queryable.Apple },
            new() { Name = Constants.Queryable.Banana },
            new() { Name = Constants.Queryable.Cherry }
        }.AsQueryable();

        // Act
        var result = items.WhereContainsCaseInsensitive(x => x.Name, Constants.Queryable.WhitespaceSearchTerm).ToList();

        // Assert
        result.Count.ShouldBe(3);
    }

    [Fact]
    public void WhereContainsCaseInsensitive_ShouldReturnEmpty_WhenNoMatches()
    {
        // Arrange
        var items = new List<TestEntity>
        {
            new() { Name = Constants.Queryable.Apple },
            new() { Name = Constants.Queryable.Banana },
            new() { Name = Constants.Queryable.Cherry }
        }.AsQueryable();

        // Act
        var result = items.WhereContainsCaseInsensitive(x => x.Name, Constants.Queryable.XyzSearchTerm).ToList();

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
        var result = items.WhereContainsCaseInsensitive(x => x.Name, Constants.Queryable.AppleSearchTerm).ToList();

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
        var result = items.WhereContainsCaseInsensitive(x => x.Name, Constants.Queryable.AppleSearchTermUpper).ToList();

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
            new() { Name = Constants.Queryable.Pineapple },
            new() { Name = Constants.Queryable.Apple },
            new() { Name = Constants.Queryable.Grapple }
        }.AsQueryable();

        // Act
        var result = items.WhereContainsCaseInsensitive(x => x.Name, Constants.Queryable.AppSearchTerm).ToList();

        // Assert
        result.Count.ShouldBe(3);
    }

    private class TestEntity
    {
        public string Name { get; init; } = string.Empty;
    }
}

