using ErrorOr;
using MediatR;
using PantryCloud.SharedKernel.Correlation;
using PantryCloud.SharedKernel.Messaging;
using PantryCloud.ShoppingList.Application.Dtos;
using PantryCloud.ShoppingList.Application.Events;

namespace PantryCloud.ShoppingList.Application.Commands.Handlers;

public class CheckShoppingListItemCommandHandler(
    IShoppingListManagementService shoppingListManagementService,
    IMessageBus messageBus,
    ICorrelationIdProvider correlationIdProvider)
    : IRequestHandler<CheckShoppingListItemCommand, ErrorOr<CheckShoppingListItemResponseDto>>
{
    public async Task<ErrorOr<CheckShoppingListItemResponseDto>> Handle(CheckShoppingListItemCommand request, CancellationToken cancellationToken)
    {
        var result = await shoppingListManagementService.CheckShoppingListItemAsync(request.Request, cancellationToken);

        if (result.IsError)
            return result;

        if (result.Value.AllItemsChecked && result.Value.HouseholdId.HasValue && result.Value.ShoppingListId.HasValue && result.Value.ShoppingListName != null && result.Value.CheckedByUserId.HasValue)
        {
            await messageBus.PublishAsync(new ShoppingListAllItemsCheckedEvent
            {
                HouseholdId = result.Value.HouseholdId.Value,
                ShoppingListId = result.Value.ShoppingListId.Value,
                ShoppingListName = result.Value.ShoppingListName,
                CheckedByUserId = result.Value.CheckedByUserId.Value,
                CorrelationId = correlationIdProvider.GetCorrelationId()
            }, cancellationToken);
        }

        return result;
    }
}


