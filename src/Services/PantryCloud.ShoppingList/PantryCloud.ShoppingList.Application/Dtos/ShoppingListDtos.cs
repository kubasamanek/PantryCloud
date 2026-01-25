using PantryCloud.SharedKernel.Enums;
using PantryCloud.ShoppingList.Core.Enums;

namespace PantryCloud.ShoppingList.Application.Dtos;

// Shopping List DTOs
public record CreateShoppingListRequestDto(string Name);

public record CreateShoppingListResponseDto(
    Guid Id,
    Guid HouseholdId,
    string Name,
    Guid CreatedBy,
    DateTime CreatedAt);

public record DeleteShoppingListRequestDto(Guid Id);

public record DeleteShoppingListResponseDto(Guid Id);

public record GetShoppingListRequestDto(Guid Id);

public record GetShoppingListResponseDto(
    Guid Id,
    Guid HouseholdId,
    string Name,
    Guid CreatedBy,
    DateTime CreatedAt,
    Guid? ModifiedBy,
    DateTime? ModifiedAt,
    IReadOnlyList<ShoppingListItemDto> Items);

public record ListShoppingListsRequestDto(
    int Page = 1,
    int PageSize = 50);

public record ListShoppingListsResponseDto(
    IReadOnlyList<ShoppingListSummaryDto> Lists,
    int TotalCount,
    int Page,
    int PageSize);

public record ShoppingListSummaryDto(
    Guid Id,
    string Name,
    int ItemCount,
    int CheckedItemCount,
    Guid CreatedBy,
    DateTime CreatedAt,
    Guid? ModifiedBy,
    DateTime? ModifiedAt);

// Shopping List Item DTOs
public record AddShoppingListItemRequestDto(
    string Name,
    decimal Quantity,
    Unit Unit,
    ItemSource Source = ItemSource.Manual);

public record AddShoppingListItemResponseDto(
    Guid Id,
    Guid ShoppingListId,
    string Name,
    decimal Quantity,
    Unit Unit,
    bool IsChecked,
    ItemSource Source,
    Guid CreatedBy,
    DateTime CreatedAt,
    byte[] RowVersion);

public record AddShoppingListItemsBatchRequestDto(
    IReadOnlyList<AddShoppingListItemRequestDto> Items);

public record AddShoppingListItemsBatchResponseDto(
    IReadOnlyList<AddShoppingListItemResponseDto> Items);

public record UpdateShoppingListItemRequestDto(
    string Name,
    decimal Quantity,
    Unit Unit,
    byte[] RowVersion);

public record UpdateShoppingListItemResponseDto(
    Guid Id,
    Guid ShoppingListId,
    string Name,
    decimal Quantity,
    Unit Unit,
    bool IsChecked,
    Guid? CheckedBy,
    DateTime? CheckedAt,
    ItemSource Source,
    Guid CreatedBy,
    DateTime CreatedAt,
    Guid? ModifiedBy,
    DateTime? ModifiedAt,
    byte[] RowVersion);

public record DeleteShoppingListItemRequestDto(Guid ListId, Guid ItemId);

public record DeleteShoppingListItemResponseDto(Guid ItemId);

public record CheckShoppingListItemRequestDto(Guid ListId, Guid ItemId);

public record CheckShoppingListItemResponseDto(
    Guid Id,
    bool IsChecked,
    Guid? CheckedBy,
    DateTime? CheckedAt);

public record ShoppingListItemDto(
    Guid Id,
    string Name,
    decimal Quantity,
    Unit Unit,
    bool IsChecked,
    Guid? CheckedBy,
    DateTime? CheckedAt,
    ItemSource Source,
    Guid CreatedBy,
    DateTime CreatedAt,
    Guid? ModifiedBy,
    DateTime? ModifiedAt,
    byte[] RowVersion);

