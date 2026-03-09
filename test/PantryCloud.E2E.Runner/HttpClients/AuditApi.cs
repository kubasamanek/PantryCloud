using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using PantryCloud.Audit.Application.Dtos;
using PantryCloud.E2E.Runner.Configuration;

namespace PantryCloud.E2E.Runner.HttpClients;

public sealed class AuditApi(HttpClient client, E2ESettings settings)
{
    private string VersionedBasePath() =>
        $"/api/v{settings.Gateway.ApiVersion}/audit";

    private void SetAuthHeader(string accessToken)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);
    }

    public async Task<ListAuditEntriesResponseDto> ListHouseholdEntriesAsync(
        string accessToken,
        Guid householdId,
        DateTime? from = null,
        DateTime? to = null,
        string? actionType = null,
        string? entityType = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        SetAuthHeader(accessToken);

        var queryParams = new List<string> { $"page={page}", $"pageSize={pageSize}" };
        if (from.HasValue) queryParams.Add($"from={from.Value:O}");
        if (to.HasValue) queryParams.Add($"to={to.Value:O}");
        if (!string.IsNullOrEmpty(actionType)) queryParams.Add($"actionType={Uri.EscapeDataString(actionType)}");
        if (!string.IsNullOrEmpty(entityType)) queryParams.Add($"entityType={Uri.EscapeDataString(entityType)}");

        var query = "?" + string.Join("&", queryParams);

        using var response = await client.GetAsync(
            $"{VersionedBasePath()}/households/{householdId}/entries{query}",
            cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"ListHouseholdAuditEntries failed with status {(int)response.StatusCode}: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<ListAuditEntriesResponseDto>(cancellationToken: cancellationToken);
        if (result is null)
        {
            throw new InvalidOperationException("ListHouseholdAuditEntries response is null.");
        }

        return result;
    }
}
