using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using PantryCloud.Web.Services.Auth;

namespace PantryCloud.Web.Services;

public class GatewayAuthorizationMessageHandler(
    ITokenStorage tokenStorage,
    IAuthApi authApi,
    TokenAuthenticationStateProvider authStateProvider,
    IOptions<GatewayOptions> options,
    NavigationManager navigation)
    : DelegatingHandler
{
    private readonly string _gatewayBaseUrl = options.Value.BaseUrl.TrimEnd('/');

    private NavigationManager Navigation { get; } = navigation;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (IsGatewayRequest(request.RequestUri))
        {
            var accessToken = await tokenStorage.GetAccessTokenAsync(cancellationToken);
            if (!string.IsNullOrEmpty(accessToken))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && IsGatewayRequest(request.RequestUri))
        {
            var refreshToken = await tokenStorage.GetRefreshTokenAsync(cancellationToken);
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var refreshResponse = await authApi.RefreshAsync(new RefreshTokenRequest(refreshToken), cancellationToken);
                if (refreshResponse != null)
                {
                    await tokenStorage.SetTokensAsync(refreshResponse.AccessToken, refreshResponse.RefreshToken, cancellationToken);
                    if (request.Method == HttpMethod.Get && request.RequestUri != null)
                    {
                        var retryRequest = new HttpRequestMessage(HttpMethod.Get, request.RequestUri);
                        retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshResponse.AccessToken);
                        response = await base.SendAsync(retryRequest, cancellationToken);
                    }
                    if (response.IsSuccessStatusCode)
                        return response;
                    // Refresh succeeded but retry still failed (e.g. 401 from another service). Return response without logging out.
                    return response;
                }
            }

            await tokenStorage.ClearAsync(cancellationToken);
            authStateProvider.NotifyLoggedOut();
            Navigation.NavigateTo("/login", forceLoad: true);
        }

        return response;
    }

    private bool IsGatewayRequest(Uri? requestUri)
    {
        if (requestUri == null || !requestUri.IsAbsoluteUri) return false;
        var gateway = _gatewayBaseUrl.TrimEnd('/');
        var uri = requestUri.GetLeftPart(UriPartial.Authority).TrimEnd('/');
        return uri.Equals(gateway, StringComparison.OrdinalIgnoreCase)
            || requestUri.AbsoluteUri.StartsWith(_gatewayBaseUrl + "/", StringComparison.OrdinalIgnoreCase);
    }
}
