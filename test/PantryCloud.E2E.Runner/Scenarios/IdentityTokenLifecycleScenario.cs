using System.Net;
using System.Net.Http.Json;
using PantryCloud.E2E.Runner.Core;
using PantryCloud.Identity.Application.DTOs;

namespace PantryCloud.E2E.Runner.Scenarios;

public sealed class IdentityTokenLifecycleScenario : IE2EScenario
{
    public string Name => "identity-token-lifecycle";

    public async Task<E2EScenarioResult> RunAsync(E2EContext context, CancellationToken cancellationToken)
    {
        try
        {
            var email = TestIdBuilder.IdentityEmail(context.Random);
            var password = context.Settings.Auth.DefaultPassword;

            var (_, verifyEmailToken) =
                await context.Auth.RegisterAsync(email, password, cancellationToken);

            await context.Auth.VerifyEmailAsync(email, verifyEmailToken, cancellationToken);

            var initialLogin = await context.Auth.LoginAsync(email, password, cancellationToken);

            var refreshed = await context.Auth.RefreshAsync(initialLogin.RefreshToken, cancellationToken);

            if (string.Equals(initialLogin.AccessToken, refreshed.AccessToken, StringComparison.Ordinal) &&
                string.Equals(initialLogin.RefreshToken, refreshed.RefreshToken, StringComparison.Ordinal))
            {
                return new E2EScenarioResult(false, "Refreshed tokens must differ from initial tokens.");
            }

            await context.Auth.LogoutAsync(refreshed.RefreshToken, cancellationToken);

            var negativeResult = await TryRefreshAsync(
                context,
                refreshed.RefreshToken,
                cancellationToken);

            if (negativeResult == HttpStatusCode.OK)
            {
                return new E2EScenarioResult(false, "Refresh after logout should not succeed.");
            }

            return new E2EScenarioResult(true);
        }
        catch (Exception ex)
        {
            return new E2EScenarioResult(false, ex.Message);
        }
    }

    private static async Task<HttpStatusCode> TryRefreshAsync(
        E2EContext context,
        string refreshToken,
        CancellationToken cancellationToken)
    {
        var request = new RefreshTokenRequestDto(refreshToken);
        var path = $"/api/v{context.Settings.Gateway.ApiVersion}/identity/auth/refresh";
        using var response = await context.Client.PostAsJsonAsync(path, request, cancellationToken);
        return response.StatusCode;
    }
}

