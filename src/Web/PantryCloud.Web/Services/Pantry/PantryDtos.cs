using System.Text.Json.Serialization;

namespace PantryCloud.Web.Services.Pantry;

/// <summary>
/// Request/response DTOs for Pantry API. Unit is int (backend enum serialized as number).
/// RowVersion is byte[] (JSON base64).
/// </summary>
public record CreatePantryItemRequest
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("quantity")] public decimal Quantity { get; set; }
    [JsonPropertyName("unit")] public int Unit { get; set; }
    [JsonPropertyName("expirationDate")] public DateTime? ExpirationDate { get; set; }
    [JsonPropertyName("category")] public string? Category { get; set; }
    [JsonPropertyName("notes")] public string? Notes { get; set; }
    [JsonPropertyName("imageUrl")] public string? ImageUrl { get; set; }
}

public record CreatePantryItemResponse
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("householdId")] public Guid HouseholdId { get; init; }
    [JsonPropertyName("name")] public string Name { get; init; } = "";
    [JsonPropertyName("quantity")] public decimal Quantity { get; init; }
    [JsonPropertyName("unit")] public int Unit { get; init; }
    [JsonPropertyName("expirationDate")] public DateTime? ExpirationDate { get; init; }
    [JsonPropertyName("category")] public string? Category { get; init; }
    [JsonPropertyName("notes")] public string? Notes { get; init; }
    [JsonPropertyName("imageUrl")] public string? ImageUrl { get; init; }
    [JsonPropertyName("createdBy")] public Guid CreatedBy { get; init; }
    [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; init; }
    [JsonPropertyName("rowVersion")] public byte[] RowVersion { get; init; } = [];
}

public record UpdatePantryItemRequest
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("quantity")] public decimal Quantity { get; set; }
    [JsonPropertyName("unit")] public int Unit { get; set; }
    [JsonPropertyName("expirationDate")] public DateTime? ExpirationDate { get; set; }
    [JsonPropertyName("category")] public string? Category { get; set; }
    [JsonPropertyName("notes")] public string? Notes { get; set; }
    [JsonPropertyName("imageUrl")] public string? ImageUrl { get; set; }
    [JsonPropertyName("rowVersion")] public byte[] RowVersion { get; set; } = [];
}

public record UpdatePantryItemResponse
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("householdId")] public Guid HouseholdId { get; init; }
    [JsonPropertyName("name")] public string Name { get; init; } = "";
    [JsonPropertyName("quantity")] public decimal Quantity { get; init; }
    [JsonPropertyName("unit")] public int Unit { get; init; }
    [JsonPropertyName("expirationDate")] public DateTime? ExpirationDate { get; init; }
    [JsonPropertyName("category")] public string? Category { get; init; }
    [JsonPropertyName("notes")] public string? Notes { get; init; }
    [JsonPropertyName("imageUrl")] public string? ImageUrl { get; init; }
    [JsonPropertyName("createdBy")] public Guid CreatedBy { get; init; }
    [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; init; }
    [JsonPropertyName("modifiedBy")] public Guid? ModifiedBy { get; init; }
    [JsonPropertyName("modifiedAt")] public DateTime? ModifiedAt { get; init; }
    [JsonPropertyName("rowVersion")] public byte[] RowVersion { get; init; } = [];
}

public record PantryItemDto
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("name")] public string Name { get; init; } = "";
    [JsonPropertyName("quantity")] public decimal Quantity { get; init; }
    [JsonPropertyName("unit")] public int Unit { get; init; }
    [JsonPropertyName("expirationDate")] public DateTime? ExpirationDate { get; init; }
    [JsonPropertyName("category")] public string? Category { get; init; }
    [JsonPropertyName("notes")] public string? Notes { get; init; }
    [JsonPropertyName("imageUrl")] public string? ImageUrl { get; init; }
    [JsonPropertyName("createdBy")] public Guid CreatedBy { get; init; }
    [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; init; }
    [JsonPropertyName("modifiedBy")] public Guid? ModifiedBy { get; init; }
    [JsonPropertyName("modifiedAt")] public DateTime? ModifiedAt { get; init; }
    [JsonPropertyName("rowVersion")] public byte[] RowVersion { get; init; } = [];
}

public record ListPantryItemsResponse
{
    [JsonPropertyName("items")] public IReadOnlyList<PantryItemDto> Items { get; init; } = [];
    [JsonPropertyName("totalCount")] public int TotalCount { get; init; }
    [JsonPropertyName("page")] public int Page { get; init; }
    [JsonPropertyName("pageSize")] public int PageSize { get; init; }
}

/// <summary>
/// Result of an update call; signals 409 concurrency so UI can refresh and show message.
/// </summary>
public record PantryUpdateResult
{
    public UpdatePantryItemResponse? Value { get; init; }
    public bool IsConcurrencyConflict { get; init; }
}

/// <summary>
/// Result of pantry item form dialog: either create (EditId=null, RowVersion=null) or edit (EditId and RowVersion set).
/// </summary>
public record PantryItemFormResult(Guid? EditId, string Name, decimal Quantity, int Unit, DateTime? ExpirationDate, string? Category, string? Notes, byte[]? RowVersion);

/// <summary>
/// Result of list pantry items call. Use to distinguish 404 (no household) from 401 (auth) or other errors.
/// </summary>
public record ListPantryItemsResult(ListPantryItemsResponse? Data, bool NoHousehold, bool Unauthorized);
