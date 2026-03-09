using PantryCloud.E2E.Runner.Core;

namespace PantryCloud.E2E.Runner.Scenarios;

public sealed class HouseholdTransferAndLeaveScenario : IE2EScenario
{
    public string Name => "household-transfer-and-leave";

    public async Task<E2EScenarioResult> RunAsync(E2EContext context, CancellationToken cancellationToken)
    {
        try
        {
            var password = context.Settings.Auth.DefaultPassword;

            var ownerEmail = TestIdBuilder.HouseholdOwnerEmail(context.Random);
            var memberEmail = TestIdBuilder.HouseholdMemberEmail(context.Random);

            var (ownerUserIdRaw, ownerVerifyToken) =
                await context.Auth.RegisterAsync(ownerEmail, password, cancellationToken);
            var (memberUserIdRaw, memberVerifyToken) =
                await context.Auth.RegisterAsync(memberEmail, password, cancellationToken);

            await context.Auth.VerifyEmailAsync(ownerEmail, ownerVerifyToken, cancellationToken);
            await context.Auth.VerifyEmailAsync(memberEmail, memberVerifyToken, cancellationToken);

            var ownerLogin = await context.Auth.LoginAsync(ownerEmail, password, cancellationToken);
            var memberLogin = await context.Auth.LoginAsync(memberEmail, password, cancellationToken);

            var ownerUserId = Guid.Parse(ownerUserIdRaw);
            var memberUserId = Guid.Parse(memberUserIdRaw);

            var householdName = $"E2E Transfer Household {DateTime.UtcNow:O}";

            var householdId = await context.Household.CreateHouseholdAsync(
                ownerLogin.AccessToken,
                householdName,
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

            var transferResult = await context.Household.TransferOwnershipAsync(
                ownerLogin.AccessToken,
                memberUserId,
                cancellationToken);

            if (transferResult.HouseholdId != householdId)
            {
                return new E2EScenarioResult(false, "TransferOwnership household id does not match expected household id.");
            }

            if (transferResult.PreviousOwnerId != ownerUserId ||
                transferResult.NewOwnerId != memberUserId)
            {
                return new E2EScenarioResult(false, "TransferOwnership owner ids do not match expected values.");
            }

            var leaveResult = await context.Household.LeaveHouseholdAsync(
                ownerLogin.AccessToken,
                cancellationToken);

            if (leaveResult.HouseholdId != householdId ||
                leaveResult.MemberId != ownerUserId)
            {
                return new E2EScenarioResult(false, "LeaveHousehold response does not match expected household or member id.");
            }

            var newOwnerCurrent = await context.Household.GetCurrentHouseholdAsync(
                memberLogin.AccessToken,
                cancellationToken);

            if (newOwnerCurrent is null || newOwnerCurrent.Id != householdId)
            {
                return new E2EScenarioResult(false, "New owner does not see expected current household after transfer.");
            }

            var oldOwnerCurrent = await context.Household.GetCurrentHouseholdAsync(
                ownerLogin.AccessToken,
                cancellationToken);

            if (oldOwnerCurrent is not null)
            {
                return new E2EScenarioResult(false, "Old owner should not have a current household after leaving.");
            }

            return new E2EScenarioResult(true);
        }
        catch (Exception ex)
        {
            return new E2EScenarioResult(false, ex.Message);
        }
    }
}

