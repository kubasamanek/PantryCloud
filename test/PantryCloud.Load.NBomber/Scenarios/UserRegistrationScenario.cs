using NBomber.Contracts;
using NBomber.CSharp;
using PantryCloud.Load.NBomber.Clients;
using PantryCloud.Load.NBomber.Configuration;

namespace PantryCloud.Load.NBomber.Scenarios;

public static class UserRegistrationScenario
{
    public static ScenarioProps Create(LoadTestSettings settings, HttpClient httpClient)
    {
        var identityClient = new IdentityClient(httpClient, settings.Gateway);

        var scenario = Scenario.Create(settings.Load.ScenarioName, async context =>
        {
            try
            {
                var stepResult = await Step.Run("register_user", context, async () =>
                {
                    var email = BuildUniqueEmail(settings.Registration.EmailDomain, context.InvocationNumber);

                    await identityClient.RegisterAsync(
                        email,
                        settings.Registration.BasePassword,
                        CancellationToken.None);

                    return Response.Ok();
                });

                return stepResult.IsError ? Response.Fail() : Response.Ok();
            }
            catch (Exception)
            {
                return Response.Fail();
            }
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(settings.Load.WarmupSeconds))
        .WithLoadSimulations(
            Simulation.KeepConstant(
                settings.Load.ConcurrentCopies,
                TimeSpan.FromSeconds(settings.Load.DurationSeconds)));

        return scenario;
    }

    private static string BuildUniqueEmail(string domain, long invocationNumber)
    {
        var ticks = DateTime.UtcNow.Ticks;
        return $"user_{ticks}_{invocationNumber}@{domain}";
    }
}


