using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;
using PantryCloud.Identity.Application;

namespace PantryCloud.Identity.Infrastructure;

public sealed class IdentityUserContext(IHttpContextAccessor httpContextAccessor) : IIdentityUserContext
{
    private ClaimsPrincipal User
    {
        get
        {
            var context = httpContextAccessor.HttpContext
                ?? throw new UnauthorizedAccessException("No HttpContext found");
            var user = context.User;
            if (user?.Identity?.IsAuthenticated != true)
                throw new UnauthorizedAccessException("User not authenticated");
            return user;
        }
    }

    public Guid UserId =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id)
            ? id
            : throw new UnauthorizedAccessException("User ID not found in token");

    public string Email =>
        User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(JwtRegisteredClaimNames.Email)
        ?? throw new UnauthorizedAccessException("Email not found in token");

    public Guid? SessionId =>
        Guid.TryParse(User.FindFirstValue("sid"), out var sid) ? sid : null;
}
