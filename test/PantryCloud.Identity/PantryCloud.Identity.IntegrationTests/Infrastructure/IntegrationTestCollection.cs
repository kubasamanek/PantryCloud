namespace PantryCloud.Identity.IntegrationTests.Infrastructure;

/// <summary>
/// Defines a test collection that ensures all integration tests share the same
/// WebApplicationFactory instance and run sequentially to avoid database conflicts.
/// </summary>
[CollectionDefinition(nameof(IntegrationTestCollection))]
public class IntegrationTestCollection : ICollectionFixture<IdentityIntegrationTestWebAppFactory>
{
    // This class is never instantiated. It's just used to define the collection.
}

