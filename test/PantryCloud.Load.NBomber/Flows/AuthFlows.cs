using PantryCloud.Load.NBomber.Clients;
using PantryCloud.Load.NBomber.Configuration;

namespace PantryCloud.Load.NBomber.Flows;

public sealed record AuthContext(string Email, string AccessToken);

public static class AuthFlows
{
    public static async Task<AuthContext> RegisterVerifyAndLoginAsync(
        LoadTestSettings settings,
        IdentityClient identityClient,
        CancellationToken cancellationToken)
    {
        var email = BuildUniqueEmail(settings.Registration.EmailDomain);
        var password = settings.Registration.BasePassword;

        var registerResponse = await identityClient.RegisterAsync(email, password, cancellationToken);

        await identityClient.VerifyEmailAsync(email, registerResponse.VerifyEmailToken, cancellationToken);

        var loginResponse = await identityClient.LoginAsync(email, password, cancellationToken);

        return new AuthContext(email, loginResponse.AccessToken);
    }

    private static string BuildUniqueEmail(string domain)
    {
        var ticks = DateTime.UtcNow.Ticks;
        return $"user_{ticks}_{Guid.NewGuid():N}@{domain}";
    }
}

