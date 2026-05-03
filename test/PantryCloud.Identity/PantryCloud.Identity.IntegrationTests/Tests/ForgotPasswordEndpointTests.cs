using System.Net;
using System.Net.Http.Json;
using PantryCloud.Identity.Application.DTOs;
using PantryCloud.Identity.IntegrationTests.Constants;
using PantryCloud.Identity.IntegrationTests.Infrastructure;
using Shouldly;

namespace PantryCloud.Identity.IntegrationTests.Tests;

public class ForgotPasswordEndpointTests(IdentityTestFixture fixture) : BaseIntegrationTest(fixture)
{
    [Fact]
    public async Task ForgotPassword_ShouldReturnOk_AndCreateToken_WhenUserExists()
    {
        await ResetAsync();
        // Arrange
        await SeedUserAsync(TestConstants.Users.DefaultEmail, TestConstants.Users.DefaultPassword, verified: true);

        var request = new ForgotPasswordRequestDto(TestConstants.Users.DefaultEmail);

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.ForgotPassword, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await GetFromJsonAsync<ForgotPasswordResponseDto>(response);
        result.ShouldNotBeNull();
        result.Token.ShouldNotBeNullOrWhiteSpace();
        result.Url.ShouldNotBeNullOrWhiteSpace();
        result.Url.ShouldContain("reset-password");
        result.Url.ShouldContain($"email={TestConstants.Users.DefaultEmail}");

        // Verify token was created in database
        var tokenEntity = await GetResetPasswordTokenAsync(TestConstants.Users.DefaultEmail, result.Token);
        tokenEntity.ShouldNotBeNull();
        tokenEntity.IsExpired.ShouldBeFalse();
        tokenEntity.IsUsed.ShouldBeFalse();
    }

    [Fact]
    public async Task ForgotPassword_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        await ResetAsync();
        // Arrange
        var request = new ForgotPasswordRequestDto("nonexistent@pantrycloud.com");

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.ForgotPassword, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ForgotPassword_ShouldReturnBadRequest_WhenEmailIsEmpty()
    {
        await ResetAsync();
        // Arrange
        var request = new ForgotPasswordRequestDto(TestConstants.InvalidData.EmptyEmail);

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.ForgotPassword, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ForgotPassword_ShouldReturnBadRequest_WhenEmailFormatIsInvalid()
    {
        await ResetAsync();
        // Arrange
        var request = new ForgotPasswordRequestDto(TestConstants.InvalidData.InvalidEmailFormat);

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.ForgotPassword, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ForgotPassword_ShouldCreateMultipleTokens_WhenCalledMultipleTimes()
    {
        await ResetAsync();
        // Arrange
        await SeedUserAsync(TestConstants.Users.DefaultEmail, TestConstants.Users.DefaultPassword, verified: true);

        var request = new ForgotPasswordRequestDto(TestConstants.Users.DefaultEmail);

        // Act - First request
        var response1 = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.ForgotPassword, request);
        var result1 = await GetFromJsonAsync<ForgotPasswordResponseDto>(response1);

        // Act - Second request
        var response2 = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.ForgotPassword, request);
        var result2 = await GetFromJsonAsync<ForgotPasswordResponseDto>(response2);

        // Assert
        response1.StatusCode.ShouldBe(HttpStatusCode.OK);
        response2.StatusCode.ShouldBe(HttpStatusCode.OK);

        result1.ShouldNotBeNull();
        result2.ShouldNotBeNull();

        // Tokens should be different
        result1.Token.ShouldNotBe(result2.Token);

        // Both tokens should exist in database
        var tokens = await GetResetPasswordTokensByEmailAsync(TestConstants.Users.DefaultEmail);
        tokens.Count.ShouldBeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task ForgotPassword_ShouldWork_ForUnverifiedUsers()
    {
        await ResetAsync();
        // Arrange - User with unverified email
        await SeedUserAsync(TestConstants.Users.DefaultEmail, TestConstants.Users.DefaultPassword, verified: false);

        var request = new ForgotPasswordRequestDto(TestConstants.Users.DefaultEmail);

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.ForgotPassword, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await GetFromJsonAsync<ForgotPasswordResponseDto>(response);
        result.ShouldNotBeNull();
        result.Token.ShouldNotBeNullOrWhiteSpace();
    }
}

