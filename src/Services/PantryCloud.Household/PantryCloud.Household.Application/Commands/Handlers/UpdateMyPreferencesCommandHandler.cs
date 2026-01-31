using ErrorOr;
using MediatR;
using PantryCloud.Household.Application.Dtos;

namespace PantryCloud.Household.Application.Commands.Handlers;

public class UpdateMyPreferencesCommandHandler(IPreferencesService preferencesService)
    : IRequestHandler<UpdateMyPreferencesCommand, ErrorOr<UpdateMyPreferencesResponseDto>>
{
    public async Task<ErrorOr<UpdateMyPreferencesResponseDto>> Handle(UpdateMyPreferencesCommand request, CancellationToken cancellationToken)
    {
        return await preferencesService.UpdateMyPreferencesAsync(request.Request, cancellationToken);
    }
}
