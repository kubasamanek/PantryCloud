using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace PantryCloud.Web.Services.Auth;

public sealed class TokenAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ITokenStorage _tokenStorage;
    private readonly JwtClaimsHelper _jwtHelper;
    private readonly IAuthApi _authApi;

    public TokenAuthenticationStateProvider(
        ITokenStorage tokenStorage,
        JwtClaimsHelper jwtHelper,
        IAuthApi authApi)
    {
        _tokenStorage = tokenStorage;
        _jwtHelper = jwtHelper;
        _authApi = authApi;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var accessToken = await _tokenStorage.GetAccessTokenAsync();
        var (email, exp, _) = _jwtHelper.DecodePayload(accessToken);

        if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(email))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        if (_jwtHelper.IsExpired(exp))
        {
            var refreshToken = await _tokenStorage.GetRefreshTokenAsync();
            if (string.IsNullOrEmpty(refreshToken))
            {
                await _tokenStorage.ClearAsync();
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var refreshResponse = await _authApi.RefreshAsync(new RefreshTokenRequest(refreshToken));
            if (refreshResponse == null)
            {
                await _tokenStorage.ClearAsync();
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            await _tokenStorage.SetTokensAsync(refreshResponse.AccessToken, refreshResponse.RefreshToken);
            var (newEmail, _, _) = _jwtHelper.DecodePayload(refreshResponse.AccessToken);
            return new AuthenticationState(_jwtHelper.BuildPrincipal(newEmail));
        }

        return new AuthenticationState(_jwtHelper.BuildPrincipal(email));
    }

    public void NotifyLoggedIn()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void NotifyLoggedOut()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
