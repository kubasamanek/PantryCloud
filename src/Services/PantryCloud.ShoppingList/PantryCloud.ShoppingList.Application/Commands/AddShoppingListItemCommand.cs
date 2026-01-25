using ErrorOr;
using MediatR;
using PantryCloud.ShoppingList.Application.Dtos;

namespace PantryCloud.ShoppingList.Application.Commands;

public record AddShoppingListItemCommand(Guid ListId, AddShoppingListItemRequestDto Request) : IRequest<ErrorOr<AddShoppingListItemResponseDto>>;

