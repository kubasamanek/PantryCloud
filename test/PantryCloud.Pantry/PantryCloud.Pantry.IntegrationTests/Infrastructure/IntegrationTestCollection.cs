namespace PantryCloud.Pantry.IntegrationTests.Infrastructure;

[CollectionDefinition(nameof(IntegrationTestCollection))]
public class IntegrationTestCollection : ICollectionFixture<PantryTestFixture>
{
}
