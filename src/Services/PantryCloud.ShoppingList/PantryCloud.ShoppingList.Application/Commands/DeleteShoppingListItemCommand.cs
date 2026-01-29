using ErrorOr;
using MediatR;
using PantryCloud.ShoppingList.Application.Dtos;

namespace PantryCloud.ShoppingList.Application.Commands;

public record DeleteShoppingListItemCommand(DeleteShoppingListItemRequestDto Request) : IRequest<ErrorOr<DeleteShoppingListItemResponseDto>>;


