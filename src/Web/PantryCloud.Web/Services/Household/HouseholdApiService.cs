using System.Net.Http.Json;
using System.Text.Json;

namespace PantryCloud.Web.Services.Household;

public class HouseholdApiService(IHttpClientFactory httpClientFactory) : IHouseholdApi
{
    private const string BasePath = "api/household/api/households";

    private static readonly JsonSerializerOptions MembersJsonOptions = new()
    {
        Converters = { new HouseholdRoleJsonConverter() }
    };

    private HttpClient Client => httpClientFactory.CreateClient("Gateway");

    public async Task<GetCurrentHouseholdResponse?> GetCurrentHouseholdAsync(CancellationToken cancellationToken = default)
    {
        var response = await Client.GetAsync($"{BasePath}/me", cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<GetCurrentHouseholdResponse>(cancellationToken)
            : null;
    }

    public async Task<CreateHouseholdResponse?> CreateHouseholdAsync(CreateHouseholdRequest request, CancellationToken cancellationToken = default)
    {
        var response = await Client.PostAsJsonAsync(BasePath, request, cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<CreateHouseholdResponse>(cancellationToken)
            : null;
    }

    public async Task<GetHouseholdMembersResponse?> GetHouseholdMembersAsync(CancellationToken cancellationToken = default)
    {
        var response = await Client.GetAsync($"{BasePath}/me/members/preferences", cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<GetHouseholdMembersResponse>(MembersJsonOptions, cancellationToken)
            : null;
    }

    public async Task<SendInvitationResponse?> SendInvitationAsync(string toEmail, string householdId, CancellationToken cancellationToken = default)
    {
        var request = new SendInvitationRequest(toEmail, householdId);
        var response = await Client.PostAsJsonAsync($"{BasePath}/invite", request, cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<SendInvitationResponse>(cancellationToken)
            : null;
    }

    public async Task<AcceptInvitationResponse?> AcceptInvitationAsync(string code, CancellationToken cancellationToken = default)
    {
        var request = new AcceptInvitationRequest(code);
        var response = await Client.PostAsJsonAsync($"{BasePath}/join", request, cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<AcceptInvitationResponse>(cancellationToken)
            : null;
    }

    public async Task<LeaveHouseholdResponse?> LeaveHouseholdAsync(CancellationToken cancellationToken = default)
    {
        var response = await Client.PostAsJsonAsync<object>($"{BasePath}/leave", new { }, cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<LeaveHouseholdResponse>(cancellationToken)
            : null;
    }

    public async Task<bool> KickMemberAsync(Guid memberUserId, CancellationToken cancellationToken = default)
    {
        var response = await Client.DeleteAsync($"{BasePath}/members/{memberUserId}", cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<TransferOwnershipResponse?> TransferOwnershipAsync(Guid newOwnerUserId, CancellationToken cancellationToken = default)
    {
        var request = new TransferOwnershipRequest(newOwnerUserId);
        var response = await Client.PostAsJsonAsync($"{BasePath}/transfer-ownership", request, cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<TransferOwnershipResponse>(cancellationToken)
            : null;
    }

    public async Task<GetMyPreferencesResponse?> GetMyPreferencesAsync(CancellationToken cancellationToken = default)
    {
        var response = await Client.GetAsync($"{BasePath}/me/preferences", cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<GetMyPreferencesResponse>(cancellationToken)
            : null;
    }

    public async Task<UpdateMyPreferencesResponse?> UpdateMyPreferencesAsync(UpdateMyPreferencesRequest request, CancellationToken cancellationToken = default)
    {
        var response = await Client.PutAsJsonAsync($"{BasePath}/me/preferences", request, cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<UpdateMyPreferencesResponse>(cancellationToken)
            : null;
    }
}
