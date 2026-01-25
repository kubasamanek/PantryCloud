using ErrorOr;
using MediatR;
using PantryCloud.ShoppingList.Application.Dtos;

namespace PantryCloud.ShoppingList.Application.Commands;

public record UpdateShoppingListItemCommand(Guid ListId, Guid ItemId, UpdateShoppingListItemRequestDto Request) : IRequest<ErrorOr<UpdateShoppingListItemResponseDto>>;

