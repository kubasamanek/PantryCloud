using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PantryCloud.Household.Infrastructure.Persistence;
using PantryCloud.Household.IntegrationTests.Constants;
using PantryCloud.SharedKernel.Testing.Infrastructure;
using PantryCloud.SharedKernel.Testing.Infrastructure.Environment;
using PantryCloud.SharedKernel.Testing.Infrastructure.RabbitMq;

namespace PantryCloud.Household.IntegrationTests.Infrastructure;

/// <summary>
/// WebApplicationFactory for Household API with Testcontainers Postgres, RabbitMQ, and JWT validation
/// </summary>
public sealed class HouseholdWebAppFactory(Func<string>? getRabbitMqHostPort = null)
    : IntegrationTestWebAppFactory<Program, HouseholdDbContext>
{
    private static readonly string JwtSecret = TestJwtProvider.GenerateSecret();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        var dict = new Dictionary<string, string?>
        {
            ["Jwt:IssuerSigningKey"] = JwtSecret,
            ["Jwt:Issuer"] = TestConstants.Jwt.Issuer,
            ["Jwt:Audience"] = TestConstants.Jwt.Audience
        };

        if (getRabbitMqHostPort != null)
        {
            var hostPort = getRabbitMqHostPort();
            var parts = hostPort.Contains(':')
                ? hostPort.Split(':', 2)
                : [hostPort, RabbitMqTestOptions.AmqpPort.ToString()];
            dict["Messaging:RabbitMQ:Host"] = parts[0];
            dict["Messaging:RabbitMQ:Port"] = parts.Length > 1 ? parts[1] : RabbitMqTestOptions.AmqpPort.ToString();
            dict["Messaging:RabbitMQ:Username"] = RabbitMqTestOptions.DefaultUser;
            dict["Messaging:RabbitMQ:Password"] = RabbitMqTestOptions.DefaultPassword;
        }

        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(dict));
    }

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        base.ConfigureTestServices(services);

        // Post-configure JWT to use our symmetric key so tokens from TestJwtProvider are accepted.
        var key = new SymmetricSecurityKey(Convert.FromBase64String(JwtSecret));
        services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = TestConstants.Jwt.Issuer,
                ValidateAudience = true,
                ValidAudience = TestConstants.Jwt.Audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key
            };
        });
    }
}
