using System.Net.Http.Headers;
using PantryCloud.E2E.Runner.Configuration;

namespace PantryCloud.E2E.Runner.Http;

public static class E2EHttpClientFactory
{
    public static HttpClient Create(GatewaySettings settings)
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri(settings.BaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };

        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        return client;
    }
}

