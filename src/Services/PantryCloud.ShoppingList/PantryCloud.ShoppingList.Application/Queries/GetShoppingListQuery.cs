using ErrorOr;
using MediatR;
using PantryCloud.ShoppingList.Application.Dtos;

namespace PantryCloud.ShoppingList.Application.Queries;

public record GetShoppingListQuery(GetShoppingListRequestDto Request) : IRequest<ErrorOr<GetShoppingListResponseDto>>;


