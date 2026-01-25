using PantryCloud.SharedKernel.Configuration;

namespace PantryCloud.ShoppingList.Core;

public class ApiConfiguration : ApiConfigurationBase
{
    public JwtSettings Jwt { get; set; } = new();
    public AppSettings App { get; set; } = new();
}

public class JwtSettings
{
    public string Issuer { get; set; } = "pantry-identity";
    public string Audience { get; set; } = "pantry-cloud";
}

public class AppSettings
{
    public string IdentityUrl { get; set; } = "http://localhost:5072";
}

