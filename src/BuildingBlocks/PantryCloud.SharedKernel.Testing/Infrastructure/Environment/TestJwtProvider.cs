using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace PantryCloud.SharedKernel.Testing.Infrastructure.Environment;

/// <summary>
/// Generates JWT tokens for integration tests using a symmetric key.
/// </summary>
public sealed class TestJwtProvider(
    string base64Secret,
    string issuer = "pantry-identity",
    string audience = "pantry-cloud")
{
    /// <summary>
    /// Creates a JWT with sub and email claims. Email defaults to user-{userId}@test.local if not provided.
    /// </summary>
    public string CreateToken(Guid userId, string? email = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email ?? $"user-{userId:N}@test.local")
        };
        return CreateToken(claims);
    }

    private string CreateToken(IEnumerable<Claim> claims)
    {
        var key = new SymmetricSecurityKey(Convert.FromBase64String(base64Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string GenerateSecret() =>
        Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
}
