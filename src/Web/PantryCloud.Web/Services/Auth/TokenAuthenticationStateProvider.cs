using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace PantryCloud.Web.Services.Auth;

public sealed class TokenAuthenticationStateProvider(
    ITokenStorage tokenStorage,
    JwtClaimsHelper jwtHelper,
    IAuthApi authApi)
    : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var accessToken = await tokenStorage.GetAccessTokenAsync();
            var (email, exp, _) = jwtHelper.DecodePayload(accessToken);

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(email))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            if (jwtHelper.IsExpired(exp))
            {
                var refreshToken = await tokenStorage.GetRefreshTokenAsync();
                if (string.IsNullOrEmpty(refreshToken))
                {
                    await tokenStorage.ClearAsync();
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                var refreshResponse = await authApi.RefreshAsync(new RefreshTokenRequest(refreshToken));
                if (refreshResponse == null || string.IsNullOrEmpty(refreshResponse.AccessToken) || string.IsNullOrEmpty(refreshResponse.RefreshToken))
                {
                    await tokenStorage.ClearAsync();
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                await tokenStorage.SetTokensAsync(refreshResponse.AccessToken, refreshResponse.RefreshToken);
                var (newEmail, _, _) = jwtHelper.DecodePayload(refreshResponse.AccessToken);
                return new AuthenticationState(jwtHelper.BuildPrincipal(newEmail));
            }

            return new AuthenticationState(jwtHelper.BuildPrincipal(email));
        }
        catch
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
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
