using System.Net;
using System.Net.Http.Json;
using PantryCloud.Load.NBomber.Configuration;
using PantryCloud.Load.NBomber.Models;
using PantryCloud.Load.NBomber.Utils;

namespace PantryCloud.Load.NBomber.Clients;

public sealed class IdentityClient(HttpClient client, GatewaySettings gateway)
{
    public async Task<RegisterResponse> RegisterAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        var requestBody = new RegistrationRequest
        {
            Email = email,
            Password = password,
            ConfirmPassword = password
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, gateway.RegistrationPath);
        request.Content = JsonContent.Create(requestBody);

        request.Headers.Add("X-Correlation-Id", CorrelationIdProvider.Create());

        using var response = await client.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Register failed with status {(int)response.StatusCode}: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<RegisterResponse>(cancellationToken: cancellationToken);
        if (result is null ||
            string.IsNullOrWhiteSpace(result.UserId) ||
            string.IsNullOrWhiteSpace(result.VerifyEmailToken))
        {
            throw new InvalidOperationException("Register response is missing required data.");
        }

        return result;
    }

    public async Task VerifyEmailAsync(
        string email,
        string verifyToken,
        CancellationToken cancellationToken)
    {
        var requestBody = new VerifyEmailRequest
        {
            Email = email,
            Token = verifyToken
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, gateway.VerifyEmailPath)
        {
            Content = JsonContent.Create(requestBody)
        };

        request.Headers.Add("X-Correlation-Id", CorrelationIdProvider.Create());

        using var response = await client.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Verify email failed with status {(int)response.StatusCode}: {body}");
        }
    }

    public async Task<LoginResponse> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        var requestBody = new LoginRequest
        {
            Email = email,
            Password = password
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, gateway.LoginPath)
        {
            Content = JsonContent.Create(requestBody)
        };

        request.Headers.Add("X-Correlation-Id", CorrelationIdProvider.Create());

        using var response = await client.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Login failed with status {(int)response.StatusCode}: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: cancellationToken);
        if (result is null || string.IsNullOrWhiteSpace(result.AccessToken))
        {
            throw new InvalidOperationException("Login response is missing access token.");
        }

        return result;
    }
}

