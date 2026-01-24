using Microsoft.Extensions.DependencyInjection;
using PantryCloud.Identity.Infrastructure.Persistence;
using PantryCloud.SharedKernel.Testing.Infrastructure;

namespace PantryCloud.Identity.IntegrationTests.Infrastructure;

/// <summary>
/// Identity service-specific WebApplicationFactory for integration tests.
/// </summary>
public class IdentityIntegrationTestWebAppFactory 
    : IntegrationTestWebAppFactory<Program, ApplicationDbContext>
{
    protected override void ConfigureTestServices(IServiceCollection services)
    {
        // Disable email sending in tests
        services.Configure<Core.ApiConfiguration>(config =>
        {
            config.App.SendEmails = false;
        });
    }
}

