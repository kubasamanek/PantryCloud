using System.Text.Json.Serialization;

namespace PantryCloud.Load.NBomber.Models;

public sealed class VerifyEmailRequest
{
    [JsonPropertyName("email")]
    public string Email { get; init; } = string.Empty;

    [JsonPropertyName("token")]
    public string Token { get; init; } = string.Empty;
}
