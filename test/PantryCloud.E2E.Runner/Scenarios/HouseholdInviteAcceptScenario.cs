using PantryCloud.E2E.Runner.Core;

namespace PantryCloud.E2E.Runner.Scenarios;

public sealed class HouseholdInviteAcceptScenario : IE2EScenario
{
    public string Name => "household-invite";

    public async Task<E2EScenarioResult> RunAsync(E2EContext context, CancellationToken cancellationToken)
    {
        try
        {
            var ownerEmail = TestIdBuilder.HouseholdOwnerEmail(context.Random);
            var memberEmail = TestIdBuilder.HouseholdMemberEmail(context.Random);
            var password = context.Settings.Auth.DefaultPassword;

            var (_, ownerVerifyToken) = await context.Auth.RegisterAsync(ownerEmail, password, cancellationToken);
            var (_, memberVerifyToken) = await context.Auth.RegisterAsync(memberEmail, password, cancellationToken);

            await context.Auth.VerifyEmailAsync(ownerEmail, ownerVerifyToken, cancellationToken);
            await context.Auth.VerifyEmailAsync(memberEmail, memberVerifyToken, cancellationToken);

            var ownerLogin = await context.Auth.LoginAsync(ownerEmail, password, cancellationToken);
            var memberLogin = await context.Auth.LoginAsync(memberEmail, password, cancellationToken);

            var householdId = await context.Household.CreateHouseholdAsync(
                ownerLogin.AccessToken,
                "E2E Test Household",
                cancellationToken);

            var inviteCode = await context.Household.InviteMemberAsync(
                ownerLogin.AccessToken,
                householdId,
                memberEmail,
                cancellationToken);

            await context.Household.AcceptInviteAsync(
                memberLogin.AccessToken,
                inviteCode,
                cancellationToken);

            var memberCurrent = await context.Household.GetCurrentHouseholdAsync(
                memberLogin.AccessToken,
                cancellationToken);

            if (memberCurrent is null)
            {
                return new E2EScenarioResult(false, "Member current household is null after accepting invite.");
            }

            if (memberCurrent.Id != householdId)
            {
                return new E2EScenarioResult(false, "Member current household id does not match invited household id.");
            }

            return new E2EScenarioResult(true);
        }
        catch (Exception ex)
        {
            return new E2EScenarioResult(false, ex.Message);
        }
    }
}

