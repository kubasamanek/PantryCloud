using System.Security.Claims;
using System.Text.Json;

namespace PantryCloud.Web.Services.Auth;

/// <summary>
/// Decodes JWT payload (no signature verification; server validates).
/// Reads exp and email for auth state and expiry check.
/// </summary>
public sealed class JwtClaimsHelper
{
    private const string SubClaim = "sub";
    private const string EmailClaim = "email";
    private const string ExpClaim = "exp";
    private const string IatClaim = "iat";

    /// <summary>
    /// Returns (email, expUnixSeconds, iatUnixSeconds) from the access token payload. iat may be null if not present.
    /// </summary>
    public (string? Email, long? Exp, long? Iat) DecodePayload(string? accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            return (null, null, null);

        var parts = accessToken.Split('.');
        if (parts.Length != 3)
            return (null, null, null);

        try
        {
            var payload = Base64UrlDecode(parts[1]);
            if (string.IsNullOrEmpty(payload))
                return (null, null, null);

            using var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;
            var email = root.TryGetProperty(EmailClaim, out var e) ? e.GetString() : null;
            long? exp = root.TryGetProperty(ExpClaim, out var ex) && ex.ValueKind == JsonValueKind.Number
                ? ex.GetInt64()
                : null;
            long? iat = root.TryGetProperty(IatClaim, out var i) && i.ValueKind == JsonValueKind.Number
                ? i.GetInt64()
                : null;
            return (email, exp, iat);
        }
        catch
        {
            return (null, null, null);
        }
    }

    /// <summary>
    /// Returns the user id (sub claim) from the access token payload, or null if missing or invalid.
    /// </summary>
    public Guid? GetUserId(string? accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return null;
        }

        var parts = accessToken.Split('.');
        if (parts.Length != 3)
        {
            return null;
        }

        try
        {
            var payload = Base64UrlDecode(parts[1]);
            if (string.IsNullOrEmpty(payload))
                return null;

            using var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;
            var sub = root.TryGetProperty(SubClaim, out var s) ? s.GetString() : null;
            return Guid.TryParse(sub, out var userId) ? userId : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// True if exp is present and in the past (with 60s buffer).
    /// </summary>
    public bool IsExpired(long? expUnixSeconds)
    {
        if (expUnixSeconds == null) return true;
        var utcNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return expUnixSeconds.Value < utcNow + 60;
    }

    public ClaimsPrincipal BuildPrincipal(string? email)
    {
        if (string.IsNullOrEmpty(email))
            return new ClaimsPrincipal(new ClaimsIdentity());

        var identity = new ClaimsIdentity("pantrycloud", "email", "role");
        identity.AddClaim(new Claim(ClaimTypes.Email, email));
        identity.AddClaim(new Claim(ClaimTypes.Name, email));
        return new ClaimsPrincipal(identity);
    }

    private static string? Base64UrlDecode(string input)
    {
        if (string.IsNullOrEmpty(input)) return null;
        var base64 = input.Replace('-', '+').Replace('_', '/');
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        try
        {
            var bytes = Convert.FromBase64String(base64);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return null;
        }
    }
}
