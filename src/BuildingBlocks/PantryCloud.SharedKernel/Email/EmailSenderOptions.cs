namespace PantryCloud.SharedKernel.Email;

/// <summary>
/// Configuration for SMTP-based email sending. Bind from config (e.g. Email:Host, Email:Port).
/// </summary>
public class EmailSenderOptions
{
    public const string SectionName = "Email";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
}
