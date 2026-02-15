using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PantryCloud.Web.Constants;

namespace PantryCloud.Web.Services.ShoppingList;

public class ShoppingListApiService(IHttpClientFactory httpClientFactory) : IShoppingListApi
{
    private static readonly string BasePath = ApiPaths.ShoppingList;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private HttpClient Client => httpClientFactory.CreateClient("Gateway");

    public async Task<ListShoppingListsResult> ListListsAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var response = await Client.GetAsync($"{BasePath}?page={page}&pageSize={pageSize}", cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<ListShoppingListsResponse>(JsonOptions, cancellationToken);
            return new ListShoppingListsResult(data, NoHousehold: false, Unauthorized: false);
        }
        var noHousehold = response.StatusCode == HttpStatusCode.NotFound;
        var unauthorized = response.StatusCode == HttpStatusCode.Unauthorized;
        return new ListShoppingListsResult(null, NoHousehold: noHousehold, Unauthorized: unauthorized);
    }

    public async Task<GetShoppingListResult> GetListAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await Client.GetAsync($"{BasePath}/{id}", cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<GetShoppingListResponse>(JsonOptions, cancellationToken);
            return new GetShoppingListResult(data, NoHousehold: false, Unauthorized: false);
        }
        var noHousehold = response.StatusCode == HttpStatusCode.NotFound;
        var unauthorized = response.StatusCode == HttpStatusCode.Unauthorized;
        return new GetShoppingListResult(null, NoHousehold: noHousehold, Unauthorized: unauthorized);
    }

    public async Task<CreateShoppingListResult> CreateListAsync(CreateShoppingListRequest request, CancellationToken cancellationToken = default)
    {
        var response = await Client.PostAsJsonAsync(BasePath, request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Created)
        {
            var value = await response.Content.ReadFromJsonAsync<CreateShoppingListResponse>(cancellationToken);
            return new CreateShoppingListResult { Value = value };
        }
        if (response.StatusCode == HttpStatusCode.Conflict)
            return new CreateShoppingListResult { IsDuplicateName = true };
        return new CreateShoppingListResult();
    }

    public async Task<bool> DeleteListAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await Client.DeleteAsync($"{BasePath}/{id}", cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<AddShoppingListItemResponse?> AddItemAsync(Guid listId, AddShoppingListItemRequest request, CancellationToken cancellationToken = default)
    {
        var response = await Client.PostAsJsonAsync($"{BasePath}/{listId}/items", request, cancellationToken);
        return response.StatusCode == HttpStatusCode.Created
            ? await response.Content.ReadFromJsonAsync<AddShoppingListItemResponse>(cancellationToken)
            : null;
    }

    public async Task<ShoppingListItemUpdateResult> UpdateItemAsync(Guid listId, Guid itemId, UpdateShoppingListItemRequest request, CancellationToken cancellationToken = default)
    {
        var response = await Client.PutAsJsonAsync($"{BasePath}/{listId}/items/{itemId}", request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Conflict)
            return new ShoppingListItemUpdateResult { IsConcurrencyConflict = true };
        if (!response.IsSuccessStatusCode)
            return new ShoppingListItemUpdateResult();
        var value = await response.Content.ReadFromJsonAsync<UpdateShoppingListItemResponse>(cancellationToken);
        return new ShoppingListItemUpdateResult { Value = value };
    }

    public async Task<bool> DeleteItemAsync(Guid listId, Guid itemId, CancellationToken cancellationToken = default)
    {
        var response = await Client.DeleteAsync($"{BasePath}/{listId}/items/{itemId}", cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<CheckShoppingListItemResponse?> CheckItemAsync(Guid listId, Guid itemId, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, $"{BasePath}/{listId}/items/{itemId}/check");
        var response = await Client.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<CheckShoppingListItemResponse>(cancellationToken)
            : null;
    }
}
