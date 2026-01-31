using ErrorOr;
using MediatR;
using PantryCloud.SharedKernel.Correlation;
using PantryCloud.SharedKernel.Messaging;
using PantryCloud.ShoppingList.Application.Dtos;
using PantryCloud.ShoppingList.Application.Events;

namespace PantryCloud.ShoppingList.Application.Commands.Handlers;

public class CreateShoppingListCommandHandler(
    IShoppingListManagementService shoppingListManagementService,
    IMessageBus messageBus,
    ICorrelationIdProvider correlationIdProvider)
    : IRequestHandler<CreateShoppingListCommand, ErrorOr<CreateShoppingListResponseDto>>
{
    public async Task<ErrorOr<CreateShoppingListResponseDto>> Handle(CreateShoppingListCommand request, CancellationToken cancellationToken)
    {
        var result = await shoppingListManagementService.CreateShoppingListAsync(request.Request, cancellationToken);

        if (result.IsError)
            return result;

        await messageBus.PublishAsync(new ShoppingListCreatedEvent
        {
            HouseholdId = result.Value.HouseholdId,
            ShoppingListId = result.Value.Id,
            ShoppingListName = result.Value.Name,
            CreatedByUserId = result.Value.CreatedBy,
            CorrelationId = correlationIdProvider.GetCorrelationId()
        }, cancellationToken);

        return result;
    }
}


