using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using PantryCloud.Identity.Application;
using PantryCloud.Identity.Core;
using PantryCloud.Identity.Core.Entities;

namespace PantryCloud.Identity.Infrastructure.Services;

public sealed class TokenProvider : ITokenProvider, IDisposable
{
    private readonly ApiConfiguration _configuration;
    private readonly SigningCredentials _signingCredentials;
    private readonly RSA _rsa;

    public TokenProvider(ApiConfiguration configuration)
    {
        var privateKeyPath = configuration.Jwt.PrivateKeyPath;
        _rsa = RSA.Create();
        var privateKey = File.ReadAllText(privateKeyPath);
        _rsa.ImportFromPem(privateKey.ToCharArray());

        var rsaSecurityKey = new RsaSecurityKey(_rsa)
        {
            KeyId = "key-id"
        };

        _signingCredentials = new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256);
        _configuration = configuration;
    }

    public string CreateAccessToken(ApplicationUser user, Guid? sessionId = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("email_verified", user.EmailVerified.ToString())
        };
        if (sessionId.HasValue)
        {
            claims.Add(new Claim("sid", sessionId.Value.ToString()));
        }

        var handler = new JsonWebTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_configuration.Jwt.ExpirationInMinutes),
            SigningCredentials = _signingCredentials,
            Issuer = _configuration.Jwt.Issuer,
            Audience = _configuration.Jwt.Audience,
        };

        return handler.CreateToken(tokenDescriptor);
    }

    public string CreateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    public string CreatePasswordResetToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    public string CreateVerifyEmailToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    public void Dispose()
    {
        _rsa.Dispose();
    }
}