using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using PantryCloud.E2E.Runner.Configuration;
using PantryCloud.Pantry.Application.Dtos;
using PantryCloud.SharedKernel.Enums;

namespace PantryCloud.E2E.Runner.HttpClients;

public sealed class PantryApi(HttpClient client, E2ESettings settings)
{
    private string VersionedBasePath() =>
        $"/api/v{settings.Gateway.ApiVersion}/pantry";

    private void SetAuthHeader(string accessToken)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);
    }

    public async Task<CreatePantryItemResponseDto> CreateItemAsync(
        string accessToken,
        string name,
        decimal quantity,
        Unit unit,
        string? category,
        CancellationToken cancellationToken)
    {
        SetAuthHeader(accessToken);

        var request = new CreatePantryItemRequestDto(
            name,
            quantity,
            unit,
            ExpirationDate: null,
            Category: category,
            Notes: null,
            ImageUrl: null);

        const int maxAttempts = 12;
        const int delayMilliseconds = 1500;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            using var response = await client.PostAsJsonAsync(
                $"{VersionedBasePath()}/items",
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
                    $"CreatePantryItem failed with status {(int)response.StatusCode}: {body}");
            }

            var result = await response.Content.ReadFromJsonAsync<CreatePantryItemResponseDto>(cancellationToken: cancellationToken);
            if (result is null || result.Id == Guid.Empty)
            {
                throw new InvalidOperationException("CreatePantryItem response is missing item id.");
            }

            return result;
        }

        throw new InvalidOperationException("CreatePantryItem failed with 404 after multiple attempts.");
    }

    public async Task<ListPantryItemsResponseDto> GetItemsAsync(
        string accessToken,
        CancellationToken cancellationToken)
    {
        SetAuthHeader(accessToken);

        using var response = await client.GetAsync(
            $"{VersionedBasePath()}/items",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return new ListPantryItemsResponseDto(Array.Empty<PantryItemDto>(), 0, 1, 50);
        }

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"ListPantryItems failed with status {(int)response.StatusCode}: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<ListPantryItemsResponseDto>(cancellationToken: cancellationToken);
        if (result is null)
        {
            throw new InvalidOperationException("ListPantryItems response is null.");
        }

        return result;
    }

    public async Task<UpdatePantryItemResponseDto> UpdateItemAsync(
        string accessToken,
        Guid id,
        string name,
        decimal quantity,
        Unit unit,
        byte[] rowVersion,
        string? category,
        CancellationToken cancellationToken)
    {
        SetAuthHeader(accessToken);

        var request = new UpdatePantryItemRequestDto(
            name,
            quantity,
            unit,
            ExpirationDate: null,
            Category: category,
            Notes: null,
            ImageUrl: null,
            RowVersion: rowVersion);

        using var response = await client.PutAsJsonAsync(
            $"{VersionedBasePath()}/items/{id}",
            request,
            cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"UpdatePantryItem failed with status {(int)response.StatusCode}: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<UpdatePantryItemResponseDto>(cancellationToken: cancellationToken);
        if (result is null)
        {
            throw new InvalidOperationException("UpdatePantryItem response is null.");
        }

        return result;
    }

    public async Task DeleteItemAsync(
        string accessToken,
        Guid id,
        CancellationToken cancellationToken)
    {
        SetAuthHeader(accessToken);

        using var response = await client.DeleteAsync(
            $"{VersionedBasePath()}/items/{id}",
            cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK &&
            response.StatusCode != HttpStatusCode.NotFound)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"DeletePantryItem failed with status {(int)response.StatusCode}: {body}");
        }
    }
}

