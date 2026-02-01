using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace PantryCloud.Notification.IntegrationTests.Infrastructure.Environment;

public sealed class TestJwtProvider(string base64Secret)
{
    public string CreateToken(Guid userId)
    {
        var key = new SymmetricSecurityKey(Convert.FromBase64String(base64Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: Constants.Jwt.Issuer,
            audience: Constants.Jwt.Audience,
            claims: [new Claim(JwtRegisteredClaimNames.Sub, userId.ToString())],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string GenerateSecret() =>
        Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
}
