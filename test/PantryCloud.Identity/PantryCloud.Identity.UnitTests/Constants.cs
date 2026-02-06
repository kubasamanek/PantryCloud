using Microsoft.Extensions.Configuration;
using PantryCloud.Identity.Core.Entities;
using PantryCloud.Identity.Infrastructure;

namespace PantryCloud.Identity.UnitTests;

internal static class Constants
{
    public static class User
    {
        public static readonly Guid Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        public const string Email = "alice@example.com";
        public const string NotFoundEmail = "notfound@example.com";
        public const string TestEmail = "user@example.com";
    }

    public static class Passwords
    {
        public const string Strong = "StrongPass#1";
        public const string Example = "password";
        public const string Wrong = "Wrong#123";
        public const string New = "NewPass123!";
    }

    public static class Tokens
    {
        public const string AccessToken = "AT";
        public const string RefreshToken = "RT";
        public const string OldRefreshToken = "OLD_RT";
        public const string ExpiredRefreshToken = "EXPIRED_RT";
        public const string NewAccessToken = "NEW_AT";
        public const string NewRefreshToken = "NEW_RT";
        public const string NonExistent = "NON_EXISTENT";
        public const string Invalid = "invalid-token";
        public const string PasswordReset = "PASSWORD_RESET_TOKEN";
    }

    public static class PasswordHasher
    {
        public const string WrongPassword = "WrongPassword";
        public const string VerifyTestPassword = "anything";
        public const string MalformedNoDash = "ABCDEF";
        public const string MalformedNonHex = "NOTHEX-ALSOnotHEX";
    }

    public static class Emails
    {
        public const string NotFound = "nobody@example.com";
        public const string Missing = "missing@example.com";
    }

    public static readonly ApplicationUser ExampleUser = new()
    {
        Id = User.Id,
        Email = User.Email,
        EmailVerified = true,
        PasswordHash = PantryCloud.Identity.Infrastructure.PasswordHasher.Hash(Passwords.Example)
    };

    public static class Jwt
    {
        public const string Secret = "CORRECT_SECRET_KEY_32+_CHARS________________";
        public const string BadSecret = "BAD_SECRET_KEY_32+_CHARS___________________";
        public const string Issuer = "PantryCloud.IdentityService";
        public const string Audience = "PantryCloud.WebClient";
        public const int ExpirationInMinutes = 5;
        public const string PublicKeyPath = "Auth/Certs/jwt-public.pem";
        public const string PrivateKeyPath = "Auth/Certs/jwt-private.pem";

        public static readonly IConfiguration Config =
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Secret"] = Secret,
                    ["Jwt:Issuer"] = Issuer,
                    ["Jwt:Audience"] = Audience,
                    ["Jwt:ExpirationInMinutes"] = ExpirationInMinutes.ToString()
                }!)
                .Build();
    }
}