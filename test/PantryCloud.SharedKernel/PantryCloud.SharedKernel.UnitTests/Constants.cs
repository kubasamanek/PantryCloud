namespace PantryCloud.SharedKernel.UnitTests;

internal static class Constants
{
    public static class Correlation
    {
        public const string ExistingCorrelationId = "existing-correlation-id-123";
        public const string FirstCorrelationId = "first-id";
        public const string HeaderValueMultipleIds = "first-id, second-id, third-id";
        public const string TestCorrelationId = "test-correlation-id-123";
        public const string HeaderCorrelationId = "header-correlation-id-456";
        public const string ItemsCorrelationId = "items-correlation-id";
        public const string HeaderCorrelationIdShort = "header-correlation-id";
        public const string ItemsCorrelationIdShort = "items-id";
        public const string HeaderCorrelationIdShortAlt = "header-id";
        public const string ProviderCorrelationId = "provider-correlation-id";
        public const string WhitespaceCorrelationId = "   ";
    }

    public static class Http
    {
        public const int StatusOk = 200;
        public const int StatusNoContent = 204;
        public const string ProblemDetailsContentType = "application/problem+json";
    }

    public static class Exceptions
    {
        public const string AccessDenied = "Access denied";
        public const string InvalidOperation = "Invalid operation";
        public const string SomethingWentWrong = "Something went wrong";
        public const string TestMessage = "Test";
        public const string TestError = "Test error";
        public const string UnauthorizedTitle = "Unauthorized";
        public const string BadRequestTitle = "Bad Request";
        public const string InternalServerErrorTitle = "Internal Server Error";
        public const string ApplicationExceptionType = "ApplicationException";
    }

    public static class Concurrency
    {
        public const string ConcurrencyConflictMessage = "Concurrency conflict";
        public const string UserEntityName = "User";
        public const string ProductEntityName = "Product";
        public const string OrderEntityName = "Order";
        public const string InvoiceEntityName = "Invoice";
        public const string CategoryEntityName = "Category";
        public const string EntityEntityName = "Entity";
        public const string UserConcurrencyConflictCode = "User.ConcurrencyConflict";
        public const string UserConcurrencyConflictDescription = "The User was modified by another user. Please refresh and try again.";
        public const string ProductStaleDataCode = "Product.StaleData";
        public const string ProductStaleDataDescription = "Product data is stale";
        public const string ProductConcurrencyConflictCode = "Product.ConcurrencyConflict";
        public const string CustomErrorCode = "Custom.Error";
        public const string CustomErrorMessage = "Custom message";
        public const string SuccessResult = "success";
    }

    public static class Validation
    {
        public const string TestValue = "test";
        public const string ValidValue = "valid";
        public const string SuccessResult = "success";
        public const string ValueField = "Value";
        public const string ValueRequired = "Value is required";
        public const string ValueMustNotBeEmpty = "Value must not be empty";
        public const string ErrorFromValidator1 = "Error from validator 1";
        public const string AnotherField = "AnotherField";
        public const string ErrorFromValidator2 = "Error from validator 2";
    }

    public static class Controllers
    {
        public const string TestValue = "test-value";
        public const int TestId = 123;
        public const string AuthInvalidCode = "Auth.Invalid";
        public const string InvalidCredentials = "Invalid credentials";
        public const string UserNotFoundCode = "User.NotFound";
        public const string UserNotFound = "User not found";
        public const string UserExistsCode = "User.Exists";
        public const string UserAlreadyExists = "User already exists";
        public const string OperationFailedCode = "Operation.Failed";
        public const string OperationFailed = "Operation failed";
        public const string EmailField = "Email";
        public const string EmailRequired = "Email is required";
        public const string PasswordField = "Password";
        public const string PasswordTooShort = "Password is too short";
    }

    public static class Identity
    {
        public const string TestEmail = "test@example.com";
        public const string JwtEmail = "jwt@example.com";
        public const string UserIdNotFoundMessage = "User ID not found in token";
        public const string EmailNotFoundMessage = "Email not found in token";
        public const string NoHttpContextMessage = "No HttpContext found";
        public const string UserNotAuthenticatedMessage = "User not authenticated";
        public const string NotAGuid = "not-a-guid";
        public const string TestAuthType = "TestAuth";
    }

    public static class Persistence
    {
        public static readonly Guid ExistingEntityId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        public static readonly Guid NonExistentEntityId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        public static readonly Guid AuditUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        public const string TestEntityName = "Test";
    }

    public static class Queryable
    {
        public const string Apple = "Apple";
        public const string Banana = "Banana";
        public const string Cherry = "Cherry";
        public const string Pineapple = "Pineapple";
        public const string Grapple = "Grapple";
        public const string AppSearchTerm = "app";
        public const string XyzSearchTerm = "xyz";
        public const string AppleSearchTerm = "apple";
        public const string AppleSearchTermUpper = "APPLE";
        public const string WhitespaceSearchTerm = "   ";
    }
}
