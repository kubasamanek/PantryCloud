namespace PantryCloud.ShoppingList.IntegrationTests.Infrastructure;

[CollectionDefinition(nameof(IntegrationTestCollection))]
public class IntegrationTestCollection : ICollectionFixture<ShoppingListTestFixture>
{
}
