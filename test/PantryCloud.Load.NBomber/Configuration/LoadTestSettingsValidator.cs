namespace PantryCloud.Load.NBomber.Configuration;

public static class LoadTestSettingsValidator
{
    public static void Validate(LoadTestSettings settings)
    {
        if (settings.Gateway is null)
        {
            throw new InvalidOperationException("Gateway configuration is missing.");
        }

        if (string.IsNullOrWhiteSpace(settings.Gateway.BaseUrl))
        {
            throw new InvalidOperationException("Gateway:BaseUrl must be configured.");
        }

        if (string.IsNullOrWhiteSpace(settings.Gateway.RegistrationPath))
        {
            throw new InvalidOperationException("Gateway:RegistrationPath must be configured.");
        }

        if (string.IsNullOrWhiteSpace(settings.Gateway.LoginPath))
        {
            throw new InvalidOperationException("Gateway:LoginPath must be configured.");
        }

        if (string.IsNullOrWhiteSpace(settings.Gateway.VerifyEmailPath))
        {
            throw new InvalidOperationException("Gateway:VerifyEmailPath must be configured.");
        }

        if (string.IsNullOrWhiteSpace(settings.Gateway.HouseholdPath))
        {
            throw new InvalidOperationException("Gateway:HouseholdPath must be configured.");
        }

        if (string.IsNullOrWhiteSpace(settings.Gateway.PantryPath))
        {
            throw new InvalidOperationException("Gateway:PantryPath must be configured.");
        }

        if (settings.Load is null)
        {
            throw new InvalidOperationException("Load configuration is missing.");
        }

        if (string.IsNullOrWhiteSpace(settings.Load.ScenarioName))
        {
            throw new InvalidOperationException("Load:ScenarioName must be configured.");
        }

        if (!ScenarioNames.IsSupported(settings.Load.ScenarioName))
        {
            throw new InvalidOperationException($"Unsupported scenario name '{settings.Load.ScenarioName}'.");
        }

        if (settings.Registration is null)
        {
            throw new InvalidOperationException("Registration configuration is missing.");
        }

        if (string.IsNullOrWhiteSpace(settings.Registration.EmailDomain))
        {
            throw new InvalidOperationException("Registration:EmailDomain must be configured.");
        }

        if (string.IsNullOrWhiteSpace(settings.Registration.BasePassword))
        {
            throw new InvalidOperationException("Registration:BasePassword must be configured.");
        }

        if (settings.Reports is null)
        {
            throw new InvalidOperationException("Reports configuration is missing.");
        }

        if (string.IsNullOrWhiteSpace(settings.Reports.OutputDirectory))
        {
            throw new InvalidOperationException("Reports:OutputDirectory must be configured.");
        }
    }
}

