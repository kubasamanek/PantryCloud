namespace PantryCloud.E2E.Runner.Configuration;

public sealed class E2ESettings
{
    public GatewaySettings Gateway { get; init; } = new();

    public AuthSettings Auth { get; init; } = new();

    public EnvironmentSettings Environment { get; init; } = new();
}

public sealed class GatewaySettings
{
    public string BaseUrl { get; init; } = "http://pantry.test";

    public string ApiVersion { get; init; } = "1";
}

public sealed class AuthSettings
{
    public string DefaultPassword { get; init; } = "TestUser123!";
}

public sealed class EnvironmentSettings
{
    public string Name { get; init; } = "kind";
}

