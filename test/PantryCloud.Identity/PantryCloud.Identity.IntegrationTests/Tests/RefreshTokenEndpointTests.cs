using System.Net;
using System.Net.Http.Json;
using PantryCloud.Identity.Application.DTOs;
using PantryCloud.Identity.IntegrationTests.Constants;
using PantryCloud.Identity.IntegrationTests.Infrastructure;
using Shouldly;

namespace PantryCloud.Identity.IntegrationTests.Tests;

public class RefreshTokenEndpointTests(IdentityTestFixture fixture) : BaseIntegrationTest(fixture)
{
    [Fact]
    public async Task RefreshToken_ShouldReturnOk_AndNewTokens_WhenTokenIsValid()
    {
        await ResetAsync();
        // Arrange
        var user = await SeedUserAsync(TestConstants.Users.DefaultEmail, TestConstants.Users.DefaultPassword, verified: true);
        var oldRefreshToken = await SeedRefreshTokenAsync(user.Id, expired: false);

        var request = new RefreshTokenRequestDto(oldRefreshToken);

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.Refresh, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await GetFromJsonAsync<RefreshTokenResponseDto>(response);
        result.ShouldNotBeNull();
        result.AccessToken.ShouldNotBeNullOrWhiteSpace();
        result.RefreshToken.ShouldNotBeNullOrWhiteSpace();

        // New refresh token should be different from old one
        result.RefreshToken.ShouldNotBe(oldRefreshToken);

        // Verify new refresh token was persisted in database
        var session = await GetSessionByRefreshTokenAsync(result.RefreshToken);
        session.ShouldNotBeNull();
        session.RefreshToken.ShouldBe(result.RefreshToken);
        session.ExpiresAt.ShouldBeGreaterThan(DateTime.UtcNow);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnUnauthorized_WhenTokenDoesNotExist()
    {
        await ResetAsync();
        // Arrange
        var request = new RefreshTokenRequestDto("non-existent-token");

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.Refresh, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnUnauthorized_WhenTokenIsExpired()
    {
        await ResetAsync();
        // Arrange
        var user = await SeedUserAsync(TestConstants.Users.DefaultEmail, TestConstants.Users.DefaultPassword, verified: true);
        var expiredToken = await SeedRefreshTokenAsync(user.Id, expired: true);

        var request = new RefreshTokenRequestDto(expiredToken);

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.Refresh, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnBadRequest_WhenTokenIsEmpty()
    {
        await ResetAsync();
        // Arrange
        var request = new RefreshTokenRequestDto("");

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.Refresh, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RefreshToken_ShouldRotateTokens_OnSuccessiveRefreshes()
    {
        await ResetAsync();
        // Arrange
        var user = await SeedUserAsync(TestConstants.Users.DefaultEmail, TestConstants.Users.DefaultPassword, verified: true);
        var firstRefreshToken = await SeedRefreshTokenAsync(user.Id, expired: false);

        // Act - First refresh
        var request1 = new RefreshTokenRequestDto(firstRefreshToken);
        var response1 = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.Refresh, request1);
        var result1 = await GetFromJsonAsync<RefreshTokenResponseDto>(response1);

        // Act - Second refresh with new token
        var request2 = new RefreshTokenRequestDto(result1!.RefreshToken);
        var response2 = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.Refresh, request2);
        var result2 = await GetFromJsonAsync<RefreshTokenResponseDto>(response2);

        // Assert
        response1.StatusCode.ShouldBe(HttpStatusCode.OK);
        response2.StatusCode.ShouldBe(HttpStatusCode.OK);

        result1.ShouldNotBeNull();
        result2.ShouldNotBeNull();

        // All three tokens should be different
        firstRefreshToken.ShouldNotBe(result1.RefreshToken);
        result1.RefreshToken.ShouldNotBe(result2.RefreshToken);

        // Database should have the latest refresh token
        var session = await GetSessionByRefreshTokenAsync(result2.RefreshToken);
        session.ShouldNotBeNull();
        session.RefreshToken.ShouldBe(result2.RefreshToken);
    }

    [Fact]
    public async Task RefreshToken_ShouldNotWork_WithOldTokenAfterRotation()
    {
        await ResetAsync();
        // Arrange
        var user = await SeedUserAsync(TestConstants.Users.DefaultEmail, TestConstants.Users.DefaultPassword, verified: true);
        var oldRefreshToken = await SeedRefreshTokenAsync(user.Id, expired: false);

        // Act - First refresh to rotate token
        var request1 = new RefreshTokenRequestDto(oldRefreshToken);
        var response1 = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.Refresh, request1);
        response1.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Act - Try to use old token again
        var request2 = new RefreshTokenRequestDto(oldRefreshToken);
        var response2 = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.Refresh, request2);

        // Assert
        response2.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}

