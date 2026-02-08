using System.Net.Http.Json;

namespace PantryCloud.Web.Services.Household;

public class HouseholdApiService(IHttpClientFactory httpClientFactory) : IHouseholdApi
{
    private const string BasePath = "api/household/api/households";

    private HttpClient Client => httpClientFactory.CreateClient("Gateway");

    public async Task<GetCurrentHouseholdResponse?> GetCurrentHouseholdAsync(CancellationToken cancellationToken = default)
    {
        var response = await Client.GetAsync($"{BasePath}/me", cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<GetCurrentHouseholdResponse>(cancellationToken)
            : null;
    }

    public async Task<CreateHouseholdResponse?> CreateHouseholdAsync(CreateHouseholdRequest request, CancellationToken cancellationToken = default)
    {
        var response = await Client.PostAsJsonAsync(BasePath, request, cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<CreateHouseholdResponse>(cancellationToken)
            : null;
    }
}
