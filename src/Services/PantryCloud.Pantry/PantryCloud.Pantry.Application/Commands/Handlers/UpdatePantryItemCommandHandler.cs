using ErrorOr;
using MediatR;
using PantryCloud.Pantry.Application.Dtos;
using PantryCloud.Pantry.Application.Events;
using PantryCloud.SharedKernel.Correlation;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Pantry.Application.Commands.Handlers;

public class UpdatePantryItemCommandHandler(
    IPantryManagementService pantryManagementService,
    IOutboxWriter outboxWriter,
    ICorrelationIdProvider correlationIdProvider) : IRequestHandler<UpdatePantryItemCommand, ErrorOr<UpdatePantryItemResponseDto>>
{
    public async Task<ErrorOr<UpdatePantryItemResponseDto>> Handle(UpdatePantryItemCommand request, CancellationToken cancellationToken)
    {
        var result = await pantryManagementService.UpdatePantryItemAsync(request.Id, request.Request, cancellationToken);

        if (result.IsError)
            return result;

        await outboxWriter.WriteAsync(new PantryItemUpdatedEvent
        {
            HouseholdId = result.Value.HouseholdId,
            ItemId = result.Value.Id,
            ItemName = result.Value.Name,
            Quantity = result.Value.Quantity,
            UserId = result.Value.ModifiedBy ?? result.Value.CreatedBy,
            CorrelationId = correlationIdProvider.GetCorrelationId()
        }, cancellationToken);

        if (result.Value.Quantity == 0)
        {
            await outboxWriter.WriteAsync(new PantryItemDepletedEvent
            {
                HouseholdId = result.Value.HouseholdId,
                ItemId = result.Value.Id,
                ItemName = result.Value.Name,
                InitiatedByUserId = result.Value.ModifiedBy ?? result.Value.CreatedBy,
                CorrelationId = correlationIdProvider.GetCorrelationId()
            }, cancellationToken);
        }


        return result;
    }
}
