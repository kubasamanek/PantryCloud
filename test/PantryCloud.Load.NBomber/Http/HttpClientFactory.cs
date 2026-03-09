using System.Net.Http.Headers;
using PantryCloud.Load.NBomber.Configuration;

namespace PantryCloud.Load.NBomber.Clients;

public static class HttpClientFactory
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

