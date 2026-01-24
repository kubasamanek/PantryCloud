using Microsoft.AspNetCore.Http;
using PantryCloud.SharedKernel.Http;

namespace PantryCloud.SharedKernel.Correlation;

/// <summary>
/// Implementation of <see cref="ICorrelationIdProvider"/> that retrieves correlation ID from HttpContext.
/// </summary>
public class CorrelationIdProvider(IHttpContextAccessor httpContextAccessor) : ICorrelationIdProvider
{
    public string? GetCorrelationId()
    {
         return httpContextAccessor.GetCorrelationIdFromContext();
    }
}


