using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace PantryCloud.ApiGateway.Infrastructure.Transforms;

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
            var correlationId = transformContext.HttpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                               ?? transformContext.HttpContext.Items["CorrelationId"]?.ToString()
                               ?? Guid.NewGuid().ToString();
            
            transformContext.ProxyRequest.Headers.Add("X-Correlation-Id", correlationId);
            
            return ValueTask.CompletedTask;
        });
    }
}


