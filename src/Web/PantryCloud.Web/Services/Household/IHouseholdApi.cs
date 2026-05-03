namespace PantryCloud.Web.Services.Household;

public interface IHouseholdApi
{
    Task<GetCurrentHouseholdResponse?> GetCurrentHouseholdAsync(CancellationToken cancellationToken = default);
    Task<CreateHouseholdResponse?> CreateHouseholdAsync(CreateHouseholdRequest request, CancellationToken cancellationToken = default);
    Task<GetHouseholdMembersResponse?> GetHouseholdMembersAsync(CancellationToken cancellationToken = default);
    Task<SendInvitationResponse?> SendInvitationAsync(string toEmail, string householdId, CancellationToken cancellationToken = default);
    Task<AcceptInvitationResponse?> AcceptInvitationAsync(string code, CancellationToken cancellationToken = default);
    Task<LeaveHouseholdResponse?> LeaveHouseholdAsync(CancellationToken cancellationToken = default);
    Task<bool> KickMemberAsync(Guid memberUserId, CancellationToken cancellationToken = default);
    Task<TransferOwnershipResponse?> TransferOwnershipAsync(Guid newOwnerUserId, CancellationToken cancellationToken = default);
    Task<GetMyPreferencesResponse?> GetMyPreferencesAsync(CancellationToken cancellationToken = default);
    Task<UpdateMyPreferencesResponse?> UpdateMyPreferencesAsync(UpdateMyPreferencesRequest request, CancellationToken cancellationToken = default);
}
