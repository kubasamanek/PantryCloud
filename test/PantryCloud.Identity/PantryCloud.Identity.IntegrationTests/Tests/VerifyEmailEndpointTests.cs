using System.Net;
using System.Net.Http.Json;
using PantryCloud.Identity.Application.DTOs;
using PantryCloud.Identity.IntegrationTests.Constants;
using PantryCloud.Identity.IntegrationTests.Infrastructure;
using Shouldly;

namespace PantryCloud.Identity.IntegrationTests.Tests;

public class VerifyEmailEndpointTests(IdentityTestFixture fixture) : BaseIntegrationTest(fixture)
{
    [Fact]
    public async Task VerifyEmail_ShouldReturnOk_AndVerifyUser_WhenTokenIsValid()
    {
        // Arrange
        await SeedUserAsync(TestConstants.Users.DefaultEmail, TestConstants.Users.DefaultPassword, verified: false);
        var token = await SeedVerifyEmailTokenAsync(TestConstants.Users.DefaultEmail, used: false, expired: false);

        var request = new VerifyEmailRequestDto(TestConstants.Users.DefaultEmail, token.Token);

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.VerifyEmail, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Verify user email was verified in database
        var user = await GetUserByEmailAsync(TestConstants.Users.DefaultEmail);
        user.ShouldNotBeNull();
        user.EmailVerified.ShouldBeTrue();

        // Verify token was marked as used
        var updatedToken = await GetVerifyEmailTokenAsync(TestConstants.Users.DefaultEmail, token.Token);
        updatedToken.ShouldNotBeNull();
        updatedToken.UsedAt.ShouldNotBeNull();
        updatedToken.IsUsed.ShouldBeTrue();
    }

    [Fact]
    public async Task VerifyEmail_ShouldReturnUnauthorized_WhenTokenIsInvalid()
    {
        await ResetAsync();
        // Arrange
        await SeedUserAsync(TestConstants.Users.DefaultEmail, TestConstants.Users.DefaultPassword, verified: false);

        var request = new VerifyEmailRequestDto(TestConstants.Users.DefaultEmail, "invalid-token");

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.VerifyEmail, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        // Verify user email was not verified
        var user = await GetUserByEmailAsync(TestConstants.Users.DefaultEmail);
        user.ShouldNotBeNull();
        user.EmailVerified.ShouldBeFalse();
    }

    [Fact]
    public async Task VerifyEmail_ShouldReturnUnauthorized_WhenTokenIsAlreadyUsed()
    {
        await ResetAsync();
        // Arrange
        await SeedUserAsync(TestConstants.Users.DefaultEmail, TestConstants.Users.DefaultPassword, verified: false);
        var token = await SeedVerifyEmailTokenAsync(TestConstants.Users.DefaultEmail, used: true, expired: false);

        var request = new VerifyEmailRequestDto(TestConstants.Users.DefaultEmail, token.Token);

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.VerifyEmail, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task VerifyEmail_ShouldReturnUnauthorized_WhenTokenIsExpired()
    {
        await ResetAsync();
        // Arrange
        await SeedUserAsync(TestConstants.Users.DefaultEmail, TestConstants.Users.DefaultPassword, verified: false);
        var token = await SeedVerifyEmailTokenAsync(TestConstants.Users.DefaultEmail, used: false, expired: true);

        var request = new VerifyEmailRequestDto(TestConstants.Users.DefaultEmail, token.Token);

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.VerifyEmail, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        // Verify user email was not verified
        var user = await GetUserByEmailAsync(TestConstants.Users.DefaultEmail);
        user.ShouldNotBeNull();
        user.EmailVerified.ShouldBeFalse();
    }

    [Fact]
    public async Task VerifyEmail_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        await ResetAsync();
        // Arrange
        var token = await SeedVerifyEmailTokenAsync("nonexistent@pantrycloud.com", used: false, expired: false);

        var request = new VerifyEmailRequestDto("nonexistent@pantrycloud.com", token.Token);

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.VerifyEmail, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task VerifyEmail_ShouldReturnBadRequest_WhenEmailIsEmpty()
    {
        await ResetAsync();
        // Arrange
        var request = new VerifyEmailRequestDto(TestConstants.InvalidData.EmptyEmail, "some-token");

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.VerifyEmail, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task VerifyEmail_ShouldReturnBadRequest_WhenTokenIsEmpty()
    {
        await ResetAsync();
        // Arrange
        var request = new VerifyEmailRequestDto(TestConstants.Users.DefaultEmail, "");

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.VerifyEmail, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task VerifyEmail_ShouldNotVerifyTwice_WhenTokenUsedOnce()
    {
        await ResetAsync();
        // Arrange
        await SeedUserAsync(TestConstants.Users.DefaultEmail, TestConstants.Users.DefaultPassword, verified: false);
        var token = await SeedVerifyEmailTokenAsync(TestConstants.Users.DefaultEmail, used: false, expired: false);

        var request = new VerifyEmailRequestDto(TestConstants.Users.DefaultEmail, token.Token);

        // Act - First verification
        var response1 = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.VerifyEmail, request);

        // Act - Second verification attempt with same token
        var response2 = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.VerifyEmail, request);

        // Assert
        response1.StatusCode.ShouldBe(HttpStatusCode.OK);
        response2.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}

