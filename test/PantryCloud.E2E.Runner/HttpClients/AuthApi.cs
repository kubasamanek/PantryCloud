using System.Net;
using System.Net.Http.Json;
using PantryCloud.E2E.Runner.Configuration;
using PantryCloud.Identity.Application.DTOs;

namespace PantryCloud.E2E.Runner.HttpClients;

public sealed class AuthApi(HttpClient client, E2ESettings settings)
{
    private string VersionedPath(string relative) =>
        $"/api/v{settings.Gateway.ApiVersion}/identity/auth/{relative}";

    public async Task<(string UserId, string VerifyEmailToken)> RegisterAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        var request = new RegisterRequestDto(email, password);

        using var response = await client.PostAsJsonAsync(
            VersionedPath("register"),
            request,
            cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Register failed with status {(int)response.StatusCode}: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<RegisterResponseDto>(cancellationToken: cancellationToken);
        if (result is null ||
            string.IsNullOrWhiteSpace(result.UserId) ||
            string.IsNullOrWhiteSpace(result.VerifyEmailToken))
        {
            throw new InvalidOperationException("Register response is missing required data.");
        }

        return (result.UserId, result.VerifyEmailToken);
    }

    public async Task<LoginResponseDto> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        var request = new LoginRequestDto(email, password);

        using var response = await client.PostAsJsonAsync(
            VersionedPath("login"),
            request,
            cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Login failed with status {(int)response.StatusCode}: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>(cancellationToken: cancellationToken);
        if (result is null || string.IsNullOrWhiteSpace(result.AccessToken))
        {
            throw new InvalidOperationException("Login response is missing access token.");
        }

        return result;
    }

    public async Task VerifyEmailAsync(
        string email,
        string verifyToken,
        CancellationToken cancellationToken)
    {
        var request = new VerifyEmailRequestDto(email, verifyToken);

        using var response = await client.PostAsJsonAsync(
            VersionedPath("verify-email"),
            request,
            cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Verify email failed with status {(int)response.StatusCode}: {body}");
        }
    }

    public async Task<RefreshTokenResponseDto> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        var request = new RefreshTokenRequestDto(refreshToken);

        using var response = await client.PostAsJsonAsync(
            VersionedPath("refresh"),
            request,
            cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Refresh failed with status {(int)response.StatusCode}: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<RefreshTokenResponseDto>(cancellationToken: cancellationToken);
        if (result is null || string.IsNullOrWhiteSpace(result.AccessToken))
        {
            throw new InvalidOperationException("Refresh response is missing access token.");
        }

        return result;
    }

    public async Task<ForgotPasswordResponseDto> ForgotPasswordAsync(
        string email,
        CancellationToken cancellationToken)
    {
        var request = new ForgotPasswordRequestDto(email);

        using var response = await client.PostAsJsonAsync(
            VersionedPath("forgot-password"),
            request,
            cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"ForgotPassword failed with status {(int)response.StatusCode}: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<ForgotPasswordResponseDto>(cancellationToken: cancellationToken);
        if (result is null ||
            string.IsNullOrWhiteSpace(result.Token) ||
            string.IsNullOrWhiteSpace(result.Url))
        {
            throw new InvalidOperationException("ForgotPassword response is missing token or url.");
        }

        return result;
    }

    public async Task ResetPasswordAsync(
        string email,
        string token,
        string newPassword,
        CancellationToken cancellationToken)
    {
        var request = new ResetPasswordRequestDto(email, token, newPassword);

        using var response = await client.PostAsJsonAsync(
            VersionedPath("reset-password"),
            request,
            cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"ResetPassword failed with status {(int)response.StatusCode}: {body}");
        }
    }

    public async Task LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        var request = new LogoutRequestDto(refreshToken);

        using var response = await client.PostAsJsonAsync(
            VersionedPath("logout"),
            request,
            cancellationToken);

        if (response.StatusCode != HttpStatusCode.NoContent)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Logout failed with status {(int)response.StatusCode}: {body}");
        }
    }
}

