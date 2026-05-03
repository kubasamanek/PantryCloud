using System.Text.Json.Serialization;

namespace PantryCloud.Load.NBomber.Models;

public sealed class RegisterResponse
{
    [JsonPropertyName("userId")]
    public string UserId { get; init; } = string.Empty;

    [JsonPropertyName("verifyEmailToken")]
    public string VerifyEmailToken { get; init; } = string.Empty;
}
