using System.Net.Http.Json;
using System.Text;
using PantryCloud.Web.Constants;

namespace PantryCloud.Web.Services.Audit;

public class AuditApiService(IHttpClientFactory httpClientFactory) : IAuditApi
{
    private const string BasePath = ApiPaths.Audit;
    private HttpClient Client => httpClientFactory.CreateClient("Gateway");

    public async Task<ListAuditEntriesResponseDto?> ListHouseholdAuditEntriesAsync(
        Guid householdId,
        DateTime? from = null,
        DateTime? to = null,
        string? actionType = null,
        string? entityType = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = new StringBuilder($"{BasePath}/households/{householdId}/entries?page={page}&pageSize={pageSize}");

        if (from.HasValue)
        {
            query.Append($"&from={Uri.EscapeDataString(from.Value.ToString("O"))}");
        }

        if (to.HasValue)
        {
            query.Append($"&to={Uri.EscapeDataString(to.Value.ToString("O"))}");
        }

        if (!string.IsNullOrWhiteSpace(actionType))
        {
            query.Append($"&actionType={Uri.EscapeDataString(actionType)}");
        }

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            query.Append($"&entityType={Uri.EscapeDataString(entityType)}");
        }

        var response = await Client.GetAsync(query.ToString(), cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<ListAuditEntriesResponseDto>(cancellationToken: cancellationToken);
        }

        return null;
    }
}
