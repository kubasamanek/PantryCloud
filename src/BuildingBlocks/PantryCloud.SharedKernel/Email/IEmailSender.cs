namespace PantryCloud.SharedKernel.Email;

/// <summary>
/// Abstraction for sending email. Implementations may send via SMTP or log only (simulation).
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends an email asynchronously.
    /// </summary>
    /// <param name="toEmail">Recipient email address.</param>
    /// <param name="subject">Email subject.</param>
    /// <param name="htmlBody">HTML body of the email.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
