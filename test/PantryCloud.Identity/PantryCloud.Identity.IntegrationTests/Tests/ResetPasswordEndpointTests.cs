using System.Net;
using System.Net.Http.Json;
using PantryCloud.Identity.Application.DTOs;
using PantryCloud.Identity.Infrastructure;
using PantryCloud.Identity.IntegrationTests.Constants;
using PantryCloud.Identity.IntegrationTests.Infrastructure;
using Shouldly;

namespace PantryCloud.Identity.IntegrationTests.Tests;

public class ResetPasswordEndpointTests(IdentityTestFixture fixture) : BaseIntegrationTest(fixture)
{
    [Fact]
    public async Task ResetPassword_ShouldReturnOk_AndChangePassword_WhenTokenIsValid()
    {
        await ResetAsync();
        // Arrange
        await SeedUserAsync(TestConstants.Users.DefaultEmail, TestConstants.Users.DefaultPassword, verified: true);
        var token = await SeedResetPasswordTokenAsync(TestConstants.Users.DefaultEmail, used: false, expired: false);

        var originalUser = await GetUserByEmailAsync(TestConstants.Users.DefaultEmail);
        var originalPasswordHash = originalUser!.PasswordHash;

        var newPassword = "NewPassword123!";
        var request = new ResetPasswordRequestDto(TestConstants.Users.DefaultEmail, token.Token, newPassword);

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.ResetPassword, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Verify password was changed in database
        var updatedUser = await GetUserByEmailAsync(TestConstants.Users.DefaultEmail);
        updatedUser.ShouldNotBeNull();
        updatedUser.PasswordHash.ShouldNotBe(originalPasswordHash);
        updatedUser.PasswordHash.ShouldNotBe(newPassword); // Should be hashed

        // Verify token was marked as used
        var updatedToken = await GetResetPasswordTokenAsync(TestConstants.Users.DefaultEmail, token.Token);
        updatedToken.ShouldNotBeNull();
        updatedToken.UsedAt.ShouldNotBeNull();
        updatedToken.IsUsed.ShouldBeTrue();

        // Verify user can login with new password
        var loginRequest = new LoginRequestDto(TestConstants.Users.DefaultEmail, newPassword);
        var loginResponse = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.Login, loginRequest);
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnUnauthorized_WhenTokenIsInvalid()
    {
        await ResetAsync();
        // Arrange
        await SeedUserAsync(TestConstants.Users.DefaultEmail, TestConstants.Users.DefaultPassword, verified: true);

        var request = new ResetPasswordRequestDto(
            TestConstants.Users.DefaultEmail,
            "invalid-token",
            "NewPassword123!"
        );

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.ResetPassword, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        // Verify password was not changed
        var user = await GetUserByEmailAsync(TestConstants.Users.DefaultEmail);
        user.ShouldNotBeNull();

        // Old password should still work
        var loginRequest = new LoginRequestDto(TestConstants.Users.DefaultEmail, TestConstants.Users.DefaultPassword);
        var loginResponse = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.Login, loginRequest);
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnUnauthorized_WhenTokenIsAlreadyUsed()
    {
        await ResetAsync();
        // Arrange
        await SeedUserAsync(TestConstants.Users.DefaultEmail, TestConstants.Users.DefaultPassword, verified: true);
        var token = await SeedResetPasswordTokenAsync(TestConstants.Users.DefaultEmail, used: true, expired: false);

        var request = new ResetPasswordRequestDto(
            TestConstants.Users.DefaultEmail,
            token.Token,
            "NewPassword123!"
        );

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.ResetPassword, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnUnauthorized_WhenTokenIsExpired()
    {
        await ResetAsync();
        // Arrange
        await SeedUserAsync(TestConstants.Users.DefaultEmail, TestConstants.Users.DefaultPassword, verified: true);
        var token = await SeedResetPasswordTokenAsync(TestConstants.Users.DefaultEmail, used: false, expired: true);

        var request = new ResetPasswordRequestDto(
            TestConstants.Users.DefaultEmail,
            token.Token,
            "NewPassword123!"
        );

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.ResetPassword, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        // Verify password was not changed
        var loginRequest = new LoginRequestDto(TestConstants.Users.DefaultEmail, TestConstants.Users.DefaultPassword);
        var loginResponse = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.Login, loginRequest);
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        await ResetAsync();
        // Arrange
        var token = await SeedResetPasswordTokenAsync("nonexistent@pantrycloud.com", used: false, expired: false);

        var request = new ResetPasswordRequestDto(
            "nonexistent@pantrycloud.com",
            token.Token,
            "NewPassword123!"
        );

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.ResetPassword, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnBadRequest_WhenEmailIsEmpty()
    {
        await ResetAsync();
        // Arrange
        var request = new ResetPasswordRequestDto(
            TestConstants.InvalidData.EmptyEmail,
            "some-token",
            "NewPassword123!"
        );

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.ResetPassword, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnBadRequest_WhenTokenIsEmpty()
    {
        await ResetAsync();
        // Arrange
        var request = new ResetPasswordRequestDto(
            TestConstants.Users.DefaultEmail,
            "",
            "NewPassword123!"
        );

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.ResetPassword, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnBadRequest_WhenNewPasswordIsEmpty()
    {
        await ResetAsync();
        // Arrange
        await SeedUserAsync(TestConstants.Users.DefaultEmail, TestConstants.Users.DefaultPassword, verified: true);
        var token = await SeedResetPasswordTokenAsync(TestConstants.Users.DefaultEmail, used: false, expired: false);

        var request = new ResetPasswordRequestDto(
            TestConstants.Users.DefaultEmail,
            token.Token,
            TestConstants.InvalidData.EmptyPassword
        );

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.ResetPassword, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnBadRequest_WhenNewPasswordIsTooWeak()
    {
        await ResetAsync();
        // Arrange
        await SeedUserAsync(TestConstants.Users.DefaultEmail, TestConstants.Users.DefaultPassword, verified: true);
        var token = await SeedResetPasswordTokenAsync(TestConstants.Users.DefaultEmail, used: false, expired: false);

        var request = new ResetPasswordRequestDto(
            TestConstants.Users.DefaultEmail,
            token.Token,
            TestConstants.InvalidData.WeakPassword
        );

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.ResetPassword, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResetPassword_ShouldNotWorkTwice_WithSameToken()
    {
        await ResetAsync();
        // Arrange
        await SeedUserAsync(TestConstants.Users.DefaultEmail, TestConstants.Users.DefaultPassword, verified: true);
        var token = await SeedResetPasswordTokenAsync(TestConstants.Users.DefaultEmail, used: false, expired: false);

        var request1 = new ResetPasswordRequestDto(
            TestConstants.Users.DefaultEmail,
            token.Token,
            "NewPassword123!"
        );

        var request2 = new ResetPasswordRequestDto(
            TestConstants.Users.DefaultEmail,
            token.Token,
            "AnotherPassword123!"
        );

        // Act - First reset
        var response1 = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.ResetPassword, request1);

        // Act - Second reset attempt with same token
        var response2 = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.ResetPassword, request2);

        // Assert
        response1.StatusCode.ShouldBe(HttpStatusCode.OK);
        response2.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        // Verify user can login with first new password, not the second
        var loginRequest = new LoginRequestDto(TestConstants.Users.DefaultEmail, "NewPassword123!");
        var loginResponse = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.Login, loginRequest);
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ResetPassword_ShouldWork_ForUnverifiedUsers()
    {
        await ResetAsync();
        // Arrange - User with unverified email
        await SeedUserAsync(TestConstants.Users.DefaultEmail, TestConstants.Users.DefaultPassword, verified: false);
        var token = await SeedResetPasswordTokenAsync(TestConstants.Users.DefaultEmail, used: false, expired: false);

        var newPassword = "NewPassword123!";
        var request = new ResetPasswordRequestDto(TestConstants.Users.DefaultEmail, token.Token, newPassword);

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.ResetPassword, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Verify password was changed
        var user = await GetUserByEmailAsync(TestConstants.Users.DefaultEmail);
        user.ShouldNotBeNull();
        PasswordHasher.Verify(newPassword, user.PasswordHash).ShouldBeTrue();
    }
}

