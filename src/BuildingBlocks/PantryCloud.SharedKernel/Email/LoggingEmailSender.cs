using Microsoft.Extensions.Logging;

namespace PantryCloud.SharedKernel.Email;

/// <summary>
/// Email sender that logs the email instead of sending. Used for simulation and tests.
/// </summary>
public class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Email would be sent to {ToEmail}, Subject: {Subject}. Body length: {BodyLength} chars.",
            toEmail,
            subject,
            htmlBody?.Length ?? 0);
        return Task.CompletedTask;
    }
}
