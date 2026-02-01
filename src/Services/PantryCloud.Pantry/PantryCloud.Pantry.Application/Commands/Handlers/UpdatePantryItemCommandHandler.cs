using ErrorOr;
using MediatR;
using PantryCloud.Pantry.Application.Dtos;
using PantryCloud.Pantry.Application.Events;
using PantryCloud.SharedKernel.Correlation;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Pantry.Application.Commands.Handlers;

public class UpdatePantryItemCommandHandler(
    IPantryManagementService pantryManagementService,
    IMessageBus messageBus,
    ICorrelationIdProvider correlationIdProvider) : IRequestHandler<UpdatePantryItemCommand, ErrorOr<UpdatePantryItemResponseDto>>
{
    public async Task<ErrorOr<UpdatePantryItemResponseDto>> Handle(UpdatePantryItemCommand request, CancellationToken cancellationToken)
    {
        var result = await pantryManagementService.UpdatePantryItemAsync(request.Id, request.Request, cancellationToken);

        if (result.IsError)
            return result;

        if (result.Value.Quantity == 0)
        {
            await messageBus.PublishAsync(new PantryItemDepletedEvent
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

