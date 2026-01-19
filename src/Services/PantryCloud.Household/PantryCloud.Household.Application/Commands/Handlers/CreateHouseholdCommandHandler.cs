using ErrorOr;
using MediatR;
using PantryCloud.Household.Application.Dtos;
using PantryCloud.Household.Application.Events;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Household.Application.Commands.Handlers;

public class CreateHouseholdCommandHandler(
    IHouseholdManagementService householdManagementService,
    IMessageBus messageBus) 
    : IRequestHandler<CreateHouseholdCommand, ErrorOr<CreateHouseholdResponseDto>>
{
    public async Task<ErrorOr<CreateHouseholdResponseDto>> Handle(CreateHouseholdCommand request, CancellationToken cancellationToken)
    {
        var result = await householdManagementService.CreateHousehold(request.Request, cancellationToken);

        return await result.PublishIfSuccessAsync(
            messageBus,
            response => new MemberJoinedHouseholdEvent
            {
                HouseholdId = response.Id,
                NewMemberId = response.OwnerId,
                MemberEmail = response.OwnerEmail,
                JoinedAt = response.CreatedAt
            },
            cancellationToken);
    }
}