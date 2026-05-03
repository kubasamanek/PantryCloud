using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PantryCloud.Identity.Application;
using PantryCloud.Identity.Core;
using PantryCloud.Identity.Infrastructure.Persistence;
using PantryCloud.Identity.Infrastructure.Services;
using PantryCloud.SharedKernel.Correlation;
using PantryCloud.SharedKernel.Email;
using PantryCloud.SharedKernel.Extensions;
using PantryCloud.SharedKernel.Observability;

namespace PantryCloud.Identity.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureLayerServices(this IServiceCollection services, IConfiguration configuration)
    {
        var apiConfiguration = new ApiConfiguration();
        configuration.Bind(apiConfiguration);
        services.AddSingleton(apiConfiguration);

        services.AddSingleton<ITokenProvider, TokenProvider>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddIdentityJwtAuth(apiConfiguration);

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddCorrelationId();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddHealthChecks().AddNpgSql(connectionString!);
        services.AddOpenTelemetryTracing(configuration);
        services.AddApiVersioningDefaults();

        services.Configure<EmailSenderOptions>(opts =>
        {
            opts.Host = apiConfiguration.Email.Host;
            opts.Port = apiConfiguration.Email.Port;
            opts.From = apiConfiguration.Email.From;
            opts.UserName = apiConfiguration.Email.Username;
            opts.Password = apiConfiguration.Email.Password;
        });
        if (apiConfiguration.App.SendEmails)
            services.AddSmtpEmailSender();
        else
            services.AddLoggingEmailSender();

        return services;
    }
}