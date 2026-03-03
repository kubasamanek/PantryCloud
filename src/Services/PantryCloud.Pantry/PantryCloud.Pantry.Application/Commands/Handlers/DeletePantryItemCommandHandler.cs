using ErrorOr;
using MediatR;
using PantryCloud.Pantry.Application.Dtos;
using PantryCloud.Pantry.Application.Events;
using PantryCloud.SharedKernel.Correlation;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Pantry.Application.Commands.Handlers;

public class DeletePantryItemCommandHandler(
    IPantryManagementService pantryManagementService,
    IOutboxWriter outboxWriter,
    ICorrelationIdProvider correlationIdProvider) : IRequestHandler<DeletePantryItemCommand, ErrorOr<DeletePantryItemResponseDto>>
{
    public async Task<ErrorOr<DeletePantryItemResponseDto>> Handle(DeletePantryItemCommand request, CancellationToken cancellationToken)
    {
        var result = await pantryManagementService.DeletePantryItemAsync(request.Request, cancellationToken);

        return await result.WriteToOutboxIfSuccessAsync(
            outboxWriter,
            (response, correlationId) => new PantryItemDeletedEvent
            {
                HouseholdId = response.HouseholdId,
                ItemId = response.Id,
                ItemName = response.ItemName,
                InitiatedByUserId = response.InitiatedByUserId,
                CorrelationId = correlationId
            },
            correlationIdProvider,
            cancellationToken);
    }
}
