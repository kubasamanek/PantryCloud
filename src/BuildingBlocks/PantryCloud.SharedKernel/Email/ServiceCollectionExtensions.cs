using Microsoft.Extensions.DependencyInjection;

namespace PantryCloud.SharedKernel.Email;

/// <summary>
/// Extension methods for registering email sender implementations.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="LoggingEmailSender"/> as <see cref="IEmailSender"/>. Use for simulation or tests.
    /// </summary>
    public static IServiceCollection AddLoggingEmailSender(this IServiceCollection services)
    {
        services.AddScoped<IEmailSender, LoggingEmailSender>();
        return services;
    }

    /// <summary>
    /// Registers <see cref="SmtpEmailSender"/> as <see cref="IEmailSender"/>. Requires <see cref="EmailSenderOptions"/> to be configured (e.g. services.Configure&lt;EmailSenderOptions&gt;(configuration.GetSection(EmailSenderOptions.SectionName))).
    /// </summary>
    public static IServiceCollection AddSmtpEmailSender(this IServiceCollection services)
    {
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        return services;
    }
}
