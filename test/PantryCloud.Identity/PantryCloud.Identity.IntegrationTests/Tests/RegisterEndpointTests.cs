using System.Net;
using System.Net.Http.Json;
using PantryCloud.Identity.Application.DTOs;
using PantryCloud.Identity.IntegrationTests.Constants;
using PantryCloud.Identity.IntegrationTests.Infrastructure;
using Shouldly;

namespace PantryCloud.Identity.IntegrationTests.Tests;

public class RegisterEndpointTests(IdentityTestFixture fixture) : BaseIntegrationTest(fixture)
{
    [Fact]
    public async Task Register_ShouldReturnOk_AndCreateUser_WhenDataIsValid()
    {
        await ResetAsync();
        // Arrange
        var request = new RegisterRequestDto(
            TestConstants.Users.DefaultEmail,
            TestConstants.Users.DefaultPassword
        );

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.Register, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await GetFromJsonAsync<RegisterResponseDto>(response);
        result.ShouldNotBeNull();
        result.UserId.ShouldNotBeNullOrWhiteSpace();
        result.VerifyEmailToken.ShouldNotBeNullOrWhiteSpace();

        // Verify user was created in database
        var user = await GetUserByEmailAsync(TestConstants.Users.DefaultEmail);
        user.ShouldNotBeNull();
        user.EmailVerified.ShouldBeFalse();
        user.PasswordHash.ShouldNotBe(TestConstants.Users.DefaultPassword); // Password should be hashed

        // Verify VerifyEmailToken was created in database
        var token = await GetVerifyEmailTokenAsync(TestConstants.Users.DefaultEmail, result.VerifyEmailToken);
        token.ShouldNotBeNull();
        token.IsExpired.ShouldBeFalse();
        token.IsUsed.ShouldBeFalse();
    }

    [Fact]
    public async Task Register_ShouldReturnConflict_WhenEmailAlreadyExists()
    {
        await ResetAsync();
        // Arrange
        await SeedUserAsync(TestConstants.Users.DefaultEmail, TestConstants.Users.DefaultPassword);

        var request = new RegisterRequestDto(
            TestConstants.Users.DefaultEmail,
            TestConstants.Users.DefaultPassword
        );

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.Register, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenEmailIsEmpty()
    {
        await ResetAsync();
        // Arrange
        var request = new RegisterRequestDto(
            TestConstants.InvalidData.EmptyEmail,
            TestConstants.Users.DefaultPassword
        );

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.Register, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenPasswordIsEmpty()
    {
        await ResetAsync();
        // Arrange
        var request = new RegisterRequestDto(
            TestConstants.Users.DefaultEmail,
            TestConstants.InvalidData.EmptyPassword
        );

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.Register, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenEmailFormatIsInvalid()
    {
        await ResetAsync();
        // Arrange
        var request = new RegisterRequestDto(
            TestConstants.InvalidData.InvalidEmailFormat,
            TestConstants.Users.DefaultPassword
        );

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.Register, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenPasswordIsTooWeak()
    {
        await ResetAsync();
        // Arrange
        var request = new RegisterRequestDto(
            TestConstants.Users.DefaultEmail,
            TestConstants.InvalidData.WeakPassword
        );

        // Act
        var response = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.Register, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_ShouldCreateMultipleUsers_WhenEmailsAreDifferent()
    {
        await ResetAsync();
        // Arrange
        var request1 = new RegisterRequestDto(
            TestConstants.Users.DefaultEmail,
            TestConstants.Users.DefaultPassword
        );

        var request2 = new RegisterRequestDto(
            TestConstants.Users.AlternativeEmail,
            TestConstants.Users.AlternativePassword
        );

        // Act
        var response1 = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.Register, request1);
        var response2 = await HttpClient.PostAsJsonAsync(TestConstants.Endpoints.Register, request2);

        // Assert
        response1.StatusCode.ShouldBe(HttpStatusCode.OK);
        response2.StatusCode.ShouldBe(HttpStatusCode.OK);

        var user1 = await GetUserByEmailAsync(TestConstants.Users.DefaultEmail);
        var user2 = await GetUserByEmailAsync(TestConstants.Users.AlternativeEmail);

        user1.ShouldNotBeNull();
        user2.ShouldNotBeNull();
        user1.Id.ShouldNotBe(user2.Id);
    }
}

