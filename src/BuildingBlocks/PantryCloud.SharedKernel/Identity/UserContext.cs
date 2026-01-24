using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace PantryCloud.SharedKernel.Identity;

/// <summary>
/// Implementation of <see cref="IUserContext"/> that extracts user information from JWT claims.
/// </summary>
public sealed class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    private ClaimsPrincipal User
    {
        get
        {
            var context = httpContextAccessor.HttpContext
                          ?? throw new UnauthorizedAccessException("No HttpContext found");

            var user = context.User;
            if (user == null || user.Identity?.IsAuthenticated != true)
                throw new UnauthorizedAccessException("User not authenticated");

            return user;
        }
    }

    public Guid UserId =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                      User.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id)
            ? id
            : throw new UnauthorizedAccessException("User ID not found in token");

    public string Email =>
        User.FindFirstValue(ClaimTypes.Email)
        ?? User.FindFirstValue(JwtRegisteredClaimNames.Email)
        ?? throw new UnauthorizedAccessException("Email not found in token");
}

