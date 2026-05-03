using System.Text.Json.Serialization;

namespace PantryCloud.Load.NBomber.Models;

public sealed class LoginResponse
{
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; init; } = string.Empty;

    [JsonPropertyName("refreshToken")]
    public string RefreshToken { get; init; } = string.Empty;
}
