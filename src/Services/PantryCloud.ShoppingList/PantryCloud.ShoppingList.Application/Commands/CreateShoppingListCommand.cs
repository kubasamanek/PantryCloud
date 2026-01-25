using ErrorOr;
using MediatR;
using PantryCloud.ShoppingList.Application.Dtos;

namespace PantryCloud.ShoppingList.Application.Commands;

public record CreateShoppingListCommand(CreateShoppingListRequestDto Request) : IRequest<ErrorOr<CreateShoppingListResponseDto>>;

