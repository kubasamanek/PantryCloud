namespace PantryCloud.Household.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for Household integration tests.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public abstract class BaseIntegrationTest(HouseholdTestFixture fixture)
{
    protected HouseholdTestFixture Fixture { get; } = fixture;

    protected Task ResetAsync() => Fixture.ResetAsync();

    protected HttpClient CreateClientWithToken(Guid userId, string? email = null) => Fixture.CreateClientWithToken(userId, email);

    protected static async Task<T?> GetFromJsonAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken = default) =>
        await HouseholdTestFixture.GetFromJsonAsync<T>(response, cancellationToken);
}
