namespace PantryCloud.Web.Services.Auth;

public interface ITokenStorage
{
    Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default);
    Task<string?> GetRefreshTokenAsync(CancellationToken cancellationToken = default);
    Task SetTokensAsync(string accessToken, string refreshToken, CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
}
