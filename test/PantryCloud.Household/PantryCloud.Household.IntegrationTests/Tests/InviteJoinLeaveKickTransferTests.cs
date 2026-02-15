using System.Net;
using System.Net.Http.Json;
using PantryCloud.Household.Application.Dtos;
using PantryCloud.Household.IntegrationTests.Constants;
using PantryCloud.Household.IntegrationTests.Infrastructure;
using Shouldly;

namespace PantryCloud.Household.IntegrationTests.Tests;

public class InviteJoinLeaveKickTransferTests(HouseholdTestFixture fixture) : BaseIntegrationTest(fixture)
{
    [Fact]
    public async Task SendInvite_Returns200_WithCode_WhenOwnerInvites()
    {
        await ResetAsync();
        var ownerId = Guid.NewGuid();
        var client = CreateClientWithToken(ownerId);

        var createResponse = await client.PostAsJsonAsync(TestConstants.Endpoints.CreateHousehold, new CreateHouseholdRequestDto(TestConstants.TestData.HouseholdName));
        createResponse.EnsureSuccessStatusCode();
        var created = await GetFromJsonAsync<CreateHouseholdResponseDto>(createResponse);
        created.ShouldNotBeNull();

        var inviteRequest = new SendHouseholdInvitationRequestDto(TestConstants.TestData.InviteeEmail, created.Id.ToString());
        var inviteResponse = await client.PostAsJsonAsync(TestConstants.Endpoints.SendInvite, inviteRequest);

        inviteResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var inviteBody = await GetFromJsonAsync<SendHouseholdInvitationResponseDto>(inviteResponse);
        inviteBody.ShouldNotBeNull();
        inviteBody.Code.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Join_Returns200_WithHousehold_WhenValidCode()
    {
        await ResetAsync();
        var ownerId = Guid.NewGuid();
        var joinerId = Guid.NewGuid();
        var ownerClient = CreateClientWithToken(ownerId);
        var joinerClient = CreateClientWithToken(joinerId, TestConstants.TestData.InviteeEmail);

        var createResponse = await ownerClient.PostAsJsonAsync(TestConstants.Endpoints.CreateHousehold, new CreateHouseholdRequestDto(TestConstants.TestData.HouseholdName));
        createResponse.EnsureSuccessStatusCode();
        var created = await GetFromJsonAsync<CreateHouseholdResponseDto>(createResponse);
        created.ShouldNotBeNull();

        var inviteResponse = await ownerClient.PostAsJsonAsync(TestConstants.Endpoints.SendInvite,
            new SendHouseholdInvitationRequestDto(TestConstants.TestData.InviteeEmail, created.Id.ToString()));
        inviteResponse.EnsureSuccessStatusCode();
        var inviteBody = await GetFromJsonAsync<SendHouseholdInvitationResponseDto>(inviteResponse);
        inviteBody.ShouldNotBeNull();

        var joinResponse = await joinerClient.PostAsJsonAsync(TestConstants.Endpoints.Join, new AcceptHouseholdInvitationRequestDto(inviteBody.Code));

        joinResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var joinBody = await GetFromJsonAsync<AcceptHouseholdInvitationResponseDto>(joinResponse);
        joinBody.ShouldNotBeNull();
        joinBody.HouseholdId.ShouldBe(created.Id);
        joinBody.MemberId.ShouldBe(joinerId);

        var getCurrent = await joinerClient.GetAsync(TestConstants.Endpoints.GetCurrentHousehold);
        getCurrent.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Leave_Returns200_WhenMemberLeaves()
    {
        await ResetAsync();
        var ownerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var ownerClient = CreateClientWithToken(ownerId);
        var memberClient = CreateClientWithToken(memberId, TestConstants.TestData.InviteeEmail);

        var householdId = await CreateHouseholdAndInviteMemberAsync(ownerClient, memberClient);

        var leaveResponse = await memberClient.PostAsJsonAsync(TestConstants.Endpoints.Leave, new LeaveHouseholdRequestDto());

        leaveResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var leaveBody = await GetFromJsonAsync<LeaveHouseholdResponseDto>(leaveResponse);
        leaveBody.ShouldNotBeNull();
        leaveBody.HouseholdId.ShouldBe(householdId);
        leaveBody.MemberId.ShouldBe(memberId);

        var getCurrent = await memberClient.GetAsync(TestConstants.Endpoints.GetCurrentHousehold);
        getCurrent.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task KickMember_Returns200_WhenOwnerKicksMember()
    {
        await ResetAsync();
        var ownerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var ownerClient = CreateClientWithToken(ownerId);
        var memberClient = CreateClientWithToken(memberId, TestConstants.TestData.InviteeEmail);

        await CreateHouseholdAndInviteMemberAsync(ownerClient, memberClient);

        var kickResponse = await ownerClient.DeleteAsync($"{TestConstants.Endpoints.KickMember}/{memberId}");

        kickResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var kickBody = await GetFromJsonAsync<KickMemberResponseDto>(kickResponse);
        kickBody.ShouldNotBeNull();
        kickBody.KickedUserId.ShouldBe(memberId);

        var getCurrent = await memberClient.GetAsync(TestConstants.Endpoints.GetCurrentHousehold);
        getCurrent.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task TransferOwnership_Returns200_WhenOwnerTransfersToMember()
    {
        await ResetAsync();
        var ownerId = Guid.NewGuid();
        var newOwnerId = Guid.NewGuid();
        var ownerClient = CreateClientWithToken(ownerId);
        var newOwnerClient = CreateClientWithToken(newOwnerId, TestConstants.TestData.InviteeEmail);

        var householdId = await CreateHouseholdAndInviteMemberAsync(ownerClient, newOwnerClient);

        var transferResponse = await ownerClient.PostAsJsonAsync(TestConstants.Endpoints.TransferOwnership, new TransferOwnershipRequestDto(newOwnerId));

        transferResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var transferBody = await GetFromJsonAsync<TransferOwnershipResponseDto>(transferResponse);
        transferBody.ShouldNotBeNull();
        transferBody.HouseholdId.ShouldBe(householdId);
        transferBody.PreviousOwnerId.ShouldBe(ownerId);
        transferBody.NewOwnerId.ShouldBe(newOwnerId);
    }

    [Fact]
    public async Task Join_Returns400_WhenCodeInvalid()
    {
        await ResetAsync();
        var userId = Guid.NewGuid();
        var client = CreateClientWithToken(userId);
        // Ensure token is valid by creating a household first (same client, so auth is verified).
        var createResponse = await client.PostAsJsonAsync(TestConstants.Endpoints.CreateHousehold, new CreateHouseholdRequestDto(TestConstants.TestData.HouseholdName));
        createResponse.EnsureSuccessStatusCode();

        var joinResponse = await client.PostAsJsonAsync(TestConstants.Endpoints.Join, new AcceptHouseholdInvitationRequestDto("invalid-code"));

        joinResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    private async Task<Guid> CreateHouseholdAndInviteMemberAsync(HttpClient ownerClient, HttpClient memberClient)
    {
        var createResponse = await ownerClient.PostAsJsonAsync(TestConstants.Endpoints.CreateHousehold, new CreateHouseholdRequestDto(TestConstants.TestData.HouseholdName));
        createResponse.EnsureSuccessStatusCode();
        var created = await GetFromJsonAsync<CreateHouseholdResponseDto>(createResponse);
        created.ShouldNotBeNull();
        var householdId = created.Id;

        var inviteResponse = await ownerClient.PostAsJsonAsync(TestConstants.Endpoints.SendInvite,
            new SendHouseholdInvitationRequestDto(TestConstants.TestData.InviteeEmail, householdId.ToString()));
        inviteResponse.EnsureSuccessStatusCode();
        var inviteBody = await GetFromJsonAsync<SendHouseholdInvitationResponseDto>(inviteResponse);
        inviteBody.ShouldNotBeNull();

        var joinResponse = await memberClient.PostAsJsonAsync(TestConstants.Endpoints.Join, new AcceptHouseholdInvitationRequestDto(inviteBody.Code));
        joinResponse.EnsureSuccessStatusCode();
        return householdId;
    }
}
