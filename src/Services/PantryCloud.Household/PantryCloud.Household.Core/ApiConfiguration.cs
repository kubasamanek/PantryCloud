using PantryCloud.SharedKernel.Configuration;

namespace PantryCloud.Household.Core;

public class ApiConfiguration : ApiConfigurationBase
{
    public JwtSettings Jwt { get; set; } = new();
    public AppSettings App { get; set; } = new();
    public EmailSettings Email { get; set; } = new();
}

public class JwtSettings
{
    public string Issuer { get; set; } = "http://localhost:5072";
    public string Audience { get; set; } = "PantryCloud.WebClient";
}

public class AppSettings
{
    public string IdentityUrl { get; set; } = "http://localhost:5072";
    public string FrontendUrl { get; set; } = "http://localhost:5019";
    public bool SendEmails { get; set; } = false;
    public int InvitationExpirationMinutes { get; set; } = 2;
}

public class EmailSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
}
