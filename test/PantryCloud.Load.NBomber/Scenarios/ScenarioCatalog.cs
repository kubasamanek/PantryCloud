using NBomber.Contracts;
using PantryCloud.Load.NBomber.Configuration;

namespace PantryCloud.Load.NBomber.Scenarios;

public sealed record ScenarioDefinition(
    string Name,
    string Description,
    string DefaultProfile,
    Func<LoadTestSettings, HttpClient, ScenarioProps> Factory);

public static class ScenarioCatalog
{
    private static readonly IReadOnlyDictionary<string, ScenarioDefinition> Definitions =
        new[]
        {
            new ScenarioDefinition(
                ScenarioNames.UserRegistration,
                "Registers new users via the Identity service.",
                LoadProfiles.Smoke,
                UserRegistrationScenario.Create),
            new ScenarioDefinition(
                ScenarioNames.AuthenticatedUserFlow,
                "Full authenticated user flow: register, verify, login, household, pantry.",
                LoadProfiles.HpaLoad,
                AuthenticatedUserFlowScenario.Create),
            new ScenarioDefinition(
                ScenarioNames.PantryWriteLoad,
                "High-volume pantry item writes for HPA validation.",
                LoadProfiles.HpaLoad,
                PantryWriteLoadScenario.Create)
        }.ToDictionary(d => d.Name, d => d, StringComparer.OrdinalIgnoreCase);

    public static ScenarioDefinition Get(string scenarioName)
    {
        if (!Definitions.TryGetValue(scenarioName, out var definition))
        {
            throw new InvalidOperationException(
                $"Unknown scenario '{scenarioName}'. Supported scenarios: {string.Join(", ", Definitions.Keys)}");
        }

        return definition;
    }
}


