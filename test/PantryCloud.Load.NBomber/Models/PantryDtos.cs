using System.Text.Json.Serialization;

namespace PantryCloud.Load.NBomber.Models;

public sealed class CreatePantryItemRequest
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("quantity")]
    public decimal Quantity { get; init; }

    [JsonPropertyName("unit")]
    public int Unit { get; init; }

    [JsonPropertyName("expirationDate")]
    public DateTime? ExpirationDate { get; init; }

    [JsonPropertyName("category")]
    public string? Category { get; init; }

    [JsonPropertyName("notes")]
    public string? Notes { get; init; }

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; init; }
}
