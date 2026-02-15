namespace PantryCloud.Pantry.IntegrationTests.Infrastructure;

[Collection(nameof(IntegrationTestCollection))]
public abstract class BaseIntegrationTest(PantryTestFixture fixture)
{
    protected PantryTestFixture Fixture { get; } = fixture;

    protected Task ResetAsync() => Fixture.ResetAsync();

    protected Task SeedHouseholdMembershipAsync(Guid userId, Guid householdId) =>
        Fixture.SeedHouseholdMembershipAsync(userId, householdId);

    protected HttpClient CreateClientWithToken(Guid userId, string? email = null) =>
        Fixture.CreateClientWithToken(userId, email);

    protected static async Task<T?> GetFromJsonAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken = default) =>
        await PantryTestFixture.GetFromJsonAsync<T>(response, cancellationToken);
}
