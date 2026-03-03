using ErrorOr;
using MediatR;
using PantryCloud.SharedKernel.Correlation;
using PantryCloud.SharedKernel.Messaging;
using PantryCloud.ShoppingList.Application.Dtos;
using PantryCloud.ShoppingList.Application.Events;

namespace PantryCloud.ShoppingList.Application.Commands.Handlers;

public class CreateShoppingListCommandHandler(
    IShoppingListManagementService shoppingListManagementService,
    IOutboxWriter outboxWriter,
    ICorrelationIdProvider correlationIdProvider)
    : IRequestHandler<CreateShoppingListCommand, ErrorOr<CreateShoppingListResponseDto>>
{
    public async Task<ErrorOr<CreateShoppingListResponseDto>> Handle(CreateShoppingListCommand request, CancellationToken cancellationToken)
    {
        var result = await shoppingListManagementService.CreateShoppingListAsync(request.Request, cancellationToken);

        return await result.WriteToOutboxIfSuccessAsync(
            outboxWriter,
            (response, correlationId) => new ShoppingListCreatedEvent
            {
                HouseholdId = response.HouseholdId,
                ShoppingListId = response.Id,
                ShoppingListName = response.Name,
                CreatedByUserId = response.CreatedBy,
                CorrelationId = correlationId
            },
            correlationIdProvider,
            cancellationToken);
    }
}
