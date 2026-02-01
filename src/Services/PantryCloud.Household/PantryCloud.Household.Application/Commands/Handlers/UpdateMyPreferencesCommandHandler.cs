using ErrorOr;
using MediatR;
using PantryCloud.Household.Application.Dtos;
using PantryCloud.Household.Application.Events;
using PantryCloud.SharedKernel.Correlation;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Household.Application.Commands.Handlers;

public class UpdateMyPreferencesCommandHandler(
    IPreferencesService preferencesService,
    IMessageBus messageBus,
    ICorrelationIdProvider correlationIdProvider)
    : IRequestHandler<UpdateMyPreferencesCommand, ErrorOr<UpdateMyPreferencesResponseDto>>
{
    public async Task<ErrorOr<UpdateMyPreferencesResponseDto>> Handle(UpdateMyPreferencesCommand request, CancellationToken cancellationToken)
    {
        var result = await preferencesService.UpdateMyPreferencesAsync(request.Request, cancellationToken);

        if (result.IsError)
            return result;

        await messageBus.PublishAsync(new PreferenceAddedEvent
        {
            HouseholdId = result.Value.HouseholdId,
            UserId = result.Value.UserId,
            CorrelationId = correlationIdProvider.GetCorrelationId()
        }, cancellationToken);

        return result;
    }
}
