using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PantryCloud.ApiGateway.Infrastructure.Middleware;
using Shouldly;

namespace PantryCloud.ApiGateway.UnitTests.Middleware;

public class CorrelationIdMiddlewareTests
{
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddlewareTests()
    {
        _logger = TestHelper.MockLogger<CorrelationIdMiddleware>();
    }

    [Fact]
    public async Task InvokeAsync_ShouldGenerateNewCorrelationId_WhenNotProvided()
    {
        // Arrange
        var context = TestHelper.CreateHttpContext();
        var nextCalled = false;
        var next = TestHelper.CreateMockNext(ctx => nextCalled = true);
        var middleware = new CorrelationIdMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        context.Response.Headers["X-Correlation-Id"].ToString().ShouldNotBeNullOrEmpty();
        context.Items["CorrelationId"].ShouldNotBeNull();
        context.Items["CorrelationId"]!.ToString().ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task InvokeAsync_ShouldUseExistingCorrelationId_WhenProvided()
    {
        // Arrange
        var expectedCorrelationId = "test-correlation-id-12345";
        var context = TestHelper.CreateHttpContext(correlationId: expectedCorrelationId);
        var nextCalled = false;
        var next = TestHelper.CreateMockNext(ctx => nextCalled = true);
        var middleware = new CorrelationIdMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        context.Response.Headers["X-Correlation-Id"].ToString().ShouldBe(expectedCorrelationId);
        context.Items["CorrelationId"]!.ToString().ShouldBe(expectedCorrelationId);
    }

    [Fact]
    public async Task InvokeAsync_ShouldSetCorrelationIdInResponseHeaders()
    {
        // Arrange
        var context = TestHelper.CreateHttpContext();
        var next = TestHelper.CreateMockNext();
        var middleware = new CorrelationIdMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.ContainsKey("X-Correlation-Id").ShouldBeTrue();
        context.Response.Headers["X-Correlation-Id"].ToString().ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task InvokeAsync_ShouldSetCorrelationIdInContextItems()
    {
        // Arrange
        var context = TestHelper.CreateHttpContext();
        var next = TestHelper.CreateMockNext();
        var middleware = new CorrelationIdMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Items.ContainsKey("CorrelationId").ShouldBeTrue();
        context.Items["CorrelationId"].ShouldNotBeNull();
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNextMiddleware()
    {
        // Arrange
        var context = TestHelper.CreateHttpContext();
        var nextCalled = false;
        var next = TestHelper.CreateMockNext(ctx => nextCalled = true);
        var middleware = new CorrelationIdMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
    }
}


