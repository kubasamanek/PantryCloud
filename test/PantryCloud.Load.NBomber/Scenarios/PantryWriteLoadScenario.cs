using NBomber.Contracts;
using NBomber.CSharp;
using PantryCloud.Load.NBomber.Clients;
using PantryCloud.Load.NBomber.Configuration;
using PantryCloud.Load.NBomber.Flows;

namespace PantryCloud.Load.NBomber.Scenarios;

public static class PantryWriteLoadScenario
{
    public static ScenarioProps Create(LoadTestSettings settings, HttpClient httpClient)
    {
        var identityClient = new IdentityClient(httpClient, settings.Gateway);
        var householdClient = new HouseholdClient(httpClient, settings.Gateway);
        var pantryClient = new PantryClient(httpClient, settings.Gateway);

        var scenario = Scenario.Create(ScenarioNames.PantryWriteLoad, async context =>
        {
            AuthContext? authContext = null;

            var authStep = await Step.Run("register_verify_login", context, async () =>
            {
                authContext = await AuthFlows.RegisterVerifyAndLoginAsync(
                    settings,
                    identityClient,
                    CancellationToken.None);

                return Response.Ok();
            });

            if (authStep.IsError || authContext is null)
            {
                return Response.Fail();
            }

            var bootstrapStep = await Step.Run("ensure_household", context, async () =>
            {
                try
                {
                    await householdClient.CreateHouseholdAsync(
                        authContext.AccessToken,
                        $"HPA HH {context.InvocationNumber}",
                        CancellationToken.None);

                    await householdClient.GetCurrentHouseholdAsync(
                        authContext.AccessToken,
                        CancellationToken.None);

                    return Response.Ok();
                }
                catch (Exception)
                {
                    return Response.Fail();
                }
            });

            if (bootstrapStep.IsError)
            {
                return Response.Fail();
            }

            for (var i = 0; i < settings.Load.AuthenticatedLoopIterations; i++)
            {
                if (settings.Load.DelayBetweenAuthenticatedStepsMs > 0)
                {
                    await Task.Delay(settings.Load.DelayBetweenAuthenticatedStepsMs);
                }

                var writeStep = await Step.Run("create_pantry_item", context, async () =>
                {
                    try
                    {
                        var itemName = $"HPA Item {context.InvocationNumber} i{i}";

                        await pantryClient.CreateItemAsync(
                            authContext.AccessToken,
                            itemName,
                            1,
                            unit: 0,
                            CancellationToken.None);

                        return Response.Ok();
                    }
                    catch (Exception)
                    {
                        return Response.Fail();
                    }
                });

                if (writeStep.IsError)
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
}

