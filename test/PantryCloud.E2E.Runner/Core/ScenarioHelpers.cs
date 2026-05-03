using PantryCloud.E2E.Runner.HttpClients;

namespace PantryCloud.E2E.Runner.Core;

public static class ScenarioHelpers
{
    public static async Task<(string AccessToken, Guid HouseholdId)> BootstrapUserWithHouseholdAsync(
        E2EContext context,
        CancellationToken cancellationToken)
    {
        var email = TestIdBuilder.UserEmail(context.Random);
        var password = context.Settings.Auth.DefaultPassword;

        var (_, verifyToken) =
            await context.Auth.RegisterAsync(email, password, cancellationToken);

        await context.Auth.VerifyEmailAsync(email, verifyToken, cancellationToken);

        var login = await context.Auth.LoginAsync(email, password, cancellationToken);

        var householdName = $"E2E Household {DateTime.UtcNow:O}";

        var householdId = await context.Household.CreateHouseholdAsync(
            login.AccessToken,
            householdName,
            cancellationToken);

        return (login.AccessToken, householdId);
    }

    public static async Task<(string OwnerAccessToken, string MemberAccessToken, Guid HouseholdId, Guid MemberUserId)> BootstrapOwnerAndMemberInHouseholdAsync(
        E2EContext context,
        CancellationToken cancellationToken)
    {
        var ownerEmail = TestIdBuilder.HouseholdOwnerEmail(context.Random);
        var memberEmail = TestIdBuilder.HouseholdMemberEmail(context.Random);
        var password = context.Settings.Auth.DefaultPassword;

        var (ownerUserIdRaw, ownerVerifyToken) = await context.Auth.RegisterAsync(ownerEmail, password, cancellationToken);
        var (memberUserIdRaw, memberVerifyToken) = await context.Auth.RegisterAsync(memberEmail, password, cancellationToken);

        await context.Auth.VerifyEmailAsync(ownerEmail, ownerVerifyToken, cancellationToken);
        await context.Auth.VerifyEmailAsync(memberEmail, memberVerifyToken, cancellationToken);

        var ownerLogin = await context.Auth.LoginAsync(ownerEmail, password, cancellationToken);
        var memberLogin = await context.Auth.LoginAsync(memberEmail, password, cancellationToken);

        var householdName = $"E2E Household {DateTime.UtcNow:O}";
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

        var memberUserId = Guid.Parse(memberUserIdRaw);
        return (ownerLogin.AccessToken, memberLogin.AccessToken, householdId, memberUserId);
    }
}
