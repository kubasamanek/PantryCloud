using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PantryCloud.Identity.Application.DTOs;
using PantryCloud.Identity.Infrastructure.Persistence;
using PantryCloud.Identity.IntegrationTests.Constants;
using PantryCloud.Identity.IntegrationTests.Infrastructure;
using Shouldly;

namespace PantryCloud.Identity.IntegrationTests.Tests;

public class ForgotPasswordEndpointTests(IdentityIntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task ForgotPassword_ShouldReturnOk_AndCreateToken_WhenUserExists()
    {
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
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tokenEntity = await dbContext.ResetPasswordTokens
            .FirstOrDefaultAsync(t => t.Email == TestConstants.Users.DefaultEmail && t.Token == result.Token);

        tokenEntity.ShouldNotBeNull();
        tokenEntity.IsExpired.ShouldBeFalse();
        tokenEntity.IsUsed.ShouldBeFalse();
    }

    [Fact]
    public async Task ForgotPassword_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
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
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tokens = await dbContext.ResetPasswordTokens
            .Where(t => t.Email == TestConstants.Users.DefaultEmail)
            .ToListAsync();

        tokens.Count.ShouldBeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task ForgotPassword_ShouldWork_ForUnverifiedUsers()
    {
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

