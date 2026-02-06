using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PantryCloud.Identity.Application;
using PantryCloud.Identity.Core;
using PantryCloud.Identity.Core.Entities;
using PantryCloud.Identity.Infrastructure;
using PantryCloud.Identity.Infrastructure.Persistence;
using PantryCloud.Identity.Infrastructure.Services;

namespace PantryCloud.Identity.UnitTests;

internal static class TestHelper
{
    public static ApplicationDbContext CreateInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName + "_" + Guid.NewGuid())
            .Options;
        return new ApplicationDbContext(options);
    }

    public static ITokenProvider MockTokenProvider(string accessToken = "ACCESS_TOKEN",
        string refreshToken = "REFRESH_TOKEN",
        string passwordResetToken = "PASSWORD_RESET_TOKEN")
    {
        var tp = Substitute.For<ITokenProvider>();
        tp.CreateAccessToken(Arg.Any<ApplicationUser>(), Arg.Any<Guid?>()).Returns(accessToken);
        tp.CreateRefreshToken().Returns(refreshToken);
        tp.CreatePasswordResetToken().Returns(passwordResetToken);

        return tp;
    }

    public static IIdentityUserContext MockIdentityUserContext(Guid? userId = null, Guid? sessionId = null)
    {
        var ctx = Substitute.For<IIdentityUserContext>();
        ctx.UserId.Returns(userId ?? Constants.User.Id);
        ctx.Email.Returns(Constants.User.Email);
        ctx.SessionId.Returns(sessionId);
        return ctx;
    }

    public static ApiConfiguration MockConfiguration()
    {
        var tp = new ApiConfiguration();
        return tp;
    }

    public static ILogger<AuthService> MockLogger()
        => Substitute.For<ILogger<AuthService>>();

    public static ApplicationUser MakeUser(string email, string password, bool verified = false)
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            EmailVerified = verified,
            PasswordHash = PasswordHasher.Hash(password)
        };
    }
    
    public static ResetPasswordToken MakeResetToken(string email, bool used = false, bool expired = false)
    {
        return new ResetPasswordToken
        {
            Id = Guid.NewGuid(),
            Email = email,
            Token = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expired ? DateTime.UtcNow.AddMinutes(-5) : DateTime.UtcNow.AddMinutes(10),
            UsedAt = used ? DateTime.UtcNow : null
        };
    }
    
    public static VerifyEmailToken MakeVerifyEmailToken(string email, bool used = false, bool expired = false)
    {
        return new VerifyEmailToken
        {
            Id = Guid.NewGuid(),
            Email = email,
            Token = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expired ? DateTime.UtcNow.AddMinutes(-5) : DateTime.UtcNow.AddMinutes(10),
            UsedAt = used ? DateTime.UtcNow : null
        };
    }
}