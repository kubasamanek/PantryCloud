using ErrorOr;
using MediatR;
using PantryCloud.ShoppingList.Application.Dtos;

namespace PantryCloud.ShoppingList.Application.Queries;

public record ListShoppingListsQuery(ListShoppingListsRequestDto Request) : IRequest<ErrorOr<ListShoppingListsResponseDto>>;


