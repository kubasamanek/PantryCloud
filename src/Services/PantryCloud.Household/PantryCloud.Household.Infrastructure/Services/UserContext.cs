using Microsoft.AspNetCore.Http;
using PantryCloud.Household.Application;

namespace PantryCloud.Household.Infrastructure.Services;

public sealed class UserContext : IUserContext
{
    private readonly HttpContext _context;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _context = httpContextAccessor.HttpContext
                   ?? throw new UnauthorizedAccessException("No HttpContext found");
    }

    public Guid UserId
    {
        get
        {
            if (!_context.Request.Headers.TryGetValue("X-User-Id", out var userIdHeader))
            {
                throw new UnauthorizedAccessException("X-User-Id header not found. Request must come through the API Gateway.");
            }

            var userIdValue = userIdHeader.ToString();
            if (!Guid.TryParse(userIdValue, out var userId))
            {
                throw new UnauthorizedAccessException($"Invalid user ID format in X-User-Id header: {userIdValue}");
            }

            return userId;
        }
    }

    public string Email
    {
        get
        {
            if (!_context.Request.Headers.TryGetValue("X-User-Email", out var emailHeader))
            {
                throw new UnauthorizedAccessException("X-User-Email header not found. Request must come through the API Gateway.");
            }

            var email = emailHeader.ToString();
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new UnauthorizedAccessException("Email header is empty.");
            }

            return email;
        }
    }
}