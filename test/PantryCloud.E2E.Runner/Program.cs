using Microsoft.Extensions.Configuration;
using PantryCloud.E2E.Runner.Configuration;
using PantryCloud.E2E.Runner.Core;
using PantryCloud.E2E.Runner.Scenarios;

var configuration = BuildConfiguration();
var settings = configuration.Get<E2ESettings>() ?? new E2ESettings();

var context = new E2EContext(settings);

var scenarios = new IE2EScenario[]
{
    new IdentityTokenLifecycleScenario(),
    new IdentityPasswordResetScenario(),
    new UserRegistrationScenario(),
    new HouseholdCreateAndGetScenario(),
    new HouseholdInviteAcceptScenario(),
    new HouseholdTransferAndLeaveScenario(),
    new PantryCrudScenario(),
    new PantryDepleteScenario(),
    new RecipeCreateAndSearchScenario(),
    new ShoppingListFromRecipeScenario(),
    new ShoppingListCompletionScenario(),
    new NotificationMemberJoinedScenario(),
    new NotificationPantryDepletionScenario(),
    new AuditEntryScenario()
};

var (runAll, requestedScenario) = ParseArgs(args);

var selectedScenarios = runAll
    ? scenarios
    : scenarios.Where(s => string.Equals(s.Name, requestedScenario, StringComparison.OrdinalIgnoreCase)).ToArray();

if (selectedScenarios.Length == 0)
{
    Console.WriteLine($"No scenarios matched '{requestedScenario}'. Available: {string.Join(", ", scenarios.Select(s => s.Name))}");
    return 1;
}

Console.WriteLine($"Running e2e scenarios against {settings.Gateway.BaseUrl} (env: {settings.Environment.Name})");

var anyFailed = false;

foreach (var scenario in selectedScenarios)
{
    Console.WriteLine($"--- Scenario: {scenario.Name} ---");
    var result = await scenario.RunAsync(context, CancellationToken.None);
    if (result.Success)
    {
        Console.WriteLine("Result: SUCCESS");
    }
    else
    {
        anyFailed = true;
        Console.WriteLine($"Result: FAILED - {result.Error}");
    }
}

return anyFailed ? 1 : 0;

static IConfigurationRoot BuildConfiguration()
{
    var baseBuilder = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables(prefix: "PANTRYCLOUD_E2E_");

    var baseConfig = baseBuilder.Build();

    var overrides = new Dictionary<string, string?>();

    overrides["Gateway:BaseUrl"] = Environment.GetEnvironmentVariable("PANTRYCLOUD_E2E_GATEWAY_URL");
    overrides["Environment:Name"] = Environment.GetEnvironmentVariable("PANTRYCLOUD_E2E_ENV");

    var nonEmptyOverrides = overrides
        .Where(pair => !string.IsNullOrWhiteSpace(pair.Value));

    var keyValuePairs = nonEmptyOverrides.ToList();
    if (!keyValuePairs.Any())
    {
        return baseConfig;
    }

    return new ConfigurationBuilder()
        .AddConfiguration(baseConfig)
        .AddInMemoryCollection(keyValuePairs)
        .Build();
}

static (bool RunAll, string ScenarioName) ParseArgs(string[] args)
{
    if (args.Length == 0)
    {
        return (true, string.Empty);
    }

    for (var i = 0; i < args.Length; i++)
    {
        if (string.Equals(args[i], "--all", StringComparison.OrdinalIgnoreCase))
        {
            return (true, string.Empty);
        }

        if (string.Equals(args[i], "--scenario", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
        {
            return (false, args[i + 1]);
        }
    }

    return (true, string.Empty);
}
