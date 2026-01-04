using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PantryCloud.Household.Application;

namespace PantryCloud.Household.Infrastructure.Services;

public sealed class UserContext : IUserContext
{
    private readonly ClaimsPrincipal _user;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        var context = httpContextAccessor.HttpContext
                      ?? throw new UnauthorizedAccessException("No HttpContext found");

        _user = context.User ?? throw new UnauthorizedAccessException("User not authenticated");
    }

    public Guid UserId =>
        Guid.TryParse(_user.FindFirstValue(ClaimTypes.NameIdentifier) ??
                      _user.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id)
            ? id
            : throw new UnauthorizedAccessException("User ID not found in token");

    public string Email =>
        _user.FindFirstValue(ClaimTypes.Email)
        ?? throw new UnauthorizedAccessException("Email not found in token");
}