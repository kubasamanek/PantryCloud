using ErrorOr;
using MediatR;
using PantryCloud.ShoppingList.Application.Dtos;

namespace PantryCloud.ShoppingList.Application.Commands;

public record CheckShoppingListItemCommand(CheckShoppingListItemRequestDto Request) : IRequest<ErrorOr<CheckShoppingListItemResponseDto>>;

