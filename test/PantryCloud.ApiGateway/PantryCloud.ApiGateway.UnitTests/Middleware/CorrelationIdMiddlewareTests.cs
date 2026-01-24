using Microsoft.AspNetCore.Http;
using PantryCloud.SharedKernel.Correlation;
using Shouldly;

namespace PantryCloud.ApiGateway.UnitTests.Middleware;

public class CorrelationIdMiddlewareTests
{

    [Fact]
    public async Task InvokeAsync_ShouldGenerateNewCorrelationId_WhenNotProvided()
    {
        var context = TestHelper.CreateHttpContext();
        var nextCalled = false;
        var next = TestHelper.CreateMockNext(ctx => nextCalled = true);
        var middleware = new CorrelationIdMiddleware(next);

        await middleware.InvokeAsync(context);

        nextCalled.ShouldBeTrue();
        context.Response.Headers["X-Correlation-Id"].ToString().ShouldNotBeNullOrEmpty();
        context.Items["CorrelationId"].ShouldNotBeNull();
        context.Items["CorrelationId"]!.ToString().ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task InvokeAsync_ShouldUseExistingCorrelationId_WhenProvided()
    {
        const string expectedCorrelationId = "test-correlation-id-12345";
        var context = TestHelper.CreateHttpContext(correlationId: expectedCorrelationId);
        var nextCalled = false;
        var next = TestHelper.CreateMockNext(ctx => nextCalled = true);
        var middleware = new CorrelationIdMiddleware(next);

        await middleware.InvokeAsync(context);

        nextCalled.ShouldBeTrue();
        context.Response.Headers["X-Correlation-Id"].ToString().ShouldBe(expectedCorrelationId);
        context.Items["CorrelationId"]!.ToString().ShouldBe(expectedCorrelationId);
    }

    [Fact]
    public async Task InvokeAsync_ShouldSetCorrelationIdInResponseHeaders()
    {
        var context = TestHelper.CreateHttpContext();
        var next = TestHelper.CreateMockNext();
        var middleware = new CorrelationIdMiddleware(next);

        await middleware.InvokeAsync(context);

        context.Response.Headers.ContainsKey("X-Correlation-Id").ShouldBeTrue();
        context.Response.Headers["X-Correlation-Id"].ToString().ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task InvokeAsync_ShouldSetCorrelationIdInRequestHeaders()
    {
        var context = TestHelper.CreateHttpContext();
        var next = TestHelper.CreateMockNext();
        var middleware = new CorrelationIdMiddleware(next);

        await middleware.InvokeAsync(context);

        context.Request.Headers.ContainsKey("X-Correlation-Id").ShouldBeTrue();
        context.Request.Headers["X-Correlation-Id"].ToString().ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task InvokeAsync_ShouldSetCorrelationIdInContextItems()
    {
        var context = TestHelper.CreateHttpContext();
        var next = TestHelper.CreateMockNext();
        var middleware = new CorrelationIdMiddleware(next);

        await middleware.InvokeAsync(context);

        context.Items.ContainsKey("CorrelationId").ShouldBeTrue();
        context.Items["CorrelationId"].ShouldNotBeNull();
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNextMiddleware()
    {
        var context = TestHelper.CreateHttpContext();
        var nextCalled = false;
        var next = TestHelper.CreateMockNext(ctx => nextCalled = true);
        var middleware = new CorrelationIdMiddleware(next);

        await middleware.InvokeAsync(context);

        nextCalled.ShouldBeTrue();
    }
}


