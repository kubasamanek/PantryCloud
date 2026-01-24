using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PantryCloud.Identity.Core.Entities;
using PantryCloud.Identity.Infrastructure;
using PantryCloud.Identity.Infrastructure.Persistence;
using PantryCloud.SharedKernel.Testing.Infrastructure;

namespace PantryCloud.Identity.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for Identity service integration tests.
/// Provides Identity-specific helper methods for seeding and querying data.
/// </summary>
[Collection(nameof(IntegrationTestCollection))]
public abstract class BaseIntegrationTest(IdentityIntegrationTestWebAppFactory factory) 
    : BaseIntegrationTest<Program, ApplicationDbContext>(factory)
{

    /// <summary>
    /// Seeds a user into the database.
    /// </summary>
    protected async Task<ApplicationUser> SeedUserAsync(string email, string password, bool verified = true)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = new ApplicationUser
        {
            Email = email,
            EmailVerified = verified,
            PasswordHash = PasswordHasher.Hash(password)
        };

        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();

        return user;
    }

    /// <summary>
    /// Seeds a refresh token for a user.
    /// </summary>
    protected async Task<string> SeedRefreshTokenAsync(Guid userId, bool expired = false)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = await dbContext.Users.FindAsync(userId);
        if (user == null)
            throw new InvalidOperationException($"User with ID {userId} not found");

        var refreshToken = Guid.NewGuid().ToString();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = expired 
            ? DateTime.UtcNow.AddMinutes(-1) 
            : DateTime.UtcNow.AddDays(7);

        await dbContext.SaveChangesAsync();

        return refreshToken;
    }

    /// <summary>
    /// Seeds a verify email token for a user.
    /// </summary>
    protected async Task<VerifyEmailToken> SeedVerifyEmailTokenAsync(string email, bool used = false, bool expired = false)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var token = new VerifyEmailToken
        {
            Email = email,
            Token = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expired 
                ? DateTime.UtcNow.AddMinutes(-5) 
                : DateTime.UtcNow.AddMinutes(60),
            UsedAt = used ? DateTime.UtcNow : null
        };

        await dbContext.VerifyEmailTokens.AddAsync(token);
        await dbContext.SaveChangesAsync();

        return token;
    }

    /// <summary>
    /// Seeds a reset password token for a user.
    /// </summary>
    protected async Task<ResetPasswordToken> SeedResetPasswordTokenAsync(string email, bool used = false, bool expired = false)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var token = new ResetPasswordToken
        {
            Email = email,
            Token = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expired 
                ? DateTime.UtcNow.AddMinutes(-5) 
                : DateTime.UtcNow.AddMinutes(60),
            UsedAt = used ? DateTime.UtcNow : null,
            CallBackUrl = $"http://localhost:5019/reset-password?email={email}&token=test"
        };

        await dbContext.ResetPasswordTokens.AddAsync(token);
        await dbContext.SaveChangesAsync();

        return token;
    }


    /// <summary>
    /// Gets a user from the database by email.
    /// </summary>
    protected async Task<ApplicationUser?> GetUserByEmailAsync(string email)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    /// <summary>
    /// Gets a user from the database by ID.
    /// </summary>
    protected async Task<ApplicationUser?> GetUserByIdAsync(Guid userId)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await dbContext.Users.FindAsync(userId);
    }

    /// <summary>
    /// Gets a verify email token from the database.
    /// </summary>
    protected async Task<VerifyEmailToken?> GetVerifyEmailTokenAsync(string email, string token)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await dbContext.VerifyEmailTokens
            .FirstOrDefaultAsync(t => t.Email == email && t.Token == token);
    }

    /// <summary>
    /// Gets a reset password token from the database.
    /// </summary>
    protected async Task<ResetPasswordToken?> GetResetPasswordTokenAsync(string email, string token)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await dbContext.ResetPasswordTokens
            .FirstOrDefaultAsync(t => t.Email == email && t.Token == token);
    }
}

