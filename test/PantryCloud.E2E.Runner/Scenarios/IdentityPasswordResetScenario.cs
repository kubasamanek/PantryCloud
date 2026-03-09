using System.Net;
using System.Net.Http.Json;
using PantryCloud.E2E.Runner.Core;
using PantryCloud.Identity.Application.DTOs;

namespace PantryCloud.E2E.Runner.Scenarios;

public sealed class IdentityPasswordResetScenario : IE2EScenario
{
    public string Name => "identity-password-reset";

    public async Task<E2EScenarioResult> RunAsync(E2EContext context, CancellationToken cancellationToken)
    {
        try
        {
            var email = TestIdBuilder.IdentityEmail(context.Random);
            var originalPassword = context.Settings.Auth.DefaultPassword;
            var newPassword = $"{originalPassword}New";

            var (_, verifyEmailToken) =
                await context.Auth.RegisterAsync(email, originalPassword, cancellationToken);

            await context.Auth.VerifyEmailAsync(email, verifyEmailToken, cancellationToken);

            var initialLogin = await context.Auth.LoginAsync(email, originalPassword, cancellationToken);
            if (string.IsNullOrWhiteSpace(initialLogin.AccessToken))
            {
                return new E2EScenarioResult(false, "Initial login did not return an access token.");
            }

            var forgotPasswordResult = await context.Auth.ForgotPasswordAsync(email, cancellationToken);
            if (string.IsNullOrWhiteSpace(forgotPasswordResult.Token) ||
                string.IsNullOrWhiteSpace(forgotPasswordResult.Url))
            {
                return new E2EScenarioResult(false, "Forgot-password did not return token and url.");
            }

            await context.Auth.ResetPasswordAsync(email, forgotPasswordResult.Token, newPassword, cancellationToken);

            var oldPasswordStatus = await TryLoginAsync(
                context,
                email,
                originalPassword,
                cancellationToken);

            if (oldPasswordStatus == HttpStatusCode.OK)
            {
                return new E2EScenarioResult(false, "Login with old password should fail after reset.");
            }

            var newPasswordLogin = await context.Auth.LoginAsync(email, newPassword, cancellationToken);
            if (string.IsNullOrWhiteSpace(newPasswordLogin.AccessToken))
            {
                return new E2EScenarioResult(false, "Login with new password did not return an access token.");
            }

            return new E2EScenarioResult(true);
        }
        catch (Exception ex)
        {
            return new E2EScenarioResult(false, ex.Message);
        }
    }

    private static async Task<HttpStatusCode> TryLoginAsync(
        E2EContext context,
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        var request = new LoginRequestDto(email, password);

        var path = $"/api/v{context.Settings.Gateway.ApiVersion}/identity/auth/login";

        using var response = await context.Client.PostAsJsonAsync(
            path,
            request,
            cancellationToken);

        return response.StatusCode;
    }
}

