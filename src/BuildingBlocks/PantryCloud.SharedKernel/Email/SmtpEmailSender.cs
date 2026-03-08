using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace PantryCloud.SharedKernel.Email;

/// <summary>
/// Email sender that sends via SMTP using MailKit.
/// </summary>
public class SmtpEmailSender(
    IOptions<EmailSenderOptions> options,
    ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var opts = options.Value;
        if (string.IsNullOrEmpty(opts.Host))
        {
            logger.LogWarning("Email sender configured but Host is empty; skipping send to {ToEmail}", toEmail);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(string.IsNullOrEmpty(opts.From) ? "noreply@pantrycloud.local" : opts.From));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = htmlBody };
        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(opts.Host, opts.Port, SecureSocketOptions.StartTlsWhenAvailable, cancellationToken);
        if (!string.IsNullOrEmpty(opts.UserName))
            await client.AuthenticateAsync(opts.UserName, opts.Password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
        logger.LogInformation("Email sent to {ToEmail}, Subject: {Subject}", toEmail, subject);
    }
}
