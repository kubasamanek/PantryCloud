using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using PantryCloud.E2E.Runner.Configuration;
using PantryCloud.Notification.Core.Dtos;

namespace PantryCloud.E2E.Runner.HttpClients;

public sealed class NotificationApi(HttpClient client, E2ESettings settings)
{
    private string VersionedBasePath() =>
        $"/api/v{settings.Gateway.ApiVersion}/notification/notifications";

    private void SetAuthHeader(string accessToken)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);
    }

    public async Task<IReadOnlyList<NotificationDto>> GetMyNotificationsAsync(
        string accessToken,
        int limit = 50,
        DateTime? since = null,
        CancellationToken cancellationToken = default)
    {
        SetAuthHeader(accessToken);

        var query = since.HasValue
            ? $"?limit={limit}&since={since.Value:O}"
            : $"?limit={limit}";

        using var response = await client.GetAsync(
            $"{VersionedBasePath()}/me{query}",
            cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"GetMyNotifications failed with status {(int)response.StatusCode}: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<List<NotificationDto>>(cancellationToken: cancellationToken);
        return result ?? (IReadOnlyList<NotificationDto>)Array.Empty<NotificationDto>();
    }
}
