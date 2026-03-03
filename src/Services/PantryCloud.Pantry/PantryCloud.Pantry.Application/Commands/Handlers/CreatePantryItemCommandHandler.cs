using ErrorOr;
using MediatR;
using PantryCloud.Pantry.Application.Dtos;
using PantryCloud.Pantry.Application.Events;
using PantryCloud.SharedKernel.Correlation;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Pantry.Application.Commands.Handlers;

public class CreatePantryItemCommandHandler(
    IPantryManagementService pantryManagementService,
    IOutboxWriter outboxWriter,
    ICorrelationIdProvider correlationIdProvider) : IRequestHandler<CreatePantryItemCommand, ErrorOr<CreatePantryItemResponseDto>>
{
    public async Task<ErrorOr<CreatePantryItemResponseDto>> Handle(CreatePantryItemCommand request, CancellationToken cancellationToken)
    {
        var result = await pantryManagementService.CreatePantryItemAsync(request.Request, cancellationToken);

        return await result.WriteToOutboxIfSuccessAsync(
            outboxWriter,
            (response, correlationId) => new PantryItemCreatedEvent
            {
                HouseholdId = response.HouseholdId,
                ItemId = response.Id,
                ItemName = response.Name,
                Quantity = response.Quantity,
                UserId = response.CreatedBy,
                CorrelationId = correlationId
            },
            correlationIdProvider,
            cancellationToken);
    }
}
