using PantryCloud.SharedKernel.Correlation;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace PantryCloud.ApiGateway.Infrastructure.Transforms;

/// <summary>
/// Ensures distributed tracing context is propagated to downstream services.
/// Intercepts outgoing proxy requests and injects the 'X-Correlation-Id' header, 
/// retrieving it from the incoming request or generating a new one if missing.
/// This guarantees that a single request chain can be tracked across the entire microservices cluster.
/// </summary>
public class CorrelationIdTransformProvider : ITransformProvider
{
    public void ValidateRoute(TransformRouteValidationContext context)
    {
    }

    public void ValidateCluster(TransformClusterValidationContext context)
    {
    }

    public void Apply(TransformBuilderContext context)
    {
        context.AddRequestTransform(transformContext =>
        {
            var correlationId = transformContext.HttpContext.Items[CorrelationIdConstants.HttpContextItemKey]?.ToString()
                               ?? transformContext.HttpContext.Request.Headers[CorrelationIdConstants.HeaderName].FirstOrDefault()
                               ?? Guid.NewGuid().ToString();

            transformContext.ProxyRequest.Headers.Add(CorrelationIdConstants.HeaderName, correlationId);

            return ValueTask.CompletedTask;
        });
    }
}


