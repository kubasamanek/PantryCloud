using ErrorOr;
using MediatR;
using PantryCloud.Pantry.Application.Dtos;
using PantryCloud.Pantry.Application.Events;
using PantryCloud.SharedKernel.Correlation;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Pantry.Application.Commands.Handlers;

public class DeletePantryItemCommandHandler(
    IPantryManagementService pantryManagementService,
    IMessageBus messageBus,
    ICorrelationIdProvider correlationIdProvider) : IRequestHandler<DeletePantryItemCommand, ErrorOr<DeletePantryItemResponseDto>>
{
    public async Task<ErrorOr<DeletePantryItemResponseDto>> Handle(DeletePantryItemCommand request, CancellationToken cancellationToken)
    {
        var result = await pantryManagementService.DeletePantryItemAsync(request.Request, cancellationToken);

        if (result.IsError)
            return result;

        await messageBus.PublishAsync(new PantryItemDepletedEvent
        {
            HouseholdId = result.Value.HouseholdId,
            ItemId = result.Value.Id,
            ItemName = result.Value.ItemName,
            InitiatedByUserId = result.Value.InitiatedByUserId,
            CorrelationId = correlationIdProvider.GetCorrelationId()
        }, cancellationToken);

        return result;
    }
}

