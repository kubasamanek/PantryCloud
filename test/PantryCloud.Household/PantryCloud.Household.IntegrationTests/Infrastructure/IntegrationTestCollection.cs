namespace PantryCloud.Household.IntegrationTests.Infrastructure;

/// <summary>
/// Defines a test collection so all Household integration tests share the same
/// fixture and run sequentially to avoid database and port conflicts.
/// </summary>
[CollectionDefinition(nameof(IntegrationTestCollection))]
public class IntegrationTestCollection : ICollectionFixture<HouseholdTestFixture>
{
}
