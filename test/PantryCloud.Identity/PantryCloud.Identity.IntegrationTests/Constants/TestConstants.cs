namespace PantryCloud.Identity.IntegrationTests.Constants;

/// <summary>
/// Centralized constants for integration tests to avoid hardcoding values.
/// </summary>
public static class TestConstants
{
    /// <summary>
    /// API endpoint URLs
    /// </summary>
    public static class Endpoints
    {
        public const string Register = "/api/auth/register";
        public const string Login = "/api/auth/login";
        public const string Refresh = "/api/auth/refresh";
        public const string ForgotPassword = "/api/auth/forgot-password";
        public const string ResetPassword = "/api/auth/reset-password";
        public const string VerifyEmail = "/api/auth/verify-email";
    }

    /// <summary>
    /// Test user data
    /// </summary>
    public static class Users
    {
        public const string DefaultEmail = "test@pantrycloud.com";
        public const string DefaultPassword = "Test123!";
        
        public const string AlternativeEmail = "alice@pantrycloud.com";
        public const string AlternativePassword = "Alice123!";
    }

    /// <summary>
    /// Invalid data for testing validation
    /// </summary>
    public static class InvalidData
    {
        public const string EmptyEmail = "";
        public const string InvalidEmailFormat = "not-an-email";
        public const string WeakPassword = "123";
        public const string EmptyPassword = "";
    }
}

