using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using PantryCloud.E2E.Runner.Configuration;
using PantryCloud.ShoppingList.Application.Dtos;
using PantryCloud.ShoppingList.Core.Enums;

namespace PantryCloud.E2E.Runner.HttpClients;

public sealed class ShoppingListApi(HttpClient client, E2ESettings settings)
{
    private string VersionedBasePath() =>
        $"/api/v{settings.Gateway.ApiVersion}/shoppinglist/shopping-lists";

    private void SetAuthHeader(string accessToken)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);
    }

    public async Task<CreateShoppingListResponseDto> CreateListAsync(
        string accessToken,
        string name,
        CancellationToken cancellationToken)
    {
        SetAuthHeader(accessToken);

        var request = new CreateShoppingListRequestDto(name);

        const int maxAttempts = 12;
        const int delayMilliseconds = 1500;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            using var response = await client.PostAsJsonAsync(
                VersionedBasePath(),
                request,
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound && attempt < maxAttempts)
            {
                await Task.Delay(delayMilliseconds, cancellationToken);
                continue;
            }

            if (response.StatusCode != HttpStatusCode.Created)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(
                    $"CreateShoppingList failed with status {(int)response.StatusCode}: {body}");
            }

            var result = await response.Content.ReadFromJsonAsync<CreateShoppingListResponseDto>(cancellationToken: cancellationToken);
            if (result is null || result.Id == Guid.Empty)
            {
                throw new InvalidOperationException("CreateShoppingList response is missing list id.");
            }

            return result;
        }

        throw new InvalidOperationException("CreateShoppingList failed with 404 after multiple attempts.");
    }

    public async Task<GetShoppingListResponseDto> GetListAsync(
        string accessToken,
        Guid listId,
        CancellationToken cancellationToken)
    {
        SetAuthHeader(accessToken);

        using var response = await client.GetAsync(
            $"{VersionedBasePath()}/{listId}",
            cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"GetShoppingList failed with status {(int)response.StatusCode}: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<GetShoppingListResponseDto>(cancellationToken: cancellationToken);
        if (result is null)
        {
            throw new InvalidOperationException("GetShoppingList response is null.");
        }

        return result;
    }

    public async Task<AddShoppingListItemResponseDto> AddItemAsync(
        string accessToken,
        Guid listId,
        string name,
        decimal quantity,
        PantryCloud.SharedKernel.Enums.Unit unit,
        CancellationToken cancellationToken)
    {
        SetAuthHeader(accessToken);

        var request = new AddShoppingListItemRequestDto(name, quantity, unit, Source: ItemSource.Manual);

        using var response = await client.PostAsJsonAsync(
            $"{VersionedBasePath()}/{listId}/items",
            request,
            cancellationToken);

        if (response.StatusCode != HttpStatusCode.Created)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"AddShoppingListItem failed with status {(int)response.StatusCode}: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<AddShoppingListItemResponseDto>(cancellationToken: cancellationToken);
        if (result is null || result.Id == Guid.Empty)
        {
            throw new InvalidOperationException("AddShoppingListItem response is missing item id.");
        }

        return result;
    }

    public async Task<CheckShoppingListItemResponseDto> CheckItemAsync(
        string accessToken,
        Guid listId,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        SetAuthHeader(accessToken);

        using var response = await client.PatchAsync(
            $"{VersionedBasePath()}/{listId}/items/{itemId}/check",
            null,
            cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"CheckShoppingListItem failed with status {(int)response.StatusCode}: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<CheckShoppingListItemResponseDto>(cancellationToken: cancellationToken);
        if (result is null)
        {
            throw new InvalidOperationException("CheckShoppingListItem response is null.");
        }

        return result;
    }
}
