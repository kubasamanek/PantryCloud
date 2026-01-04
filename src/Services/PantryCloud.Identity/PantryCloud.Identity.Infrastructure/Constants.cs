namespace PantryCloud.Identity.Infrastructure;

public static class Constants
{
    public const string ResetPasswordEmailSubject = "Reset your PantryCloud password";

    public const string ResetPasswordEmailBodyTemplate = """
                                                         <p>Hello,</p>
                                                         <p>You requested a password reset. Click the link below to reset it:</p>
                                                         <p><a href="{0}">Reset your password</a></p>
                                                         <p>This link will expire in 1 hour.</p>
                                                         <p>If you didn’t request this, you can safely ignore this email.</p>
                                                         """;
}