using PantryCloud.E2E.Runner.Core;

namespace PantryCloud.E2E.Runner.Scenarios;

public sealed class UserRegistrationScenario : IE2EScenario
{
    public string Name => "registration";

    public async Task<E2EScenarioResult> RunAsync(E2EContext context, CancellationToken cancellationToken)
    {
        try
        {
            var email = TestIdBuilder.UserEmail(context.Random);

            var (userId, verifyEmailToken) =
                await context.Auth.RegisterAsync(email, context.Settings.Auth.DefaultPassword, cancellationToken);

            await context.Auth.VerifyEmailAsync(email, verifyEmailToken, cancellationToken);

            var loginResult =
                await context.Auth.LoginAsync(email, context.Settings.Auth.DefaultPassword, cancellationToken);

            if (string.IsNullOrWhiteSpace(loginResult.AccessToken))
            {
                return new E2EScenarioResult(false, "Access token is empty after login.");
            }

            return new E2EScenarioResult(true);
        }
        catch (Exception ex)
        {
            return new E2EScenarioResult(false, ex.Message);
        }
    }
}

