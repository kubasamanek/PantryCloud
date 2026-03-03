using ErrorOr;
using MediatR;
using PantryCloud.Household.Application.Dtos;
using PantryCloud.Household.Application.Events;
using PantryCloud.SharedKernel.Correlation;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Household.Application.Commands.Handlers;

public class UpdateMyPreferencesCommandHandler(
    IPreferencesService preferencesService,
    IOutboxWriter outboxWriter,
    ICorrelationIdProvider correlationIdProvider)
    : IRequestHandler<UpdateMyPreferencesCommand, ErrorOr<UpdateMyPreferencesResponseDto>>
{
    public async Task<ErrorOr<UpdateMyPreferencesResponseDto>> Handle(UpdateMyPreferencesCommand request, CancellationToken cancellationToken)
    {
        var result = await preferencesService.UpdateMyPreferencesAsync(request.Request, cancellationToken);

        return await result.WriteToOutboxIfSuccessAsync(
            outboxWriter,
            (response, correlationId) => new PreferenceAddedEvent
            {
                HouseholdId = response.HouseholdId,
                UserId = response.UserId,
                CorrelationId = correlationId
            },
            correlationIdProvider,
            cancellationToken);
    }
}
