using System.Net;
using System.Net.Http.Json;
using PantryCloud.Identity.Application.DTOs;
using PantryCloud.Identity.IntegrationTests.Constants;
using PantryCloud.Identity.IntegrationTests.Infrastructure;
using Shouldly;

namespace PantryCloud.Identity.IntegrationTests.Tests;

public class SessionsEndpointTests(IdentityTestFixture fixture) : BaseIntegrationTest(fixture)
{
    [Fact]
    public async Task ListSessions_ReturnsSessions_WhenAuthenticated()
    {
        await ResetAsync();
        var user = await SeedUserAsync(TestConstants.Users.DefaultEmail, TestConstants.Users.DefaultPassword, verified: true);
        await SeedRefreshTokenAsync(user.Id, expired: false);

        var loginResponse = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.Login, new LoginRequestDto(TestConstants.Users.DefaultEmail, TestConstants.Users.DefaultPassword));
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var loginResult = await GetFromJsonAsync<LoginResponseDto>(loginResponse);
        loginResult.ShouldNotBeNull();

        HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.AccessToken);
        var response = await HttpClient.GetAsync(TestConstants.Endpoints.Sessions);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await GetFromJsonAsync<ListSessionsResponseDto>(response);
        result.ShouldNotBeNull();
        result.Sessions.ShouldNotBeEmpty();
        loginResult.SessionId.ShouldNotBeNull();
        result.Sessions.ShouldContain(s => s.Id == loginResult.SessionId!.Value);
    }

    [Fact]
    public async Task RevokeSession_InvalidatesSession()
    {
        await ResetAsync();
        var user = await SeedUserAsync(TestConstants.Users.DefaultEmail, TestConstants.Users.DefaultPassword, verified: true);
        var refreshToken = await SeedRefreshTokenAsync(user.Id, expired: false);

        var loginResponse = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.Login, new LoginRequestDto(TestConstants.Users.DefaultEmail, TestConstants.Users.DefaultPassword));
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var loginResult = await GetFromJsonAsync<LoginResponseDto>(loginResponse);
        loginResult.ShouldNotBeNull();

        var session = await GetSessionByRefreshTokenAsync(refreshToken);
        session.ShouldNotBeNull();

        HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.AccessToken);
        var revokeResponse = await HttpClient.DeleteAsync($"{TestConstants.Endpoints.Sessions}/{session.Id}");
        revokeResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var sessionAfter = await GetSessionByRefreshTokenAsync(refreshToken);
        sessionAfter.ShouldBeNull();

        var refreshResponse = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.Refresh, new RefreshTokenRequestDto(refreshToken));
        refreshResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RevokeAllOtherSessions_KeepsCurrent()
    {
        await ResetAsync();
        var user = await SeedUserAsync(TestConstants.Users.DefaultEmail, TestConstants.Users.DefaultPassword, verified: true);
        await SeedRefreshTokenAsync(user.Id, expired: false);
        await SeedRefreshTokenAsync(user.Id, expired: false);

        var loginResponse = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.Login, new LoginRequestDto(TestConstants.Users.DefaultEmail, TestConstants.Users.DefaultPassword));
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var loginResult = await GetFromJsonAsync<LoginResponseDto>(loginResponse);
        loginResult.ShouldNotBeNull();

        HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.AccessToken);
        var revokeResponse = await HttpClient.DeleteAsync($"{TestConstants.Endpoints.Sessions}/others");
        revokeResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var listResponse = await HttpClient.GetAsync(TestConstants.Endpoints.Sessions);
        var listResult = await GetFromJsonAsync<ListSessionsResponseDto>(listResponse);
        listResult.ShouldNotBeNull();
        listResult.Sessions.Count.ShouldBe(1);
        listResult.Sessions[0].Id.ShouldBe(loginResult.SessionId!.Value);
    }

    [Fact]
    public async Task Logout_RevokesCurrentSession()
    {
        await ResetAsync();
        var user = await SeedUserAsync(TestConstants.Users.DefaultEmail, TestConstants.Users.DefaultPassword, verified: true);

        var loginResponse = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.Login, new LoginRequestDto(TestConstants.Users.DefaultEmail, TestConstants.Users.DefaultPassword));
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var loginResult = await GetFromJsonAsync<LoginResponseDto>(loginResponse);
        loginResult.ShouldNotBeNull();

        var logoutResponse = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.Logout, new LogoutRequestDto(loginResult.RefreshToken));
        logoutResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var refreshResponse = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.Refresh, new RefreshTokenRequestDto(loginResult.RefreshToken));
        refreshResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
