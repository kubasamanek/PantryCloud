using System.Net.Http.Json;
using PantryCloud.Web.Constants;

namespace PantryCloud.Web.Services.Notification;

public class NotificationApiService(IHttpClientFactory httpClientFactory) : INotificationApi
{
    private static readonly string BasePath = ApiPaths.Notification;
    private HttpClient Client => httpClientFactory.CreateClient("Gateway");

    public async Task<IReadOnlyList<NotificationMessage>?> GetMyNotificationsAsync(int limit = 50, DateTime? since = null, CancellationToken cancellationToken = default)
    {
        var query = $"{BasePath}/me?limit={Math.Clamp(limit, 1, 100)}";
        if (since.HasValue)
            query += "&since=" + Uri.EscapeDataString(since.Value.ToString("O"));
        var response = await Client.GetAsync(query, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;
        var list = await response.Content.ReadFromJsonAsync<List<NotificationMessage>>(cancellationToken);
        return list ?? [];
    }
}
