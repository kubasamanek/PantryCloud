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

