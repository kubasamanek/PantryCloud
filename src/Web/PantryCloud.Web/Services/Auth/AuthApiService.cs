using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PantryCloud.Web.Services.Auth;

public class AuthApiService(IHttpClientFactory httpClientFactory) : IAuthApi
{
    private record ApiProblemDetails
    {
        [JsonPropertyName("detail")] public string? Detail { get; init; }
        [JsonPropertyName("errors")] public Dictionary<string, string[]>? Errors { get; init; }
    }

    private const string BasePath = "api/v1/identity/auth";
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private HttpClient Client => httpClientFactory.CreateClient("Auth");

    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var response = await Client.PostAsJsonAsync($"{BasePath}/login", request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var value = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken);
            return new LoginResult { Value = value };
        }

        var problem = await TryReadProblemAsync(response, cancellationToken);
        var isEmailNotVerified = problem?.Detail?.Contains("verify", StringComparison.OrdinalIgnoreCase) == true;
        var message = problem?.Detail ?? (response.StatusCode == HttpStatusCode.Unauthorized
            ? "Invalid email or password."
            : "Sign in failed. Please try again.");

        return new LoginResult { ErrorMessage = message, IsEmailNotVerified = isEmailNotVerified };
    }

    public async Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var response = await Client.PostAsJsonAsync($"{BasePath}/register", request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var value = await response.Content.ReadFromJsonAsync<RegisterResponse>(cancellationToken);
            return new RegisterResult { Value = value };
        }

        var problem = await TryReadProblemAsync(response, cancellationToken);
        var errors = new List<string>();

        if (problem?.Errors is { Count: > 0 })
        {
            errors.AddRange(problem.Errors.Values.SelectMany(v => v));
        }
        else if (!string.IsNullOrWhiteSpace(problem?.Detail))
        {
            errors.Add(problem.Detail);
        }
        else
        {
            errors.Add("Registration failed. Please try again.");
        }

        return new RegisterResult { Errors = errors };
    }

    private static async Task<ApiProblemDetails?> TryReadProblemAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<ApiProblemDetails>(JsonOptions, ct);
        }
        catch
        {
            return null;
        }
    }

    public async Task<RefreshTokenResponse?> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var response = await Client.PostAsJsonAsync($"{BasePath}/refresh", request, cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<RefreshTokenResponse>(cancellationToken)
            : null;
    }

    public async Task<ForgotPasswordResponse?> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var response = await Client.PostAsJsonAsync($"{BasePath}/forgot-password", request, cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<ForgotPasswordResponse>(cancellationToken)
            : null;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var response = await Client.PostAsJsonAsync($"{BasePath}/reset-password", request, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken = default)
    {
        var response = await Client.PostAsJsonAsync($"{BasePath}/verify-email", request, cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
