using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PantryCloud.Identity.Application;
using PantryCloud.Identity.Core.Entities;
using PantryCloud.Identity.Application.DTOs;
using PantryCloud.Identity.Core;
using PantryCloud.Identity.Core.Errors;
using PantryCloud.Identity.Infrastructure.Services;
using PantryCloud.SharedKernel.Email;
using Shouldly;

namespace PantryCloud.Identity.UnitTests.Auth;

public class AuthServiceTests
{
    private readonly ILogger<AuthService> _loggerMock = TestHelper.MockLogger();
    private readonly ITokenProvider _tokenProviderMock = TestHelper.MockTokenProvider();
    private readonly ApiConfiguration _configurationMock = TestHelper.MockConfiguration();

    private readonly IIdentityUserContext _userContextMock = TestHelper.MockIdentityUserContext();
    private readonly IEmailSender _emailSenderMock = TestHelper.MockEmailSender();

    [Fact]
    public async Task RegisterAsync_ShouldCreateUser_AndReturnId_WhenNewEmail()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(RegisterAsync_ShouldCreateUser_AndReturnId_WhenNewEmail));
        var authService = new AuthService(_tokenProviderMock, db, _loggerMock, _configurationMock, _userContextMock, _emailSenderMock);

        var request = new RegisterRequestDto(Constants.User.Email, Constants.Passwords.Strong);

        var result = await authService.RegisterAsync(request, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.UserId.ShouldNotBeNullOrWhiteSpace();

        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == request.Email);
        user.ShouldNotBeNull();
        user!.EmailVerified.ShouldBeFalse();
        user.PasswordHash.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnError_WhenEmailAlreadyExists()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(RegisterAsync_ShouldReturnError_WhenEmailAlreadyExists));
        var existing = Constants.ExampleUser;
        db.Users.Add(existing);
        await db.SaveChangesAsync();

        var authService = new AuthService(_tokenProviderMock, db, _loggerMock, _configurationMock, _userContextMock, _emailSenderMock);

        var request = new RegisterRequestDto(existing.Email, Constants.Passwords.Example);

        var result = await authService.RegisterAsync(request, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(AuthErrors.RegistrationUserAlreadyExists(existing.Email));
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnTokens_AndPersistRefreshToken_OnValidCredentials()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(LoginAsync_ShouldReturnTokens_AndPersistRefreshToken_OnValidCredentials));
        var user = TestHelper.MakeUser(Constants.User.Email, Constants.Passwords.Strong, verified: true);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var tokenProvider = TestHelper.MockTokenProvider(accessToken: Constants.Tokens.AccessToken, refreshToken: Constants.Tokens.RefreshToken);
        var authService = new AuthService(tokenProvider, db, _loggerMock, _configurationMock, _userContextMock, _emailSenderMock);

        var request = new LoginRequestDto(user.Email, Constants.Passwords.Strong);

        var result = await authService.LoginAsync(request, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.AccessToken.ShouldBe(Constants.Tokens.AccessToken);
        result.Value.RefreshToken.ShouldBe(Constants.Tokens.RefreshToken);

        var session = await db.RefreshSessions.SingleOrDefaultAsync(s => s.UserId == user.Id);
        session.ShouldNotBeNull();
        session!.RefreshToken.ShouldBe(Constants.Tokens.RefreshToken);
        session.ExpiresAt.ShouldBeGreaterThan(DateTime.UtcNow);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnError_WhenUserNotFound()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(LoginAsync_ShouldReturnError_WhenUserNotFound));
        var authService = new AuthService(_tokenProviderMock, db, _loggerMock, _configurationMock, _userContextMock, _emailSenderMock);

        var result = await authService.LoginAsync(
            new LoginRequestDto(Constants.User.Email, Constants.Passwords.Wrong),
            CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(AuthErrors.LoginInvalidCredentials);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnError_WhenPasswordInvalid()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(LoginAsync_ShouldReturnError_WhenPasswordInvalid));
        var user = Constants.ExampleUser;
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var tokenProvider = TestHelper.MockTokenProvider();
        var logger = TestHelper.MockLogger();
        var authService = new AuthService(tokenProvider, db, logger, _configurationMock, _userContextMock, _emailSenderMock);

        var result = await authService.LoginAsync(
            new LoginRequestDto(user.Email, Constants.Passwords.Wrong),
            CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(AuthErrors.LoginInvalidCredentials);
    }


    [Fact]
    public async Task RefreshTokenAsync_ShouldIssueNewTokens_AndRotateRefreshToken_WhenValid()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(RefreshTokenAsync_ShouldIssueNewTokens_AndRotateRefreshToken_WhenValid));
        var user = Constants.ExampleUser;
        db.Users.Add(user);
        db.RefreshSessions.Add(new RefreshSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RefreshToken = Constants.Tokens.OldRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow,
            LastUsedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var tokenProvider = TestHelper.MockTokenProvider(accessToken: Constants.Tokens.NewAccessToken, refreshToken: Constants.Tokens.NewRefreshToken);
        var authService = new AuthService(tokenProvider, db, _loggerMock, _configurationMock, _userContextMock, _emailSenderMock);

        var result = await authService.RefreshTokenAsync(
            new RefreshTokenRequestDto(Constants.Tokens.OldRefreshToken),
            CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.AccessToken.ShouldBe(Constants.Tokens.NewAccessToken);
        result.Value.RefreshToken.ShouldBe(Constants.Tokens.NewRefreshToken);

        var session = await db.RefreshSessions.SingleAsync(s => s.UserId == user.Id);
        session.RefreshToken.ShouldBe(Constants.Tokens.NewRefreshToken);
        session.ExpiresAt.ShouldBeGreaterThan(DateTime.UtcNow);
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnError_WhenTokenNotFound()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(RefreshTokenAsync_ShouldReturnError_WhenTokenNotFound));
        var authService = new AuthService(_tokenProviderMock, db, _loggerMock, _configurationMock, _userContextMock, _emailSenderMock);

        var result = await authService.RefreshTokenAsync(
            new RefreshTokenRequestDto(Constants.Tokens.NonExistent),
            CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(AuthErrors.InvalidRefreshToken);
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnError_WhenTokenExpired()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(RefreshTokenAsync_ShouldReturnError_WhenTokenExpired));
        var user = Constants.ExampleUser;
        db.Users.Add(user);
        db.RefreshSessions.Add(new RefreshSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RefreshToken = Constants.Tokens.ExpiredRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1),
            CreatedAt = DateTime.UtcNow,
            LastUsedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var authService = new AuthService(_tokenProviderMock, db, _loggerMock, _configurationMock, _userContextMock, _emailSenderMock);

        var result = await authService.RefreshTokenAsync(
            new RefreshTokenRequestDto(Constants.Tokens.ExpiredRefreshToken),
            CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(AuthErrors.ExpiredRefreshToken);
    }

    [Fact]
    public async Task ForgotPasswordAsync_ShouldGenerateTokenAndReturnCallback_WhenUserExists()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(ForgotPasswordAsync_ShouldGenerateTokenAndReturnCallback_WhenUserExists));
        var user = Constants.ExampleUser;
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var authService = new AuthService(_tokenProviderMock, db, _loggerMock, _configurationMock, _userContextMock, _emailSenderMock);

        var result = await authService.ForgotPasswordAsync(
            new ForgotPasswordRequestDto(user.Email), CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Token.ShouldNotBeEmpty();
        result.Value.Url.ShouldContain("reset-password?email=");
        var tokenEntity = await db.ResetPasswordTokens.SingleOrDefaultAsync(t => t.Email == user.Email);
        tokenEntity.ShouldNotBeNull();
    }

    [Fact]
    public async Task ForgotPasswordAsync_ShouldReturnError_WhenUserDoesNotExist()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(ForgotPasswordAsync_ShouldReturnError_WhenUserDoesNotExist));
        var authService = new AuthService(_tokenProviderMock, db, _loggerMock, _configurationMock, _userContextMock, _emailSenderMock);

        var result = await authService.ForgotPasswordAsync(new ForgotPasswordRequestDto(Constants.User.NotFoundEmail), CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(AuthErrors.UserDoesNotExist(Constants.User.NotFoundEmail));
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldResetPassword_WhenTokenValid()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(ResetPasswordAsync_ShouldResetPassword_WhenTokenValid));
        var user = TestHelper.MakeUser(Constants.User.TestEmail, Constants.Passwords.Strong);
        var token = TestHelper.MakeResetToken(user.Email, used: false, expired: false);

        var originalPassword = user.PasswordHash;

        db.Users.Add(user);
        db.ResetPasswordTokens.Add(token);
        await db.SaveChangesAsync();

        var authService = new AuthService(_tokenProviderMock, db, _loggerMock, _configurationMock, _userContextMock, _emailSenderMock);

        var result = await authService.ResetPasswordAsync(
            new ResetPasswordRequestDto(user.Email, token.Token, Constants.Passwords.New), CancellationToken.None);

        result.IsError.ShouldBeFalse();
        var updatedUser = await db.Users.SingleAsync(u => u.Email == user.Email);
        updatedUser.PasswordHash.ShouldNotBe(originalPassword);
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldReturnError_WhenTokenIsUsed()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(ResetPasswordAsync_ShouldReturnError_WhenTokenIsUsed));
        var user = Constants.ExampleUser;
        var token = TestHelper.MakeResetToken(user.Email, used: true);
        db.Users.Add(user);
        db.ResetPasswordTokens.Add(token);
        await db.SaveChangesAsync();

        var authService = new AuthService(_tokenProviderMock, db, _loggerMock, _configurationMock, _userContextMock, _emailSenderMock);

        var result = await authService.ResetPasswordAsync(new ResetPasswordRequestDto(user.Email, token.Token, Constants.Passwords.New), CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(AuthErrors.TokenAlreadyUsed);
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldReturnError_WhenTokenIsExpired()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(ResetPasswordAsync_ShouldReturnError_WhenTokenIsExpired));
        var user = Constants.ExampleUser;
        var token = TestHelper.MakeResetToken(user.Email, used: false, expired: true);
        db.Users.Add(user);
        db.ResetPasswordTokens.Add(token);
        await db.SaveChangesAsync();

        var authService = new AuthService(_tokenProviderMock, db, _loggerMock, _configurationMock, _userContextMock, _emailSenderMock);

        var result = await authService.ResetPasswordAsync(
            new ResetPasswordRequestDto(user.Email, token.Token, Constants.Passwords.New), CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(AuthErrors.TokenExpired);
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldReturnError_WhenTokenIsInvalid()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(ResetPasswordAsync_ShouldReturnError_WhenTokenIsInvalid));
        var user = Constants.ExampleUser;
        var token = TestHelper.MakeResetToken(user.Email, used: false, expired: false);
        db.Users.Add(user);
        // Do not add the token to db
        await db.SaveChangesAsync();

        var authService = new AuthService(_tokenProviderMock, db, _loggerMock, _configurationMock, _userContextMock, _emailSenderMock);

        var result = await authService.ResetPasswordAsync(
            new ResetPasswordRequestDto(user.Email, token.Token, Constants.Passwords.New), CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(AuthErrors.TokenNotValid);
    }

    [Fact]
    public async Task VerifyEmailAsync_ShouldVerifyEmail_WhenTokenIsValid()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(VerifyEmailAsync_ShouldVerifyEmail_WhenTokenIsValid));
        var user = Constants.ExampleUser;
        var token = TestHelper.MakeVerifyEmailToken(user.Email, used: false, expired: false);

        db.Users.Add(user);
        db.VerifyEmailTokens.Add(token);
        await db.SaveChangesAsync();

        var service = new AuthService(_tokenProviderMock, db, _loggerMock, _configurationMock, _userContextMock, _emailSenderMock);

        var result = await service.VerifyEmailAsync(new VerifyEmailRequestDto(user.Email, token.Token), CancellationToken.None);

        result.IsError.ShouldBeFalse();

        var updatedUser = await db.Users.SingleAsync(u => u.Email == user.Email);
        updatedUser.EmailVerified.ShouldBeTrue();

        var updatedToken = await db.VerifyEmailTokens.SingleAsync(t => t.Email == user.Email);
        updatedToken.UsedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task VerifyEmailAsync_ShouldReturnError_WhenUserNotFound()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(VerifyEmailAsync_ShouldReturnError_WhenUserNotFound));
        var token = TestHelper.MakeVerifyEmailToken(Constants.User.NotFoundEmail);

        db.VerifyEmailTokens.Add(token);
        await db.SaveChangesAsync();

        var service = new AuthService(_tokenProviderMock, db, _loggerMock, _configurationMock, _userContextMock, _emailSenderMock);

        var result = await service.VerifyEmailAsync(
            new VerifyEmailRequestDto(Constants.User.NotFoundEmail, token.Token),
            CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(AuthErrors.UserDoesNotExist(Constants.User.NotFoundEmail));
    }

    [Fact]
    public async Task VerifyEmailAsync_ShouldReturnError_WhenTokenNotFound()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(VerifyEmailAsync_ShouldReturnError_WhenTokenNotFound));
        var user = Constants.ExampleUser;
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var service = new AuthService(_tokenProviderMock, db, _loggerMock, _configurationMock, _userContextMock, _emailSenderMock);

        var result = await service.VerifyEmailAsync(
            new VerifyEmailRequestDto(user.Email, Constants.Tokens.Invalid),
            CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(AuthErrors.TokenNotValid);
    }

    [Fact]
    public async Task VerifyEmailAsync_ShouldReturnError_WhenTokenAlreadyUsed()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(VerifyEmailAsync_ShouldReturnError_WhenTokenAlreadyUsed));
        var user = Constants.ExampleUser;
        var token = TestHelper.MakeVerifyEmailToken(user.Email, used: true);

        db.Users.Add(user);
        db.VerifyEmailTokens.Add(token);
        await db.SaveChangesAsync();

        var service = new AuthService(_tokenProviderMock, db, _loggerMock, _configurationMock, _userContextMock, _emailSenderMock);

        var result = await service.VerifyEmailAsync(
            new VerifyEmailRequestDto(user.Email, token.Token),
            CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(AuthErrors.TokenAlreadyUsed);
    }

    [Fact]
    public async Task VerifyEmailAsync_ShouldReturnError_WhenTokenExpired()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(VerifyEmailAsync_ShouldReturnError_WhenTokenExpired));
        var user = Constants.ExampleUser;
        var token = TestHelper.MakeVerifyEmailToken(user.Email, expired: true);

        db.Users.Add(user);
        db.VerifyEmailTokens.Add(token);
        await db.SaveChangesAsync();

        var service = new AuthService(_tokenProviderMock, db, _loggerMock, _configurationMock, _userContextMock, _emailSenderMock);

        var result = await service.VerifyEmailAsync(
            new VerifyEmailRequestDto(user.Email, token.Token),
            CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(AuthErrors.TokenExpired);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnError_WhenEmailNotVerified()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(LoginAsync_ShouldReturnError_WhenEmailNotVerified));
        var user = TestHelper.MakeUser(Constants.User.Email, Constants.Passwords.Strong, verified: false);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var authService = new AuthService(_tokenProviderMock, db, _loggerMock, _configurationMock, _userContextMock, _emailSenderMock);

        var result = await authService.LoginAsync(
            new LoginRequestDto(Constants.User.Email, Constants.Passwords.Strong),
            CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(AuthErrors.LoginEmailNotVerified);
    }

    [Fact]
    public async Task ListSessionsAsync_ShouldReturnSessions_WhenUserHasSessions()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(ListSessionsAsync_ShouldReturnSessions_WhenUserHasSessions));
        var user = Constants.ExampleUser;
        var sessionId = Guid.NewGuid();
        db.Users.Add(user);
        db.RefreshSessions.Add(new RefreshSession
        {
            Id = sessionId,
            UserId = user.Id,
            RefreshToken = Constants.Tokens.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow,
            LastUsedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var userContext = TestHelper.MockIdentityUserContext(user.Id, sessionId);
        var authService = new AuthService(_tokenProviderMock, db, _loggerMock, _configurationMock, userContext, _emailSenderMock);

        var result = await authService.ListSessionsAsync(CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Sessions.Count.ShouldBe(1);
        result.Value.Sessions[0].Id.ShouldBe(sessionId);
        result.Value.Sessions[0].IsCurrent.ShouldBeTrue();
    }

    [Fact]
    public async Task RevokeSessionAsync_ShouldRemoveSession_WhenValid()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(RevokeSessionAsync_ShouldRemoveSession_WhenValid));
        var user = Constants.ExampleUser;
        var sessionId = Guid.NewGuid();
        db.Users.Add(user);
        db.RefreshSessions.Add(new RefreshSession
        {
            Id = sessionId,
            UserId = user.Id,
            RefreshToken = Constants.Tokens.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow,
            LastUsedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var userContext = TestHelper.MockIdentityUserContext(user.Id, null);
        var authService = new AuthService(_tokenProviderMock, db, _loggerMock, _configurationMock, userContext, _emailSenderMock);

        var result = await authService.RevokeSessionAsync(sessionId, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        (await db.RefreshSessions.AnyAsync(s => s.Id == sessionId)).ShouldBeFalse();
    }

    [Fact]
    public async Task RevokeSessionAsync_ShouldReturnError_WhenSessionNotFound()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(RevokeSessionAsync_ShouldReturnError_WhenSessionNotFound));
        var userContext = TestHelper.MockIdentityUserContext(Constants.User.Id, null);
        var authService = new AuthService(_tokenProviderMock, db, _loggerMock, _configurationMock, userContext, _emailSenderMock);

        var result = await authService.RevokeSessionAsync(Guid.NewGuid(), CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(AuthErrors.SessionNotFound);
    }

    [Fact]
    public async Task RevokeAllOtherSessionsAsync_ShouldRemoveOtherSessions_WhenValid()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(RevokeAllOtherSessionsAsync_ShouldRemoveOtherSessions_WhenValid));
        var user = Constants.ExampleUser;
        var currentSessionId = Guid.NewGuid();
        var otherSessionId = Guid.NewGuid();
        db.Users.Add(user);
        db.RefreshSessions.AddRange(
            new RefreshSession
            {
                Id = currentSessionId,
                UserId = user.Id,
                RefreshToken = Constants.Tokens.RefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                CreatedAt = DateTime.UtcNow,
                LastUsedAt = DateTime.UtcNow
            },
            new RefreshSession
            {
                Id = otherSessionId,
                UserId = user.Id,
                RefreshToken = Constants.Tokens.OldRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                CreatedAt = DateTime.UtcNow,
                LastUsedAt = DateTime.UtcNow
            });
        await db.SaveChangesAsync();

        var userContext = TestHelper.MockIdentityUserContext(user.Id, currentSessionId);
        var authService = new AuthService(_tokenProviderMock, db, _loggerMock, _configurationMock, userContext, _emailSenderMock);

        var result = await authService.RevokeAllOtherSessionsAsync(CancellationToken.None);

        result.IsError.ShouldBeFalse();
        (await db.RefreshSessions.AnyAsync(s => s.Id == currentSessionId)).ShouldBeTrue();
        (await db.RefreshSessions.AnyAsync(s => s.Id == otherSessionId)).ShouldBeFalse();
    }

    [Fact]
    public async Task RevokeAllOtherSessionsAsync_ShouldReturnError_WhenNoCurrentSession()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(RevokeAllOtherSessionsAsync_ShouldReturnError_WhenNoCurrentSession));
        var userContext = TestHelper.MockIdentityUserContext(Constants.User.Id, null);
        var authService = new AuthService(_tokenProviderMock, db, _loggerMock, _configurationMock, userContext, _emailSenderMock);

        var result = await authService.RevokeAllOtherSessionsAsync(CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(AuthErrors.SessionNotFound);
    }

    [Fact]
    public async Task LogoutAsync_ShouldRemoveSession_WhenValidRefreshToken()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(LogoutAsync_ShouldRemoveSession_WhenValidRefreshToken));
        var user = Constants.ExampleUser;
        var sessionId = Guid.NewGuid();
        db.Users.Add(user);
        db.RefreshSessions.Add(new RefreshSession
        {
            Id = sessionId,
            UserId = user.Id,
            RefreshToken = Constants.Tokens.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow,
            LastUsedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var authService = new AuthService(_tokenProviderMock, db, _loggerMock, _configurationMock, _userContextMock, _emailSenderMock);

        var result = await authService.LogoutAsync(new LogoutRequestDto(Constants.Tokens.RefreshToken), CancellationToken.None);

        result.IsError.ShouldBeFalse();
        (await db.RefreshSessions.AnyAsync(s => s.RefreshToken == Constants.Tokens.RefreshToken)).ShouldBeFalse();
    }

    [Fact]
    public async Task LogoutAsync_ShouldReturnError_WhenInvalidRefreshToken()
    {
        await using var db = TestHelper.CreateInMemoryContext(nameof(LogoutAsync_ShouldReturnError_WhenInvalidRefreshToken));
        var authService = new AuthService(_tokenProviderMock, db, _loggerMock, _configurationMock, _userContextMock, _emailSenderMock);

        var result = await authService.LogoutAsync(new LogoutRequestDto(Constants.Tokens.NonExistent), CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(AuthErrors.InvalidRefreshToken);
    }
}


