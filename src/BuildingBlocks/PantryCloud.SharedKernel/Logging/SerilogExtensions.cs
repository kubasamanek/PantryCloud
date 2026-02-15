using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace PantryCloud.SharedKernel.Logging;

/// <summary>
/// Extension methods for configuring Serilog structured logging with correlation ID support.
/// Uses Serilog.Enrichers.CorrelationId for proper correlation ID handling.
/// </summary>
public static class SerilogExtensions
{
    /// <summary>
    /// Configures Serilog for microservices with correlation ID support.
    /// - Development: Console sink with readable output
    /// - Production: Compact JSON format for log aggregation tools (ELK, Splunk, Datadog)
    /// Correlation IDs are automatically included via Serilog.Enrichers.CorrelationId.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSerilogLogging(this IServiceCollection services, IConfiguration configuration)
    {
        var environmentName = configuration["ASPNETCORE_ENVIRONMENT"]
                            ?? configuration["Environment"]
                            ?? "Production";

        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("Application", configuration["ApplicationName"] ?? "PantryCloud");

        // Production: Compact JSON format (structured for log aggregation)
        if (string.Equals(environmentName, "Production", StringComparison.OrdinalIgnoreCase))
        {
            loggerConfiguration.WriteTo.Console(new CompactJsonFormatter());
        }
        // Development: Readable console output with correlation ID
        else
        {
            loggerConfiguration.WriteTo.Console(
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj} (CorrelationId: {CorrelationId}){NewLine}{Exception}");
        }

        Log.Logger = loggerConfiguration.CreateLogger();

        services.AddSerilog();

        return services;
    }
}
