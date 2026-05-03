using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace PantryCloud.SharedKernel.Extensions;

/// <summary>
/// JWT Bearer authentication. Uses JWKS discovery from Identity when <c>Jwt:IssuerSigningKey</c> is not set.
/// When <c>Jwt:IssuerSigningKey</c> (base64) is set, validates with that symmetric key instead (e.g. for environments without Identity).
/// </summary>
public static class JwtBearerExtensions
{
    public static IServiceCollection AddJwtBearerFromConfiguration(
        this IServiceCollection services,
        IConfiguration configuration,
        string identityUrlConfigKey = "App:IdentityUrl",
        Action<JwtBearerOptions>? configure = null)
    {
        var issuer = configuration["Jwt:Issuer"] ?? "pantry-identity";
        var audience = configuration["Jwt:Audience"] ?? "pantry-cloud";
        var identityUrl = configuration[identityUrlConfigKey] ?? "http://localhost:5072";
        var issuerSigningKeyBase64 = configuration["Jwt:IssuerSigningKey"];

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Audience = audience;
                if (!string.IsNullOrEmpty(issuerSigningKeyBase64))
                {
                    var key = new SymmetricSecurityKey(Convert.FromBase64String(issuerSigningKeyBase64));
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = issuer,
                        ValidateAudience = true,
                        ValidAudience = audience,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = key,
                    };
                }
                else
                {
                    options.MetadataAddress = $"{identityUrl}/.well-known/openid-configuration";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = issuer,
                        ValidateAudience = true,
                        ValidAudience = audience,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                    };
                }
                options.RequireHttpsMetadata = false;
                configure?.Invoke(options);
            });

        return services;
    }
}
