using System.Security.Cryptography;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace PantryCloud.Identity.Presentation.Controllers;

[ApiController]
[Route("api/jwks")]
public class JwksController(IMediator mediator, IMapper mapper, IConfiguration configuration) : ApiControllerBase(mediator, mapper)
{
    [HttpGet("/.well-known/openid-configuration/jwks")]
    public IActionResult GetJwks()
    {
        var publicKey = System.IO.File.ReadAllText(configuration["Jwt:PublicKeyPath"]!);

        using var rsa = RSA.Create();
        rsa.ImportFromPem(publicKey.ToCharArray());

        var rsaParameters = rsa.ExportParameters(false);
        var key = new RsaSecurityKey(rsaParameters)
        {
            KeyId = "key-id" 
        };

        var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(key);
        jwk.Use = "sig";
        jwk.Alg = SecurityAlgorithms.RsaSha256;

        var jwks = new { keys = new[] { jwk } };
        return Ok(jwks);
    }
    
    [HttpGet("/.well-known/openid-configuration")]
    public IActionResult GetOpenIdConfiguration()
    {
        var issuer = configuration["Jwt:Issuer"]!;
        var jwksUri = $"{issuer}/.well-known/openid-configuration/jwks";

        var discoveryDocument = new
        {
            issuer,
            jwks_uri = jwksUri,
            token_endpoint = $"{issuer}/api/auth/login",
            authorization_endpoint = $"{issuer}/api/auth/login", // optional, for OAuth2 compatibility
            response_types_supported = new[] { "token" },
            subject_types_supported = new[] { "public" },
            id_token_signing_alg_values_supported = new[] { "RS256" }
        };

        return Ok(discoveryDocument);
    }
}