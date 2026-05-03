namespace PantryCloud.Load.NBomber.Configuration;

public sealed class LoadTestSettings
{
    public GatewaySettings Gateway { get; set; } = new();

    public LoadSettings Load { get; set; } = new();

    public RegistrationSettings Registration { get; set; } = new();

    public ReportsSettings Reports { get; set; } = new();
}

public sealed class GatewaySettings
{
    public string BaseUrl { get; set; } = "http://pantry.test";

    public string RegistrationPath { get; set; } = "/api/v1/identity/auth/register";

    public string LoginPath { get; set; } = "/api/v1/identity/auth/login";

    public string VerifyEmailPath { get; set; } = "/api/v1/identity/auth/verify-email";

    public string HouseholdPath { get; set; } = "/api/v1/household/households";

    public string PantryPath { get; set; } = "/api/v1/pantry";
}

public sealed class LoadSettings
{
    public string ScenarioName { get; set; } = ScenarioNames.UserRegistration;

    public string? Profile { get; set; }

    public int DurationSeconds { get; set; } = 300;

    public int WarmupSeconds { get; set; } = 30;

    public int ConcurrentCopies { get; set; } = 10;

    public int AuthenticatedLoopIterations { get; set; } = 5;

    public int DelayBetweenAuthenticatedStepsMs { get; set; } = 200;
}

public sealed class RegistrationSettings
{
    public string EmailDomain { get; set; } = "test.pantrycloud.local";

    public string BasePassword { get; set; } = "TestUser123!";
}

public sealed class ReportsSettings
{
    public string OutputDirectory { get; set; } = "./nbomber-reports";
}

public static class ScenarioNames
{
    public const string UserRegistration = "UserRegistration";

    public const string AuthenticatedUserFlow = "AuthenticatedUserFlow";

    public const string PantryWriteLoad = "PantryWriteLoad";

    public static bool IsSupported(string? scenarioName) =>
        scenarioName is UserRegistration or AuthenticatedUserFlow or PantryWriteLoad;
}

public static class LoadProfiles
{
    public const string Smoke = "Smoke";

    public const string HpaLoad = "HpaLoad";

    public static void Apply(LoadSettings load)
    {
        if (string.IsNullOrWhiteSpace(load.Profile))
        {
            return;
        }

        switch (load.Profile)
        {
            case Smoke:
                ApplySmokeProfile(load);
                break;
            case HpaLoad:
                ApplyHpaLoadProfile(load);
                break;
            default:
                throw new InvalidOperationException($"Unknown load profile '{load.Profile}'.");
        }
    }

    private static void ApplySmokeProfile(LoadSettings load)
    {
        load.DurationSeconds = 30;
        load.WarmupSeconds = 0;
        load.ConcurrentCopies = 1;
        load.AuthenticatedLoopIterations = 1;
        load.DelayBetweenAuthenticatedStepsMs = 0;
    }

    private static void ApplyHpaLoadProfile(LoadSettings load)
    {
        load.DurationSeconds = 600;
        load.WarmupSeconds = 60;
        load.ConcurrentCopies = 20;
        load.AuthenticatedLoopIterations = 5;
        load.DelayBetweenAuthenticatedStepsMs = 200;
    }
}

