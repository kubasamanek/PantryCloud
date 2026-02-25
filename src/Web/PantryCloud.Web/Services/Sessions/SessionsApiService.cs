using System.Net.Http.Json;

namespace PantryCloud.Web.Services.Sessions;

public class SessionsApiService(IHttpClientFactory httpClientFactory) : ISessionsApi
{
    private const string BasePath = "api/v1/identity/auth";

    private HttpClient Client => httpClientFactory.CreateClient("Gateway");

    public async Task<ListSessionsResponse?> ListSessionsAsync(CancellationToken cancellationToken = default)
    {
        var response = await Client.GetAsync($"{BasePath}/sessions", cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<ListSessionsResponse>(cancellationToken)
            : null;
    }

    public async Task<bool> RevokeSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var response = await Client.DeleteAsync($"{BasePath}/sessions/{sessionId}", cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RevokeAllOtherSessionsAsync(CancellationToken cancellationToken = default)
    {
        var response = await Client.DeleteAsync($"{BasePath}/sessions/others", cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var response = await Client.PostAsJsonAsync($"{BasePath}/logout", new { refreshToken }, cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
