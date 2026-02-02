using System.Net.Mail;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PantryCloud.Identity.Application;
using PantryCloud.Identity.Application.DTOs;
using PantryCloud.Identity.Core;
using PantryCloud.Identity.Core.Entities;
using PantryCloud.Identity.Core.Errors;
using PantryCloud.Identity.Infrastructure.Persistence;

namespace PantryCloud.Identity.Infrastructure.Services;

public class AuthService(
    ITokenProvider tokenProvider,
    ApplicationDbContext dbContext,
    ILogger<AuthService> logger,
    ApiConfiguration config,
    IIdentityUserContext userContext) : IAuthService
{
    public async Task<ErrorOr<RegisterResponseDto>> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to register user with email: {Email}", request.Email);

        if (await dbContext.Users.Exists(request.Email))
        {
            logger.LogWarning("Registration failed. User with email {Email} already exists.", request.Email);
            return AuthErrors.RegistrationUserAlreadyExists(request.Email);
        }

        var user = new ApplicationUser
        {
            Email = request.Email,
            EmailVerified = false,
            PasswordHash = PasswordHasher.Hash(request.Password),
        };

        var verifyEmailTokenString = tokenProvider.CreateVerifyEmailToken();

        var tokenEntity = new VerifyEmailToken
        {
            CreatedAt = DateTime.UtcNow,
            Email = request.Email,
            ExpiresAt = DateTime.UtcNow.AddMinutes(config.App.VerifyEmailTokenExpirationInMinutes),
            Token = verifyEmailTokenString
        };

        await dbContext.Users.AddAsync(user, cancellationToken);
        await dbContext.VerifyEmailTokens.AddAsync(tokenEntity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("User registered successfully with ID: {UserId}", user.Id);
        
        // TODO: Send confirmation email

        return new RegisterResponseDto(user.Id.ToString(), verifyEmailTokenString);
    }

    public async Task<ErrorOr<LoginResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken)
    {
        logger.LogInformation("User login attempt: {Email}", request.Email);

        var user = await dbContext.Users.GetByEmail(request.Email);

        if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
        {
            logger.LogWarning("Login failed for email: {Email}", request.Email);
            return AuthErrors.LoginInvalidCredentials;
        }

        if (!user.EmailVerified)
        {
            logger.LogWarning("Login failed for email: {Email} (not verified)", request.Email);
            return AuthErrors.LoginEmailNotVerified;
        }

        var refreshToken = tokenProvider.CreateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        var session = new RefreshSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            LastUsedAt = DateTime.UtcNow,
            DeviceName = request.DeviceName != null ? request.DeviceName.Length > 200 ? request.DeviceName[..200] : request.DeviceName : null
        };
        await dbContext.RefreshSessions.AddAsync(session, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var accessToken = tokenProvider.CreateAccessToken(user, session.Id);
        logger.LogInformation("User {UserId} logged in successfully", user.Id);

        return new LoginResponseDto(accessToken, refreshToken, session.Id);
    }

    public async Task<ErrorOr<RefreshTokenResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to refresh token");

        var session = await dbContext.RefreshSessions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.RefreshToken == request.RefreshToken, cancellationToken);

        if (session?.User == null)

        if (session?.User == null)
        {
            logger.LogWarning("Refresh token failed: token not found");
            return AuthErrors.InvalidRefreshToken;
        }

        var user = session.User;

        if (session.ExpiresAt < DateTime.UtcNow)
        {
            logger.LogWarning("Refresh token expired for user {UserId}", user.Id);
            return AuthErrors.ExpiredRefreshToken;
        }

        var newRefreshToken = tokenProvider.CreateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        session.RefreshToken = newRefreshToken;
        session.ExpiresAt = expiresAt;
        session.LastUsedAt = DateTime.UtcNow;
        if (request.DeviceName != null)
        {
            session.DeviceName = request.DeviceName.Length > 200 ? request.DeviceName[..200] : request.DeviceName;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var accessToken = tokenProvider.CreateAccessToken(user, session.Id);
        logger.LogInformation("Refresh token succeeded for user {UserId}", user.Id);

        return new RefreshTokenResponseDto(accessToken, newRefreshToken, session.Id);
    }

    public async Task<ErrorOr<ForgotPasswordResponseDto>> ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken cancellationToken)
    {
        logger.LogInformation("User {Email} forgot password.", request.Email);
        
        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        
        if (user == null)
        {
            logger.LogWarning("User {Email} does not exist", request.Email);
            return AuthErrors.UserDoesNotExist(request.Email);
        }

        var token = tokenProvider.CreatePasswordResetToken();
        var callbackUrl = $"{config.App.FrontendUrl}/reset-password?email={user.Email}&token={Uri.EscapeDataString(token)}";

        var entity = new ResetPasswordToken
        {
            Email = request.Email,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(config.App.ResetPasswordTokenExpirationInMinutes),
            Token = token,
            CallBackUrl = callbackUrl
        };
        
        await dbContext.ResetPasswordTokens.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (config.App.SendEmails)
        {
            using var client = new SmtpClient(config.Email.Host, config.Email.Port);
            using var message = new MailMessage();
            
            message.From = new MailAddress(config.Email.From);
            message.Subject = Constants.ResetPasswordEmailSubject;
            message.Body = string.Format(Constants.ResetPasswordEmailBodyTemplate, callbackUrl);
            message.IsBodyHtml = true;
            message.To.Add(user.Email);

            await client.SendMailAsync(message, cancellationToken);
        }

        return new ForgotPasswordResponseDto(token, callbackUrl);
    }

    public async Task<ErrorOr<ResetPasswordResponseDto>> ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken)
    {
        logger.LogInformation("User {Email} is trying to reset password.", request.Email);
                
        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        
        if (user is null)
        {
            logger.LogWarning("Reset password failed: user not found");
            return AuthErrors.UserDoesNotExist(request.Email);
        }
        
        var token = await dbContext.ResetPasswordTokens
            .FirstOrDefaultAsync(t => t.Email == request.Email && t.Token == request.Token, cancellationToken);

        if (token is null)
        {
            logger.LogWarning("Reset password failed: token not found");
            return AuthErrors.TokenNotValid;
        }
        
        if (token.IsUsed)
        {
            logger.LogWarning("Reset password failed: token has been used");
            return AuthErrors.TokenAlreadyUsed;
        }
        
        if (token.IsExpired)
        {
            logger.LogWarning("Reset password failed: token has expired");
            return AuthErrors.TokenExpired;
        }

        user.PasswordHash = PasswordHasher.Hash(request.NewPassword);
        token.UsedAt =  DateTime.UtcNow;
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("User {Email} successfully reset password", request.Email);

        // TODO: Token cleanup (hosted service/cron job)
        
        return new ResetPasswordResponseDto();
    }

    public async Task<ErrorOr<VerifyEmailResponseDto>> VerifyEmailAsync(VerifyEmailRequestDto request, CancellationToken cancellationToken)
    {
        logger.LogInformation("User {Email} is trying to verify email.", request.Email);
                
        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        
        if (user is null)
        {
            logger.LogWarning("Verify email failed: user not found");
            return AuthErrors.UserDoesNotExist(request.Email);
        }
        
        var token = await dbContext.VerifyEmailTokens
            .FirstOrDefaultAsync(t => t.Email == request.Email && t.Token == request.Token, cancellationToken);

        if (token is null)
        {
            logger.LogWarning("Verify email failed: token not found");
            return AuthErrors.TokenNotValid;
        }
        
        if (token.IsUsed)
        {
            logger.LogWarning("Verify email failed: token has been used");
            return AuthErrors.TokenAlreadyUsed;
        }
        
        if (token.IsExpired)
        {
            logger.LogWarning("Verify email failed: token has expired");
            return AuthErrors.TokenExpired;
        }
        
        user.EmailVerified = true;
        token.UsedAt = DateTime.UtcNow;
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("User {Email} successfully verified email.", request.Email);
        
        return new VerifyEmailResponseDto();
    }

    public async Task<ErrorOr<ListSessionsResponseDto>> ListSessionsAsync(CancellationToken cancellationToken = default)
    {
        var userId = userContext.UserId;
        var currentSessionId = userContext.SessionId;

        var sessions = await dbContext.RefreshSessions
            .AsNoTracking()
            .Where(s => s.UserId == userId && s.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(s => s.LastUsedAt ?? s.CreatedAt)
            .Select(s => new SessionDto(
                s.Id,
                s.CreatedAt,
                s.LastUsedAt,
                s.DeviceName,
                currentSessionId == s.Id))
            .ToListAsync(cancellationToken);

        return new ListSessionsResponseDto(sessions);
    }

    public async Task<ErrorOr<Unit>> RevokeSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var userId = userContext.UserId;

        var session = await dbContext.RefreshSessions.FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId, cancellationToken);
        if (session == null)
        {
            return AuthErrors.SessionNotFound;
        }

        dbContext.RefreshSessions.Remove(session);
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("User {UserId} revoked session {SessionId}", userId, sessionId);
        return Unit.Value;
    }

    public async Task<ErrorOr<Unit>> RevokeAllOtherSessionsAsync(CancellationToken cancellationToken = default)
    {
        var userId = userContext.UserId;
        var currentSessionId = userContext.SessionId;

        if (!currentSessionId.HasValue)
        {
            return AuthErrors.SessionNotFound;
        }

        var otherSessions = await dbContext.RefreshSessions
            .Where(s => s.UserId == userId && s.Id != currentSessionId.Value)
            .ToListAsync(cancellationToken);

        dbContext.RefreshSessions.RemoveRange(otherSessions);
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("User {UserId} revoked {Count} other sessions", userId, otherSessions.Count);
        return Unit.Value;
    }

    public async Task<ErrorOr<Unit>> LogoutAsync(LogoutRequestDto request, CancellationToken cancellationToken = default)
    {
        var session = await dbContext.RefreshSessions
            .FirstOrDefaultAsync(s => s.RefreshToken == request.RefreshToken, cancellationToken);

        if (session == null)
        {
            return AuthErrors.InvalidRefreshToken;
        }

        dbContext.RefreshSessions.Remove(session);
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("User logged out, session {SessionId} revoked", session.Id);
        return Unit.Value;
    }
}