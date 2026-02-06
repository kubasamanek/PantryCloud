using Microsoft.AspNetCore.Http;
using PantryCloud.SharedKernel.Correlation;
using Shouldly;

namespace PantryCloud.SharedKernel.UnitTests.Correlation;

public class CorrelationIdMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ShouldUseHeaderValue_WhenCorrelationIdInRequest()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdConstants.HeaderName] = Constants.Correlation.ExistingCorrelationId;

        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = Constants.Http.StatusOk;
            return Task.CompletedTask;
        };

        var middleware = new CorrelationIdMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Items[CorrelationIdConstants.HttpContextItemKey].ShouldBe(Constants.Correlation.ExistingCorrelationId);
        context.Request.Headers[CorrelationIdConstants.HeaderName].ToString().ShouldBe(Constants.Correlation.ExistingCorrelationId);
        context.Response.Headers[CorrelationIdConstants.HeaderName].ToString().ShouldBe(Constants.Correlation.ExistingCorrelationId);
    }

    [Fact]
    public async Task InvokeAsync_ShouldGenerateNewCorrelationId_WhenHeaderMissing()
    {
        // Arrange
        var context = new DefaultHttpContext();

        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = Constants.Http.StatusOk;
            return Task.CompletedTask;
        };

        var middleware = new CorrelationIdMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var storedCorrelationId = context.Items[CorrelationIdConstants.HttpContextItemKey]?.ToString();
        storedCorrelationId.ShouldNotBeNullOrEmpty();
        Guid.TryParse(storedCorrelationId, out _).ShouldBeTrue();
        context.Response.Headers[CorrelationIdConstants.HeaderName].ToString().ShouldBe(storedCorrelationId);
    }

    [Fact]
    public async Task InvokeAsync_ShouldUseFirstValue_WhenMultipleCommaSeparatedValuesInHeader()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdConstants.HeaderName] = Constants.Correlation.HeaderValueMultipleIds;

        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = Constants.Http.StatusOk;
            return Task.CompletedTask;
        };

        var middleware = new CorrelationIdMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Items[CorrelationIdConstants.HttpContextItemKey].ShouldBe(Constants.Correlation.FirstCorrelationId);
        context.Response.Headers[CorrelationIdConstants.HeaderName].ToString().ShouldBe(Constants.Correlation.FirstCorrelationId);
    }

    [Fact]
    public async Task InvokeAsync_ShouldGenerateNewCorrelationId_WhenHeaderIsEmpty()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdConstants.HeaderName] = string.Empty;

        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = Constants.Http.StatusOk;
            return Task.CompletedTask;
        };

        var middleware = new CorrelationIdMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var storedCorrelationId = context.Items[CorrelationIdConstants.HttpContextItemKey]?.ToString();
        storedCorrelationId.ShouldNotBeNullOrEmpty();
        Guid.TryParse(storedCorrelationId, out _).ShouldBeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNext()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var nextCalled = false;

        RequestDelegate next = ctx =>
        {
            nextCalled = true;
            ctx.Response.StatusCode = Constants.Http.StatusNoContent;
            return Task.CompletedTask;
        };

        var middleware = new CorrelationIdMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.ShouldBeTrue();
        context.Response.StatusCode.ShouldBe(Constants.Http.StatusNoContent);
    }
}
