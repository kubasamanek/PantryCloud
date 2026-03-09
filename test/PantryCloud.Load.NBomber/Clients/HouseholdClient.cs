using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using PantryCloud.Load.NBomber.Configuration;
using PantryCloud.Load.NBomber.Models;
using PantryCloud.Load.NBomber.Utils;

namespace PantryCloud.Load.NBomber.Clients;

public sealed class HouseholdClient(HttpClient client, GatewaySettings gateway)
{
    public async Task CreateHouseholdAsync(
        string accessToken,
        string name,
        CancellationToken cancellationToken,
        bool allowConflict = true)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, gateway.HouseholdPath)
        {
            Content = JsonContent.Create(new CreateHouseholdRequest
            {
                Name = name
            })
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Add("X-Correlation-Id", CorrelationIdProvider.Create());

        using var response = await client.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Conflict && allowConflict)
        {
            return;
        }

        if (response.StatusCode != HttpStatusCode.Created)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"CreateHousehold failed with status {(int)response.StatusCode}: {body}");
        }
    }

    public async Task<GetCurrentHouseholdResponse?> GetCurrentHouseholdAsync(
        string accessToken,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{gateway.HouseholdPath}/me");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Add("X-Correlation-Id", CorrelationIdProvider.Create());

        using var response = await client.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"GetCurrentHousehold failed with status {(int)response.StatusCode}: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<GetCurrentHouseholdResponse>(cancellationToken: cancellationToken);
        if (result is null || result.Id == Guid.Empty)
        {
            throw new InvalidOperationException("GetCurrentHousehold response is missing household id.");
        }

        return result;
    }
}

