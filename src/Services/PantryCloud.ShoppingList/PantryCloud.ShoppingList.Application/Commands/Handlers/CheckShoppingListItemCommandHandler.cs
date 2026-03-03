using ErrorOr;
using MediatR;
using PantryCloud.SharedKernel.Correlation;
using PantryCloud.SharedKernel.Messaging;
using PantryCloud.ShoppingList.Application.Dtos;
using PantryCloud.ShoppingList.Application.Events;

namespace PantryCloud.ShoppingList.Application.Commands.Handlers;

public class CheckShoppingListItemCommandHandler(
    IShoppingListManagementService shoppingListManagementService,
    IOutboxWriter outboxWriter,
    ICorrelationIdProvider correlationIdProvider)
    : IRequestHandler<CheckShoppingListItemCommand, ErrorOr<CheckShoppingListItemResponseDto>>
{
    public async Task<ErrorOr<CheckShoppingListItemResponseDto>> Handle(CheckShoppingListItemCommand request, CancellationToken cancellationToken)
    {
        var result = await shoppingListManagementService.CheckShoppingListItemAsync(request.Request, cancellationToken);

        if (result.IsError)
            return result;

        // All items in a shopping list checked
        if (result.Value is { AllItemsChecked: true, HouseholdId: not null, ShoppingListId: not null, ShoppingListName: not null, CheckedByUserId: not null })
        {
            await outboxWriter.WriteAsync(new ShoppingListAllItemsCheckedEvent
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
