using System.Security.Cryptography;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PantryCloud.Identity.Core;
using PantryCloud.SharedKernel.Controllers;

namespace PantryCloud.Identity.Presentation.Controllers;

[ApiController]
[Route("api/jwks")]
[AllowAnonymous]
public class JwksController(IMediator mediator, IMapper mapper, ApiConfiguration apiConfiguration) : ApiControllerBase(mediator, mapper)
{
    [HttpGet("/.well-known/openid-configuration/jwks")]
    public IActionResult GetJwks()
    {
        var publicKey = System.IO.File.ReadAllText(apiConfiguration.Jwt.PublicKeyPath);

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
        // Issuer: use AuthorityBaseUrl when set, else request host or Jwt.Issuer.
        var issuer = !string.IsNullOrEmpty(apiConfiguration.App.AuthorityBaseUrl)
            ? apiConfiguration.App.AuthorityBaseUrl.TrimEnd('/')
            : $"{Request.Scheme}://{Request.Host}";

        if (string.IsNullOrEmpty(apiConfiguration.App.AuthorityBaseUrl))
        {
            issuer = apiConfiguration.Jwt.Issuer;
        }

        // jwks_uri: use the same host that was used to fetch this document
        var jwksUri = $"{Request.Scheme}://{Request.Host}/.well-known/openid-configuration/jwks";
        var endpointsBase = !string.IsNullOrEmpty(apiConfiguration.App.AuthorityBaseUrl)
            ? apiConfiguration.App.AuthorityBaseUrl.TrimEnd('/')
            : $"{Request.Scheme}://{Request.Host}";

        var discoveryDocument = new
        {
            issuer,
            jwks_uri = jwksUri,
            token_endpoint = $"{endpointsBase}/api/auth/login",
            authorization_endpoint = $"{endpointsBase}/api/auth/login",
            response_types_supported = new[] { "token" },
            subject_types_supported = new[] { "public" },
            id_token_signing_alg_values_supported = new[] { "RS256" }
        };

        return Ok(discoveryDocument);
    }
}