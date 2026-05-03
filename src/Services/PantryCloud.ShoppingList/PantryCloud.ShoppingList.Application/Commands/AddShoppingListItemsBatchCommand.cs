using ErrorOr;
using MediatR;
using PantryCloud.ShoppingList.Application.Dtos;

namespace PantryCloud.ShoppingList.Application.Commands;

public record AddShoppingListItemsBatchCommand(Guid ListId, AddShoppingListItemsBatchRequestDto Request) : IRequest<ErrorOr<AddShoppingListItemsBatchResponseDto>>;


