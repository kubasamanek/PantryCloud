using ErrorOr;
using MediatR;
using PantryCloud.Household.Application.Dtos;

namespace PantryCloud.Household.Application.Queries.Handlers;

public class GetMyProfileQueryHandler(IProfileService profileService)
    : IRequestHandler<GetMyProfileQuery, ErrorOr<GetMyProfileResponseDto>>
{
    public async Task<ErrorOr<GetMyProfileResponseDto>> Handle(
        GetMyProfileQuery request,
        CancellationToken cancellationToken)
    {
        return await profileService.GetMyProfileAsync(cancellationToken);
    }
}
