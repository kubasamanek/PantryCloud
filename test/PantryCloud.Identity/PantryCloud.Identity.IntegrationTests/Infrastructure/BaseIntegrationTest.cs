using PantryCloud.Identity.Core.Entities;
using PantryCloud.Identity.Infrastructure.Persistence;
using PantryCloud.SharedKernel.Testing.Infrastructure;

namespace PantryCloud.Identity.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for Identity service integration tests.
/// Call <see cref="ResetAsync"/> at the start of each test for a clean database.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public abstract class BaseIntegrationTest(IdentityTestFixture fixture)
{
    protected IdentityTestFixture Fixture { get; } = fixture;
    protected HttpClient HttpClient => Fixture.HttpClient;

    protected Task ResetAsync() => Fixture.ResetDatabaseAsync();

    protected Task<ApplicationUser> SeedUserAsync(string email, string password, bool verified = true)
        => Fixture.SeedUserAsync(email, password, verified);

    protected Task<string> SeedRefreshTokenAsync(Guid userId, bool expired = false)
        => Fixture.SeedRefreshTokenAsync(userId, expired);

    protected Task<VerifyEmailToken> SeedVerifyEmailTokenAsync(string email, bool used = false, bool expired = false)
        => Fixture.SeedVerifyEmailTokenAsync(email, used, expired);

    protected Task<ResetPasswordToken> SeedResetPasswordTokenAsync(string email, bool used = false, bool expired = false)
        => Fixture.SeedResetPasswordTokenAsync(email, used, expired);

    protected Task<ApplicationUser?> GetUserByEmailAsync(string email) => Fixture.GetUserByEmailAsync(email);
    protected Task<ApplicationUser?> GetUserByIdAsync(Guid userId) => Fixture.GetUserByIdAsync(userId);
    protected Task<RefreshSession?> GetSessionByRefreshTokenAsync(string refreshToken) => Fixture.GetSessionByRefreshTokenAsync(refreshToken);
    protected Task<VerifyEmailToken?> GetVerifyEmailTokenAsync(string email, string token) => Fixture.GetVerifyEmailTokenAsync(email, token);
    protected Task<ResetPasswordToken?> GetResetPasswordTokenAsync(string email, string token) => Fixture.GetResetPasswordTokenAsync(email, token);
    protected Task<IReadOnlyList<ResetPasswordToken>> GetResetPasswordTokensByEmailAsync(string email) => Fixture.GetResetPasswordTokensByEmailAsync(email);

    protected Task<T?> GetFromJsonAsync<T>(HttpResponseMessage response) => HttpTestHelpers.GetFromJsonAsync<T>(response);
}
