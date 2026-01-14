using PantryCloud.SharedKernel.Configuration;

namespace PantryCloud.ApiGateway.Core;

public class ApiConfiguration : ApiConfigurationBase
{
    public GatewaySettings Gateway { get; set; } = new();
    public ServiceEndpoints Services { get; set; } = new();
    public JwtSettings Jwt { get; set; } = new();
    public AppSettings App { get; set; } = new();
}

public class GatewaySettings
{
    public int Port { get; set; } = 5000;
    public bool EnableSwagger { get; set; } = true;
    public RateLimitSettings RateLimit { get; set; } = new();
    public ResilienceSettings Resilience { get; set; } = new();
}

public class RateLimitSettings
{
    public bool Enabled { get; set; } = true;
    public int PermitLimit { get; set; } = 100;
    public int WindowSeconds { get; set; } = 60;
    public int QueueLimit { get; set; } = 0;
}

public class ResilienceSettings
{
    public int RequestTimeoutSeconds { get; set; } = 30;
    public CircuitBreakerSettings CircuitBreaker { get; set; } = new();
    public RetrySettings Retry { get; set; } = new();
    public HealthCheckSettings HealthCheck { get; set; } = new();
}

public class CircuitBreakerSettings
{
    public bool Enabled { get; set; } = true;
    public int FailureThreshold { get; set; } = 5;
    public int DurationOfBreakSeconds { get; set; } = 30;
    public int SamplingDurationSeconds { get; set; } = 60;
    public int MinimumThroughput { get; set; } = 10;
}

public class RetrySettings
{
    public bool Enabled { get; set; } = true;
    public int MaxRetryAttempts { get; set; } = 3;
    public int BaseDelayMilliseconds { get; set; } = 1000;
}

public class HealthCheckSettings
{
    public bool Enabled { get; set; } = true;
    public int IntervalSeconds { get; set; } = 30;
    public int TimeoutSeconds { get; set; } = 5;
    public string Path { get; set; } = "/health";
    public string Policy { get; set; } = "ConsecutiveFailures";
    public int ConsecutiveFailureThreshold { get; set; } = 3;
}

public class ServiceEndpoints
{
    public string IdentityService { get; set; } = "http://identity-api:8080";
    public string HouseholdService { get; set; } = "http://household-api:8080";
    public string PantryService { get; set; } = "http://pantry-api:8080";
    public string RecipeService { get; set; } = "http://recipe-api:8080";
    public string ShoppingListService { get; set; } = "http://shoppinglist-api:8080";
}

public class JwtSettings
{
    public string Issuer { get; set; } = "http://localhost:5072";
    public string Audience { get; set; } = "PantryCloud.WebClient";
    public string PublicKeyPath { get; set; } = "Secrets/public.pem";
}

public class AppSettings
{
    public string IdentityUrl { get; set; } = "http://localhost:5072";
}

