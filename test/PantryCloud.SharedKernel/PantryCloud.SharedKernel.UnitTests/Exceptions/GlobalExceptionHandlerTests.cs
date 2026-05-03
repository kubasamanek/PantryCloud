using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PantryCloud.SharedKernel.Exceptions;
using Shouldly;

namespace PantryCloud.SharedKernel.UnitTests.Exceptions;

public class GlobalExceptionHandlerTests
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandlerTests()
    {
        _logger = Substitute.For<ILogger<GlobalExceptionHandler>>();
    }

    [Fact]
    public async Task TryHandleAsync_ShouldReturn401_WhenUnauthorizedAccessException()
    {
        // Arrange
        var exception = new UnauthorizedAccessException("Access denied");
        var handler = new GlobalExceptionHandler(_logger);
        var context = CreateHttpContext();

        // Act
        var result = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.ShouldBeTrue();
        context.Response.StatusCode.ShouldBe(StatusCodes.Status401Unauthorized);
        context.Response.ContentType.ShouldNotBeNull();
        context.Response.ContentType.ShouldContain("json");
        var problemDetails = await DeserializeProblemDetails(context);
        problemDetails!.Status.ShouldBe(StatusCodes.Status401Unauthorized);
        problemDetails.Title.ShouldBe(Constants.Exceptions.UnauthorizedTitle);
        problemDetails.Detail.ShouldBe(Constants.Exceptions.AccessDenied);
    }

    [Fact]
    public async Task TryHandleAsync_ShouldReturn400_WhenApplicationException()
    {
        // Arrange
        var exception = new ApplicationException(Constants.Exceptions.InvalidOperation);
        var handler = new GlobalExceptionHandler(_logger);
        var context = CreateHttpContext();

        // Act
        var result = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.ShouldBeTrue();
        context.Response.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        var problemDetails = await DeserializeProblemDetails(context);
        problemDetails!.Status.ShouldBe(StatusCodes.Status400BadRequest);
        problemDetails.Title.ShouldBe(Constants.Exceptions.BadRequestTitle);
        problemDetails.Detail.ShouldBe(Constants.Exceptions.InvalidOperation);
    }

    [Fact]
    public async Task TryHandleAsync_ShouldReturn500_WhenGenericException()
    {
        // Arrange
        var exception = new InvalidOperationException(Constants.Exceptions.SomethingWentWrong);
        var handler = new GlobalExceptionHandler(_logger);
        var context = CreateHttpContext();

        // Act
        var result = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.ShouldBeTrue();
        context.Response.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
        var problemDetails = await DeserializeProblemDetails(context);
        problemDetails!.Status.ShouldBe(StatusCodes.Status500InternalServerError);
        problemDetails.Title.ShouldBe(Constants.Exceptions.InternalServerErrorTitle);
        problemDetails.Detail.ShouldBe(Constants.Exceptions.SomethingWentWrong);
    }

    [Fact]
    public async Task TryHandleAsync_ShouldSetProblemDetailsType_ToExceptionTypeName()
    {
        // Arrange
        var exception = new ApplicationException(Constants.Exceptions.TestMessage);
        var handler = new GlobalExceptionHandler(_logger);
        var context = CreateHttpContext();

        // Act
        await handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        var problemDetails = await DeserializeProblemDetails(context);
        problemDetails!.Type.ShouldBe(Constants.Exceptions.ApplicationExceptionType);
    }

    [Fact]
    public async Task TryHandleAsync_ShouldLogError()
    {
        // Arrange
        var exception = new Exception(Constants.Exceptions.TestError);
        var handler = new GlobalExceptionHandler(_logger);
        var context = CreateHttpContext();

        // Act
        await handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<Microsoft.AspNetCore.Mvc.ProblemDetails?> DeserializeProblemDetails(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();
        return JsonSerializer.Deserialize<Microsoft.AspNetCore.Mvc.ProblemDetails>(body);
    }
}
