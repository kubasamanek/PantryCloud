namespace PantryCloud.Identity.IntegrationTests.Infrastructure;

/// <summary>
/// Defines a test collection that ensures all integration tests share the same
/// fixture and run sequentially to avoid database conflicts.
/// </summary>
[CollectionDefinition(nameof(IntegrationTestCollection))]
public class IntegrationTestCollection : ICollectionFixture<IdentityTestFixture>
{
}

