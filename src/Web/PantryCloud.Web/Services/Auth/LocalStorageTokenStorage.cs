using Microsoft.JSInterop;

namespace PantryCloud.Web.Services.Auth;

public class LocalStorageTokenStorage(IJSRuntime jsRuntime) : ITokenStorage
{
    private const string AccessTokenKey = "pantrycloud_access_token";
    private const string RefreshTokenKey = "pantrycloud_refresh_token";

    public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default) =>
        await jsRuntime.InvokeAsync<string?>("getLocalStorage", cancellationToken, AccessTokenKey);

    public async Task<string?> GetRefreshTokenAsync(CancellationToken cancellationToken = default) =>
        await jsRuntime.InvokeAsync<string?>("getLocalStorage", cancellationToken, RefreshTokenKey);

    public async Task SetTokensAsync(string accessToken, string refreshToken, CancellationToken cancellationToken = default)
    {
        await jsRuntime.InvokeVoidAsync("setLocalStorage", cancellationToken, AccessTokenKey, accessToken);
        await jsRuntime.InvokeVoidAsync("setLocalStorage", cancellationToken, RefreshTokenKey, refreshToken);
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await jsRuntime.InvokeVoidAsync("removeLocalStorage", cancellationToken, AccessTokenKey);
        await jsRuntime.InvokeVoidAsync("removeLocalStorage", cancellationToken, RefreshTokenKey);
    }
}
