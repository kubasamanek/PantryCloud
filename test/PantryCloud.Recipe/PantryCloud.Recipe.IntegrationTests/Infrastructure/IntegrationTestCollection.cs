namespace PantryCloud.Recipe.IntegrationTests.Infrastructure;

[CollectionDefinition(nameof(IntegrationTestCollection))]
public class IntegrationTestCollection : ICollectionFixture<RecipeTestFixture>
{
}
