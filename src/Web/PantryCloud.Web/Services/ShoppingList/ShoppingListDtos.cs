using System.Text.Json.Serialization;

namespace PantryCloud.Web.Services.ShoppingList;

/// <summary>
/// Request/response DTOs for Shopping List API. Unit and ItemSource are int (backend enums as number). RowVersion is byte[] (JSON base64).
/// </summary>

// Lists
public record CreateShoppingListRequest
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
}

public record CreateShoppingListResponse
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("householdId")] public Guid HouseholdId { get; init; }
    [JsonPropertyName("name")] public string Name { get; init; } = "";
    [JsonPropertyName("createdBy")] public Guid CreatedBy { get; init; }
    [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Result of create list call; signals 409 duplicate name so UI can show specific message.
/// </summary>
public record CreateShoppingListResult
{
    public CreateShoppingListResponse? Value { get; init; }
    public bool IsDuplicateName { get; init; }
}

public record ShoppingListSummaryDto
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("name")] public string Name { get; init; } = "";
    [JsonPropertyName("itemCount")] public int ItemCount { get; init; }
    [JsonPropertyName("checkedItemCount")] public int CheckedItemCount { get; init; }
    [JsonPropertyName("createdBy")] public Guid CreatedBy { get; init; }
    [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; init; }
    [JsonPropertyName("modifiedBy")] public Guid? ModifiedBy { get; init; }
    [JsonPropertyName("modifiedAt")] public DateTime? ModifiedAt { get; init; }
}

public record ListShoppingListsResponse
{
    [JsonPropertyName("lists")] public IReadOnlyList<ShoppingListSummaryDto> Lists { get; init; } = [];
    [JsonPropertyName("totalCount")] public int TotalCount { get; init; }
    [JsonPropertyName("page")] public int Page { get; init; }
    [JsonPropertyName("pageSize")] public int PageSize { get; init; }
}

public record GetShoppingListResponse
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("householdId")] public Guid HouseholdId { get; init; }
    [JsonPropertyName("name")] public string Name { get; init; } = "";
    [JsonPropertyName("createdBy")] public Guid CreatedBy { get; init; }
    [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; init; }
    [JsonPropertyName("modifiedBy")] public Guid? ModifiedBy { get; init; }
    [JsonPropertyName("modifiedAt")] public DateTime? ModifiedAt { get; init; }
    [JsonPropertyName("items")] public IReadOnlyList<ShoppingListItemDto> Items { get; init; } = [];
}

public record ShoppingListItemDto
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("name")] public string Name { get; init; } = "";
    [JsonPropertyName("quantity")] public decimal Quantity { get; init; }
    [JsonPropertyName("unit")] public int Unit { get; init; }
    [JsonPropertyName("isChecked")] public bool IsChecked { get; init; }
    [JsonPropertyName("checkedBy")] public Guid? CheckedBy { get; init; }
    [JsonPropertyName("checkedAt")] public DateTime? CheckedAt { get; init; }
    [JsonPropertyName("source")] public int Source { get; init; }
    [JsonPropertyName("createdBy")] public Guid CreatedBy { get; init; }
    [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; init; }
    [JsonPropertyName("modifiedBy")] public Guid? ModifiedBy { get; init; }
    [JsonPropertyName("modifiedAt")] public DateTime? ModifiedAt { get; init; }
    [JsonPropertyName("rowVersion")] public byte[] RowVersion { get; init; } = [];
}

// List result types (for no-household / unauthorized)
public record ListShoppingListsResult(ListShoppingListsResponse? Data, bool NoHousehold, bool Unauthorized);
public record GetShoppingListResult(GetShoppingListResponse? Data, bool NoHousehold, bool Unauthorized);

// Items
public record AddShoppingListItemRequest
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("quantity")] public decimal Quantity { get; set; } = 1;
    [JsonPropertyName("unit")] public int Unit { get; set; }
    [JsonPropertyName("source")] public int Source { get; set; } = 0; // Manual
}

public record AddShoppingListItemResponse
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("shoppingListId")] public Guid ShoppingListId { get; init; }
    [JsonPropertyName("name")] public string Name { get; init; } = "";
    [JsonPropertyName("quantity")] public decimal Quantity { get; init; }
    [JsonPropertyName("unit")] public int Unit { get; init; }
    [JsonPropertyName("isChecked")] public bool IsChecked { get; init; }
    [JsonPropertyName("source")] public int Source { get; init; }
    [JsonPropertyName("createdBy")] public Guid CreatedBy { get; init; }
    [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; init; }
    [JsonPropertyName("rowVersion")] public byte[] RowVersion { get; init; } = [];
}

public record UpdateShoppingListItemRequest
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("quantity")] public decimal Quantity { get; set; }
    [JsonPropertyName("unit")] public int Unit { get; set; }
    [JsonPropertyName("rowVersion")] public byte[] RowVersion { get; set; } = [];
}

public record UpdateShoppingListItemResponse
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("shoppingListId")] public Guid ShoppingListId { get; init; }
    [JsonPropertyName("name")] public string Name { get; init; } = "";
    [JsonPropertyName("quantity")] public decimal Quantity { get; init; }
    [JsonPropertyName("unit")] public int Unit { get; init; }
    [JsonPropertyName("isChecked")] public bool IsChecked { get; init; }
    [JsonPropertyName("checkedBy")] public Guid? CheckedBy { get; init; }
    [JsonPropertyName("checkedAt")] public DateTime? CheckedAt { get; init; }
    [JsonPropertyName("source")] public int Source { get; init; }
    [JsonPropertyName("createdBy")] public Guid CreatedBy { get; init; }
    [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; init; }
    [JsonPropertyName("modifiedBy")] public Guid? ModifiedBy { get; init; }
    [JsonPropertyName("modifiedAt")] public DateTime? ModifiedAt { get; init; }
    [JsonPropertyName("rowVersion")] public byte[] RowVersion { get; init; } = [];
}

public record CheckShoppingListItemResponse
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("isChecked")] public bool IsChecked { get; init; }
    [JsonPropertyName("checkedBy")] public Guid? CheckedBy { get; init; }
    [JsonPropertyName("checkedAt")] public DateTime? CheckedAt { get; init; }
    [JsonPropertyName("allItemsChecked")] public bool AllItemsChecked { get; init; }
}

/// <summary>
/// Result of update item call; signals 409 concurrency so UI can refresh and show message.
/// </summary>
public record ShoppingListItemUpdateResult
{
    public UpdateShoppingListItemResponse? Value { get; init; }
    public bool IsConcurrencyConflict { get; init; }
}

/// <summary>
/// Result of shopping list item form dialog: either add (EditId=null, RowVersion=null) or edit (EditId and RowVersion set).
/// </summary>
public record ShoppingListItemFormResult(Guid? EditId, string Name, decimal Quantity, int Unit, byte[]? RowVersion);
