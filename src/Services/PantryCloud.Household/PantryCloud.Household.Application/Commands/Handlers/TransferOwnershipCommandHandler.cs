using ErrorOr;
using MediatR;
using PantryCloud.Household.Application.Dtos;
using PantryCloud.Household.Application.Events;
using PantryCloud.SharedKernel.Correlation;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Household.Application.Commands.Handlers;

public class TransferOwnershipCommandHandler(
    IHouseholdManagementService householdManagementService,
    IMessageBus messageBus,
    ICorrelationIdProvider correlationIdProvider)
    : IRequestHandler<TransferOwnershipCommand, ErrorOr<TransferOwnershipResponseDto>>
{
    public async Task<ErrorOr<TransferOwnershipResponseDto>> Handle(TransferOwnershipCommand request, CancellationToken cancellationToken)
    {
        var result = await householdManagementService.TransferOwnershipAsync(request.Request, cancellationToken);

        return await result.PublishIfSuccessAsync(
            messageBus,
            (response, correlationId) => new OwnershipTransferredEvent
            {
                HouseholdId = response.HouseholdId,
                PreviousOwnerId = response.PreviousOwnerId,
                NewOwnerId = response.NewOwnerId,
                NewOwnerEmail = null,
                TransferredAt = response.TransferredAt,
                CorrelationId = correlationId
            },
            correlationIdProvider,
            cancellationToken);
    }
}
