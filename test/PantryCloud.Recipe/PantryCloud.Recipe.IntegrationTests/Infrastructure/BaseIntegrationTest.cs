namespace PantryCloud.Recipe.IntegrationTests.Infrastructure;

[Collection(nameof(IntegrationTestCollection))]
public abstract class BaseIntegrationTest(RecipeTestFixture fixture)
{
    protected RecipeTestFixture Fixture { get; } = fixture;

    protected HttpClient CreateClientWithToken(Guid userId, string? email = null) =>
        Fixture.CreateClientWithToken(userId, email);

    protected static async Task<T?> GetFromJsonAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken = default) =>
        await RecipeTestFixture.GetFromJsonAsync<T>(response, cancellationToken);
}
