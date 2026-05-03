using System.Text.Json.Serialization;

namespace PantryCloud.Load.NBomber.Models;

public sealed class CreateHouseholdRequest
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;
}

public sealed class CreateHouseholdResponse
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }
}

public sealed class GetCurrentHouseholdResponse
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;
}
