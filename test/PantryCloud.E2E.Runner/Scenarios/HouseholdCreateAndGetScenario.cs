using PantryCloud.E2E.Runner.Core;

namespace PantryCloud.E2E.Runner.Scenarios;

public sealed class HouseholdCreateAndGetScenario : IE2EScenario
{
    public string Name => "household-create-and-get";

    public async Task<E2EScenarioResult> RunAsync(E2EContext context, CancellationToken cancellationToken)
    {
        try
        {
            var email = TestIdBuilder.HouseholdOwnerEmail(context.Random);
            var password = context.Settings.Auth.DefaultPassword;

            var (_, verifyToken) = await context.Auth.RegisterAsync(email, password, cancellationToken);

            await context.Auth.VerifyEmailAsync(email, verifyToken, cancellationToken);

            var login = await context.Auth.LoginAsync(email, password, cancellationToken);
            if (string.IsNullOrWhiteSpace(login.AccessToken))
            {
                return new E2EScenarioResult(false, "Access token is empty after login.");
            }

            var householdName = $"E2E Household {DateTime.UtcNow:O}";

            var householdId = await context.Household.CreateHouseholdAsync(
                login.AccessToken,
                householdName,
                cancellationToken);

            var current = await context.Household.GetCurrentHouseholdAsync(
                login.AccessToken,
                cancellationToken);

            if (current is null)
            {
                return new E2EScenarioResult(false, "GetCurrentHousehold returned null.");
            }

            if (current.Id != householdId)
            {
                return new E2EScenarioResult(false, "Current household id does not match created household id.");
            }

            if (!string.Equals(current.Name, householdName, StringComparison.Ordinal))
            {
                return new E2EScenarioResult(false, "Current household name does not match created household name.");
            }

            return new E2EScenarioResult(true);
        }
        catch (Exception ex)
        {
            return new E2EScenarioResult(false, ex.Message);
        }
    }
}

