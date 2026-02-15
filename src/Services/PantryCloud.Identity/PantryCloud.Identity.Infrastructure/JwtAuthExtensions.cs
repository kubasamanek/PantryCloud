using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PantryCloud.Identity.Core;
using PantryCloud.SharedKernel.Identity;

namespace PantryCloud.Identity.Infrastructure;

public static class JwtAuthExtensions
{
    public static IServiceCollection AddIdentityJwtAuth(this IServiceCollection services, ApiConfiguration config)
    {
        var publicKey = File.ReadAllText(config.Jwt.PublicKeyPath);
        using var rsa = RSA.Create();
        rsa.ImportFromPem(publicKey.ToCharArray());
        var key = new RsaSecurityKey(rsa.ExportParameters(false)) { KeyId = "key-id" };

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = config.Jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = config.Jwt.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                };
            });

        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<Application.IIdentityUserContext, IdentityUserContext>();
        services.AddAuthorization();

        return services;
    }
}
