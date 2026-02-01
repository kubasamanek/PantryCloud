using PantryCloud.SharedKernel.Enums;

namespace PantryCloud.Pantry.Application.Dtos;

public record CreatePantryItemRequestDto(
    string Name,
    decimal Quantity,
    Unit Unit,
    DateTime? ExpirationDate,
    string? Category,
    string? Notes,
    string? ImageUrl);

public record CreatePantryItemResponseDto(
    Guid Id,
    Guid HouseholdId,
    string Name,
    decimal Quantity,
    Unit Unit,
    DateTime? ExpirationDate,
    string? Category,
    string? Notes,
    string? ImageUrl,
    Guid CreatedBy,
    DateTime CreatedAt,
    byte[] RowVersion);

public record UpdatePantryItemRequestDto(
    string Name,
    decimal Quantity,
    Unit Unit,
    DateTime? ExpirationDate,
    string? Category,
    string? Notes,
    string? ImageUrl,
    byte[] RowVersion);

public record UpdatePantryItemResponseDto(
    Guid Id,
    Guid HouseholdId,
    string Name,
    decimal Quantity,
    Unit Unit,
    DateTime? ExpirationDate,
    string? Category,
    string? Notes,
    string? ImageUrl,
    Guid CreatedBy,
    DateTime CreatedAt,
    Guid? ModifiedBy,
    DateTime? ModifiedAt,
    byte[] RowVersion);

public record DeletePantryItemRequestDto(Guid Id);

public record DeletePantryItemResponseDto(Guid Id, Guid HouseholdId, string ItemName, Guid InitiatedByUserId);

public record GetPantryItemRequestDto(Guid Id);

public record GetPantryItemResponseDto(
    Guid Id,
    Guid HouseholdId,
    string Name,
    decimal Quantity,
    Unit Unit,
    DateTime? ExpirationDate,
    string? Category,
    string? Notes,
    string? ImageUrl,
    Guid CreatedBy,
    DateTime CreatedAt,
    Guid? ModifiedBy,
    DateTime? ModifiedAt,
    byte[] RowVersion);

public record ListPantryItemsRequestDto(
    string? Category,
    string? SearchTerm,
    int Page = 1,
    int PageSize = 50);

public record ListPantryItemsResponseDto(
    IReadOnlyList<PantryItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize);

public record PantryItemDto(
    Guid Id,
    string Name,
    decimal Quantity,
    Unit Unit,
    DateTime? ExpirationDate,
    string? Category,
    string? Notes,
    string? ImageUrl,
    Guid CreatedBy,
    DateTime CreatedAt,
    Guid? ModifiedBy,
    DateTime? ModifiedAt,
    byte[] RowVersion);

