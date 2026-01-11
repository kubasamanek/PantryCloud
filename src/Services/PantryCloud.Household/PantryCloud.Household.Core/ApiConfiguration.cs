using PantryCloud.SharedKernel.Configuration;

namespace PantryCloud.Household.Core;

public class ApiConfiguration : ApiConfigurationBase
{
    public JwtSettings Jwt { get; set; } = new();
    public AppSettings App { get; set; } = new();
}

public class JwtSettings
{
    public string Issuer { get; set; } = "http://localhost:5072";
    public string Audience { get; set; } = "PantryCloud.WebClient";
}

public class AppSettings
{
    public string IdentityUrl { get; set; } = "http://localhost:5072";
}
