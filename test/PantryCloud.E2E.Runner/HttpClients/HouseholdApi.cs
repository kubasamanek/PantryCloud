using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using PantryCloud.E2E.Runner.Configuration;
using PantryCloud.Household.Application.Dtos;

namespace PantryCloud.E2E.Runner.HttpClients;

public sealed class HouseholdApi(HttpClient client, E2ESettings settings)
{
    private string VersionedBasePath() =>
        $"/api/v{settings.Gateway.ApiVersion}/household/households";

    private void SetAuthHeader(string accessToken)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);
    }

    public async Task<Guid> CreateHouseholdAsync(
        string accessToken,
        string name,
        CancellationToken cancellationToken)
    {
        SetAuthHeader(accessToken);

        var request = new CreateHouseholdRequestDto(name);

        using var response = await client.PostAsJsonAsync(
            VersionedBasePath(),
            request,
            cancellationToken);

        if (response.StatusCode != HttpStatusCode.Created)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"CreateHousehold failed with status {(int)response.StatusCode}: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<CreateHouseholdResponseDto>(cancellationToken: cancellationToken);
        if (result is null || result.Id == Guid.Empty)
        {
            throw new InvalidOperationException("CreateHousehold response is missing household id.");
        }

        return result.Id;
    }

    public async Task<string> InviteMemberAsync(
        string accessToken,
        Guid householdId,
        string inviteeEmail,
        CancellationToken cancellationToken)
    {
        SetAuthHeader(accessToken);

        var request = new SendHouseholdInvitationRequestDto(inviteeEmail, householdId.ToString());

        using var response = await client.PostAsJsonAsync(
            $"{VersionedBasePath()}/invite",
            request,
            cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"InviteMember failed with status {(int)response.StatusCode}: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<SendHouseholdInvitationResponseDto>(cancellationToken: cancellationToken);
        if (result is null || string.IsNullOrWhiteSpace(result.Code))
        {
            throw new InvalidOperationException("InviteMember response is missing invitation code.");
        }

        return result.Code;
    }

    public async Task AcceptInviteAsync(
        string accessToken,
        string code,
        CancellationToken cancellationToken)
    {
        SetAuthHeader(accessToken);

        var request = new AcceptHouseholdInvitationRequestDto(code);

        using var response = await client.PostAsJsonAsync(
            $"{VersionedBasePath()}/join",
            request,
            cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"AcceptInvite failed with status {(int)response.StatusCode}: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<AcceptHouseholdInvitationResponseDto>(cancellationToken: cancellationToken);
        if (result is null || result.HouseholdId is null || result.MemberId is null)
        {
            throw new InvalidOperationException("AcceptInvite response is missing household or member id.");
        }
    }

    public async Task<GetCurrentHouseholdResponseDto?> GetCurrentHouseholdAsync(
        string accessToken,
        CancellationToken cancellationToken)
    {
        SetAuthHeader(accessToken);

        using var response = await client.GetAsync(
            $"{VersionedBasePath()}/me",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"GetCurrentHousehold failed with status {(int)response.StatusCode}: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<GetCurrentHouseholdResponseDto>(cancellationToken: cancellationToken);
        if (result is null || result.Id == Guid.Empty)
        {
            throw new InvalidOperationException("GetCurrentHousehold response is missing household id.");
        }

        return result;
    }

    public async Task<TransferOwnershipResponseDto> TransferOwnershipAsync(
        string accessToken,
        Guid newOwnerUserId,
        CancellationToken cancellationToken)
    {
        SetAuthHeader(accessToken);

        var request = new TransferOwnershipRequestDto(newOwnerUserId);

        using var response = await client.PostAsJsonAsync(
            $"{VersionedBasePath()}/transfer-ownership",
            request,
            cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"TransferOwnership failed with status {(int)response.StatusCode}: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<TransferOwnershipResponseDto>(cancellationToken: cancellationToken);
        if (result is null || result.HouseholdId == Guid.Empty || result.NewOwnerId == Guid.Empty)
        {
            throw new InvalidOperationException("TransferOwnership response is missing household or owner ids.");
        }

        return result;
    }

    public async Task<LeaveHouseholdResponseDto> LeaveHouseholdAsync(
        string accessToken,
        CancellationToken cancellationToken)
    {
        SetAuthHeader(accessToken);

        var request = new LeaveHouseholdRequestDto();

        using var response = await client.PostAsJsonAsync(
            $"{VersionedBasePath()}/leave",
            request,
            cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"LeaveHousehold failed with status {(int)response.StatusCode}: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<LeaveHouseholdResponseDto>(cancellationToken: cancellationToken);
        if (result is null)
        {
            throw new InvalidOperationException("LeaveHousehold response is missing body.");
        }

        return result;
    }
}

