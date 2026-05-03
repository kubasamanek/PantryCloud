using NBomber.Contracts;
using NBomber.CSharp;
using PantryCloud.Load.NBomber.Clients;
using PantryCloud.Load.NBomber.Configuration;
using PantryCloud.Load.NBomber.Flows;

namespace PantryCloud.Load.NBomber.Scenarios;

public static class AuthenticatedUserFlowScenario
{
    public static ScenarioProps Create(LoadTestSettings settings, HttpClient httpClient)
    {
        var identityClient = new IdentityClient(httpClient, settings.Gateway);
        var householdClient = new HouseholdClient(httpClient, settings.Gateway);
        var pantryClient = new PantryClient(httpClient, settings.Gateway);

        var scenario = Scenario.Create(settings.Load.ScenarioName, async context =>
        {
            AuthContext? authContext = null;

            var registerVerifyLoginStep = await Step.Run("register_verify_login", context, async () =>
            {
                authContext = await AuthFlows.RegisterVerifyAndLoginAsync(
                    settings,
                    identityClient,
                    CancellationToken.None);

                return Response.Ok();
            });

            if (registerVerifyLoginStep.IsError || authContext is null)
            {
                return Response.Fail();
            }

            for (var i = 0; i < settings.Load.AuthenticatedLoopIterations; i++)
            {
                if (settings.Load.DelayBetweenAuthenticatedStepsMs > 0)
                    await Task.Delay(settings.Load.DelayBetweenAuthenticatedStepsMs);

                var householdAndPantryStep = await Step.Run("household_and_pantry_flow", context, async () =>
                {
                    try
                    {
                        await DomainFlows.EnsureHouseholdWithPantryAsync(
                            authContext,
                            householdClient,
                            pantryClient,
                            context.InvocationNumber,
                            i,
                            CancellationToken.None);

                        return Response.Ok();
                    }
                    catch (Exception)
                    {
                        return Response.Fail();
                    }
                });

                if (householdAndPantryStep.IsError)
                {
                    return Response.Fail();
                }
            }

            return Response.Ok();
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
