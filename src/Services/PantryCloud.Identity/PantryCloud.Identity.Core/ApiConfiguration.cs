using PantryCloud.SharedKernel.Configuration;

namespace PantryCloud.Identity.Core;

public class ApiConfiguration : ApiConfigurationBase
{
    public JwtSettings Jwt { get; set; } = new();
    public AppSettings App { get; set; } = new();
    public EmailSettings Email { get; set; } = new();
}

public class JwtSettings
{
    public string Issuer { get; set; } = "https://localhost:5072";
    public string Audience { get; set; } = "PantryCloud.WebClient";
    public int ExpirationInMinutes { get; set; } = 10;
    public string PrivateKeyPath { get; set; } = "../../secrets/private.pem";
    public string PublicKeyPath { get; set; } = "../../secrets/public.pem";
}

public class AppSettings
{
    public string FrontendUrl { get; set; } = "http://localhost:5019";
    public bool SendEmails { get; set; } = false;
    public int ResetPasswordTokenExpirationInMinutes { get; set; } = 60;
    public int VerifyEmailTokenExpirationInMinutes { get; set; } = 60;
}

public class EmailSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 80;
    public string Username { get; set; } = "admin";
    public string Password { get; set; } = "admin";
    public string From { get; set; } = "admin";
}
