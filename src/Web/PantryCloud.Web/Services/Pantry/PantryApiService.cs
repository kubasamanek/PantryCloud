using System.Net;
using System.Net.Http.Json;
using PantryCloud.Web.Constants;

namespace PantryCloud.Web.Services.Pantry;

public class PantryApiService(IHttpClientFactory httpClientFactory) : IPantryApi
{
    private static readonly string BasePath = ApiPaths.Pantry;
    private HttpClient Client => httpClientFactory.CreateClient("Gateway");

    public async Task<ListPantryItemsResult> ListItemsAsync(string? category, string? searchTerm, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = $"{BasePath}/items?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(category))
            query += "&category=" + Uri.EscapeDataString(category);
        if (!string.IsNullOrWhiteSpace(searchTerm))
            query += "&searchTerm=" + Uri.EscapeDataString(searchTerm);
        var response = await Client.GetAsync(query, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<ListPantryItemsResponse>(cancellationToken);
            return new ListPantryItemsResult(data, NoHousehold: false, Unauthorized: false);
        }
        var noHousehold = response.StatusCode == HttpStatusCode.NotFound;
        var unauthorized = response.StatusCode == HttpStatusCode.Unauthorized;
        return new ListPantryItemsResult(null, NoHousehold: noHousehold, Unauthorized: unauthorized);
    }

    public async Task<PantryItemDto?> GetItemAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await Client.GetAsync($"{BasePath}/items/{id}", cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<PantryItemDto>(cancellationToken)
            : null;
    }

    public async Task<CreatePantryItemResponse?> CreateItemAsync(CreatePantryItemRequest request, CancellationToken cancellationToken = default)
    {
        var response = await Client.PostAsJsonAsync($"{BasePath}/items", request, cancellationToken);
        return response.StatusCode == HttpStatusCode.Created
            ? await response.Content.ReadFromJsonAsync<CreatePantryItemResponse>(cancellationToken)
            : null;
    }

    public async Task<PantryUpdateResult> UpdateItemAsync(Guid id, UpdatePantryItemRequest request, CancellationToken cancellationToken = default)
    {
        var response = await Client.PutAsJsonAsync($"{BasePath}/items/{id}", request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Conflict)
            return new PantryUpdateResult { IsConcurrencyConflict = true };
        if (!response.IsSuccessStatusCode)
            return new PantryUpdateResult();
        var value = await response.Content.ReadFromJsonAsync<UpdatePantryItemResponse>(cancellationToken);
        return new PantryUpdateResult { Value = value };
    }

    public async Task<bool> DeleteItemAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await Client.DeleteAsync($"{BasePath}/items/{id}", cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
