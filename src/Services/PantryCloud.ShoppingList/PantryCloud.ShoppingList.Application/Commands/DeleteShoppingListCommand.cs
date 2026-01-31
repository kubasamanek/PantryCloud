using ErrorOr;
using MediatR;
using PantryCloud.ShoppingList.Application.Dtos;

namespace PantryCloud.ShoppingList.Application.Commands;

public record DeleteShoppingListCommand(DeleteShoppingListRequestDto Request) : IRequest<ErrorOr<DeleteShoppingListResponseDto>>;


