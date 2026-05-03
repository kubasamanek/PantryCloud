using Microsoft.Extensions.Configuration;

namespace PantryCloud.Load.NBomber.Configuration;

public static class LoadConfigurationBuilder
{
    public static IConfigurationRoot Build()
    {
        var baseBuilder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables(prefix: "PANTRYCLOUD_");

        var baseConfig = baseBuilder.Build();

        var overrides = new Dictionary<string, string?>();

        overrides["Gateway:BaseUrl"] = Environment.GetEnvironmentVariable("PANTRYCLOUD_GATEWAY_URL");
        overrides["Gateway:RegistrationPath"] = Environment.GetEnvironmentVariable("PANTRYCLOUD_REGISTRATION_PATH");
        overrides["Gateway:LoginPath"] = Environment.GetEnvironmentVariable("PANTRYCLOUD_LOGIN_PATH");
        overrides["Gateway:VerifyEmailPath"] = Environment.GetEnvironmentVariable("PANTRYCLOUD_VERIFY_EMAIL_PATH");
        overrides["Gateway:HouseholdPath"] = Environment.GetEnvironmentVariable("PANTRYCLOUD_HOUSEHOLD_PATH");
        overrides["Gateway:PantryPath"] = Environment.GetEnvironmentVariable("PANTRYCLOUD_PANTRY_PATH");
        overrides["Load:ScenarioName"] = Environment.GetEnvironmentVariable("PANTRYCLOUD_LOAD_SCENARIO_NAME");
        overrides["Load:Profile"] = Environment.GetEnvironmentVariable("PANTRYCLOUD_LOAD_PROFILE");
        overrides["Load:DurationSeconds"] = Environment.GetEnvironmentVariable("PANTRYCLOUD_LOAD_DURATION");
        overrides["Load:ConcurrentCopies"] = Environment.GetEnvironmentVariable("PANTRYCLOUD_LOAD_CONCURRENCY");
        overrides["Load:AuthenticatedLoopIterations"] = Environment.GetEnvironmentVariable("PANTRYCLOUD_LOAD_AUTHENTICATED_LOOP_ITERATIONS");
        overrides["Load:DelayBetweenAuthenticatedStepsMs"] = Environment.GetEnvironmentVariable("PANTRYCLOUD_LOAD_DELAY_BETWEEN_STEPS_MS");

        var nonEmptyOverrides = overrides
            .Where(pair => !string.IsNullOrWhiteSpace(pair.Value));

        if (!nonEmptyOverrides.Any())
        {
            return baseConfig;
        }

        return new ConfigurationBuilder()
            .AddConfiguration(baseConfig)
            .AddInMemoryCollection(nonEmptyOverrides)
            .Build();
    }
}

