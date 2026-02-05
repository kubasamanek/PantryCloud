using ErrorOr;
using MediatR;
using PantryCloud.Pantry.Application.Dtos;
using PantryCloud.Pantry.Application.Events;
using PantryCloud.SharedKernel.Correlation;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Pantry.Application.Commands.Handlers;

public class CreatePantryItemCommandHandler(
    IPantryManagementService pantryManagementService,
    IMessageBus messageBus,
    ICorrelationIdProvider correlationIdProvider) : IRequestHandler<CreatePantryItemCommand, ErrorOr<CreatePantryItemResponseDto>>
{
    public async Task<ErrorOr<CreatePantryItemResponseDto>> Handle(CreatePantryItemCommand request, CancellationToken cancellationToken)
    {
        var result = await pantryManagementService.CreatePantryItemAsync(request.Request, cancellationToken);

        if (result.IsError)
            return result;

        await messageBus.PublishAsync(new PantryItemCreatedEvent
        {
            HouseholdId = result.Value.HouseholdId,
            ItemId = result.Value.Id,
            ItemName = result.Value.Name,
            Quantity = result.Value.Quantity,
            UserId = result.Value.CreatedBy,
            CorrelationId = correlationIdProvider.GetCorrelationId()
        }, cancellationToken);

        return result;
    }
}

