using ErrorOr;
using MediatR;
using PantryCloud.Household.Application.Dtos;

namespace PantryCloud.Household.Application.Commands.Handlers;

public class UpdateMyProfileCommandHandler(IProfileService profileService)
    : IRequestHandler<UpdateMyProfileCommand, ErrorOr<UpdateMyProfileResponseDto>>
{
    public async Task<ErrorOr<UpdateMyProfileResponseDto>> Handle(
        UpdateMyProfileCommand request,
        CancellationToken cancellationToken)
    {
        return await profileService.UpdateMyProfileAsync(request.Request, cancellationToken);
    }
}
