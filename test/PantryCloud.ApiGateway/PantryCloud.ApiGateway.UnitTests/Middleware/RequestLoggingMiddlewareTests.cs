using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PantryCloud.ApiGateway.Infrastructure.Middleware;
using Shouldly;

namespace PantryCloud.ApiGateway.UnitTests.Middleware;

public class RequestLoggingMiddlewareTests
{
    private readonly ILogger<RequestLoggingMiddleware> _logger = TestHelper.MockLogger<RequestLoggingMiddleware>();

    [Fact]
    public async Task InvokeAsync_ShouldLogRequest_WhenCalled()
    {
        var context = TestHelper.CreateHttpContext(method: "GET", path: "/api/test");
        context.Items["CorrelationId"] = "test-correlation-id";
        var nextCalled = false;
        var next = TestHelper.CreateMockNext(ctx => nextCalled = true);
        var middleware = new RequestLoggingMiddleware(next, _logger);

        await middleware.InvokeAsync(context);

        nextCalled.ShouldBeTrue();
        _logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Incoming request")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogCompletion_WhenRequestSucceeds()
    {
        var context = TestHelper.CreateHttpContext(method: "GET", path: "/api/test");
        context.Items["CorrelationId"] = "test-correlation-id";
        context.Response.StatusCode = 200;
        var next = TestHelper.CreateMockNext();
        var middleware = new RequestLoggingMiddleware(next, _logger);

        await middleware.InvokeAsync(context);

        _logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Request completed")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogError_WhenExceptionOccurs()
    {
        var context = TestHelper.CreateHttpContext(method: "GET", path: "/api/test");
        context.Items["CorrelationId"] = "test-correlation-id";
        var expectedException = new Exception("Test exception");
        var next = new RequestDelegate(_ => throw expectedException);
        var middleware = new RequestLoggingMiddleware(next, _logger);

        await Should.ThrowAsync<Exception>(async () => await middleware.InvokeAsync(context));

        _logger.Received().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Request failed")),
            Arg.Is<Exception>(e => e == expectedException),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task InvokeAsync_ShouldUseCorrelationIdFromContext()
    {
        var correlationId = "test-correlation-id-123";
        var context = TestHelper.CreateHttpContext();
        context.Items["CorrelationId"] = correlationId;
        var next = TestHelper.CreateMockNext();
        var middleware = new RequestLoggingMiddleware(next, _logger);

        await middleware.InvokeAsync(context);

        _logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains(correlationId)),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNextMiddleware()
    {
        var context = TestHelper.CreateHttpContext();
        context.Items["CorrelationId"] = "test-id";
        var nextCalled = false;
        var next = TestHelper.CreateMockNext(ctx => nextCalled = true);
        var middleware = new RequestLoggingMiddleware(next, _logger);

        await middleware.InvokeAsync(context);

        nextCalled.ShouldBeTrue();
    }
}


