using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using PantryCloud.Load.NBomber.Configuration;
using PantryCloud.Load.NBomber.Models;
using PantryCloud.Load.NBomber.Utils;

namespace PantryCloud.Load.NBomber.Clients;

public sealed class PantryClient(HttpClient client, GatewaySettings gateway)
{
    public async Task ListItemsAsync(
        string accessToken,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{gateway.PantryPath}/items");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Add("X-Correlation-Id", CorrelationIdProvider.Create());

        using var response = await client.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return;
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"ListPantryItems failed with status {(int)response.StatusCode}: {body}");
        }
    }

    public async Task CreateItemAsync(
        string accessToken,
        string name,
        decimal quantity,
        int unit,
        CancellationToken cancellationToken)
    {
        var requestBody = new CreatePantryItemRequest
        {
            Name = name,
            Quantity = quantity,
            Unit = unit
        };

        const int maxAttempts = 12;
        const int delayMilliseconds = 1500;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{gateway.PantryPath}/items")
            {
                Content = JsonContent.Create(requestBody)
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Add("X-Correlation-Id", CorrelationIdProvider.Create());

            using var response = await client.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Created)
            {
                return;
            }

            if (response.StatusCode == HttpStatusCode.NotFound && attempt < maxAttempts)
            {
                await Task.Delay(delayMilliseconds, cancellationToken);
                continue;
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"CreatePantryItem failed with status {(int)response.StatusCode}: {body}");
        }
    }
}

