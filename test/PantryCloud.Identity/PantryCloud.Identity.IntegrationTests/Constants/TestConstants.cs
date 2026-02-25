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
        public const string Register = "/api/v1/auth/register";
        public const string Login = "/api/v1/auth/login";
        public const string Refresh = "/api/v1/auth/refresh";
        public const string ForgotPassword = "/api/v1/auth/forgot-password";
        public const string ResetPassword = "/api/v1/auth/reset-password";
        public const string VerifyEmail = "/api/v1/auth/verify-email";
        public const string Sessions = "/api/v1/auth/sessions";
        public const string Logout = "/api/v1/auth/logout";
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

