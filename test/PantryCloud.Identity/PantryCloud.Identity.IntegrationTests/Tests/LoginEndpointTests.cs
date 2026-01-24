using System.Net;
using System.Net.Http.Json;
using PantryCloud.Identity.Application.DTOs;
using PantryCloud.Identity.IntegrationTests.Constants;
using PantryCloud.Identity.IntegrationTests.Infrastructure;
using Shouldly;

namespace PantryCloud.Identity.IntegrationTests.Tests;

public class LoginEndpointTests(IdentityIntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Login_ShouldReturnOk_AndTokens_WhenCredentialsAreValid()
    {
        // Arrange
        await SeedUserAsync(TestConstants.Users.DefaultEmail, TestConstants.Users.DefaultPassword, verified: true);

        var request = new LoginRequestDto(
            TestConstants.Users.DefaultEmail,
            TestConstants.Users.DefaultPassword
        );

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.Login, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await GetFromJsonAsync<LoginResponseDto>(response);
        result.ShouldNotBeNull();
        result.AccessToken.ShouldNotBeNullOrWhiteSpace();
        result.RefreshToken.ShouldNotBeNullOrWhiteSpace();

        // Verify refresh token was persisted in database
        var user = await GetUserByEmailAsync(TestConstants.Users.DefaultEmail);
        user.ShouldNotBeNull();
        user.RefreshToken.ShouldBe(result.RefreshToken);
        user.RefreshTokenExpiryTime.ShouldNotBeNull();
        user.RefreshTokenExpiryTime!.Value.ShouldBeGreaterThan(DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsWrong()
    {
        // Arrange
        await SeedUserAsync(TestConstants.Users.DefaultEmail, TestConstants.Users.DefaultPassword, verified: true);

        var request = new LoginRequestDto(
            TestConstants.Users.DefaultEmail,
            "WrongPassword123!"
        );

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.Login, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenUserDoesNotExist()
    {
        // Arrange
        var request = new LoginRequestDto(
            "nonexistent@pantrycloud.com",
            TestConstants.Users.DefaultPassword
        );

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.Login, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenEmailIsNotVerified()
    {
        // Arrange
        await SeedUserAsync(TestConstants.Users.DefaultEmail, TestConstants.Users.DefaultPassword, verified: false);

        var request = new LoginRequestDto(
            TestConstants.Users.DefaultEmail,
            TestConstants.Users.DefaultPassword
        );

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.Login, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenEmailIsEmpty()
    {
        // Arrange
        var request = new LoginRequestDto(
            TestConstants.InvalidData.EmptyEmail,
            TestConstants.Users.DefaultPassword
        );

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.Login, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenPasswordIsEmpty()
    {
        // Arrange
        var request = new LoginRequestDto(
            TestConstants.Users.DefaultEmail,
            TestConstants.InvalidData.EmptyPassword
        );

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.Login, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ShouldUpdateRefreshToken_OnSuccessiveLogins()
    {
        // Arrange
        await SeedUserAsync(TestConstants.Users.DefaultEmail, TestConstants.Users.DefaultPassword, verified: true);

        var request = new LoginRequestDto(
            TestConstants.Users.DefaultEmail,
            TestConstants.Users.DefaultPassword
        );

        // Act - First login
        var response1 = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.Login, request);
        var result1 = await GetFromJsonAsync<LoginResponseDto>(response1);

        // Act - Second login
        var response2 = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.Login, request);
        var result2 = await GetFromJsonAsync<LoginResponseDto>(response2);

        // Assert
        response1.StatusCode.ShouldBe(HttpStatusCode.OK);
        response2.StatusCode.ShouldBe(HttpStatusCode.OK);

        result1.ShouldNotBeNull();
        result2.ShouldNotBeNull();

        // Refresh tokens should be different
        result1.RefreshToken.ShouldNotBe(result2.RefreshToken);

        // Database should have the latest refresh token
        var user = await GetUserByEmailAsync(TestConstants.Users.DefaultEmail);
        user.ShouldNotBeNull();
        user.RefreshToken.ShouldBe(result2.RefreshToken);
    }
}

