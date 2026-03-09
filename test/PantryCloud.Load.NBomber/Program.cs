using Microsoft.Extensions.Configuration;
using NBomber.CSharp;
using NBomber.Contracts;
using PantryCloud.Load.NBomber.Clients;
using PantryCloud.Load.NBomber.Configuration;
using PantryCloud.Load.NBomber.Scenarios;

var configuration = LoadConfigurationBuilder.Build();
var settings = configuration.Get<LoadTestSettings>() ?? new LoadTestSettings();

LoadProfiles.Apply(settings.Load);
LoadTestSettingsValidator.Validate(settings);

using var httpClient = HttpClientFactory.Create(settings.Gateway);

var scenarioDefinition = ScenarioCatalog.Get(settings.Load.ScenarioName);
var scenario = scenarioDefinition.Factory(settings, httpClient);

var reportFileName = settings.Load.ScenarioName == ScenarioNames.AuthenticatedUserFlow
    ? "authenticated_user_flow"
    : "user_registration";

NBomberRunner
    .RegisterScenarios(scenario)
    .WithTestSuite("PantryCloud")
    .WithTestName(settings.Load.ScenarioName)
    .WithReportFileName(reportFileName)
    .WithReportFolder(settings.Reports.OutputDirectory)
    .Run();

