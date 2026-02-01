using System.Data.Common;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using DotNet.Testcontainers.Networks;
using Microsoft.EntityFrameworkCore;
using PantryCloud.Identity.Core.Entities;
using PantryCloud.Identity.Infrastructure;
using PantryCloud.Identity.Infrastructure.Persistence;
using PantryCloud.Identity.IntegrationTests.Infrastructure.Containers;
using PantryCloud.Identity.IntegrationTests.Infrastructure.Images;
using PantryCloud.SharedKernel.Testing.Infrastructure.Environment;
using Respawn;
using Testcontainers.PostgreSql;

namespace PantryCloud.Identity.IntegrationTests.Infrastructure;

public sealed class IdentityTestFixture : IAsyncLifetime
{
    private readonly INetwork _network;
    private readonly PostgreSqlContainer _postgres;
    private readonly IFutureDockerImage _identityImage;
    private readonly IContainer _identityApi;
    private Respawner _respawner = null!;

    public HttpClient HttpClient { get; private set; } = null!;
    public string BaseAddress { get; private set; } = null!;

    private string ConnectionString => $"Host=127.0.0.1;Port={_postgres.GetMappedPublicPort(IntegrationConstants.Postgres.Port)};Database={IntegrationConstants.Postgres.IdentityDatabase};Username={IntegrationConstants.Postgres.User};Password={IntegrationConstants.Postgres.Password}";

    public IdentityTestFixture()
    {
        _network = new NetworkBuilder()
            .WithName(Guid.NewGuid().ToString("D"))
            .Build();

        _postgres = PostgresContainer.Create(_network).Build();
        _identityImage = IdentityImageBuilder.Build();
        var secretsPath = SolutionPathHelper.GetPathFromSolutionRoot("src", "Services", "PantryCloud.Identity", "PantryCloud.Identity.Presentation", "Secrets");
        _identityApi = IdentityContainer.Create(
            _identityImage,
            _network,
            $"Host={IntegrationConstants.Postgres.Host};Port={IntegrationConstants.Postgres.Port};Database={IntegrationConstants.Postgres.IdentityDatabase};Username={IntegrationConstants.Postgres.User};Password={IntegrationConstants.Postgres.Password}",
            secretsPath
        ).Build();
    }

    public async Task InitializeAsync()
    {
        await _network.CreateAsync();
        await _postgres.StartAsync();

        await PostgresDatabaseSetup.CreateDatabaseAsync(_postgres, IntegrationConstants.Postgres.IdentityDatabase, IntegrationConstants.Postgres.User, IntegrationConstants.Postgres.DefaultDatabase);
        await PostgresDatabaseSetup.ApplyMigrationsAsync<ApplicationDbContext>(_postgres, IntegrationConstants.Postgres.IdentityDatabase, IntegrationConstants.Postgres.User, IntegrationConstants.Postgres.Password, IntegrationConstants.Postgres.Port);

        await _identityImage.CreateAsync();
        await _identityApi.StartAsync();

        BaseAddress = $"http://127.0.0.1:{_identityApi.GetMappedPublicPort(IntegrationConstants.Identity.Port)}";
        HttpClient = new HttpClient { BaseAddress = new Uri(BaseAddress) };

        await InitializeRespawnerAsync();
    }

    private async Task InitializeRespawnerAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        await using var context = new ApplicationDbContext(options);
        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore = ["__EFMigrationsHistory"]
        });
    }

    public async Task ResetDatabaseAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        await using var context = new ApplicationDbContext(options);
        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    public async Task<ApplicationUser> SeedUserAsync(string email, string password, bool verified = true)
    {
        var options = CreateDbContextOptions();
        await using var context = new ApplicationDbContext(options);
        var user = new ApplicationUser
        {
            Email = email,
            EmailVerified = verified,
            PasswordHash = PasswordHasher.Hash(password)
        };
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        return user;
    }

    public async Task<string> SeedRefreshTokenAsync(Guid userId, bool expired = false)
    {
        var options = CreateDbContextOptions();
        await using var context = new ApplicationDbContext(options);
        var user = await context.Users.FindAsync(userId)
            ?? throw new InvalidOperationException($"User with ID {userId} not found");
        var refreshToken = Guid.NewGuid().ToString();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = expired ? DateTime.UtcNow.AddMinutes(-1) : DateTime.UtcNow.AddDays(7);
        await context.SaveChangesAsync();
        return refreshToken;
    }

    public async Task<VerifyEmailToken> SeedVerifyEmailTokenAsync(string email, bool used = false, bool expired = false)
    {
        var options = CreateDbContextOptions();
        await using var context = new ApplicationDbContext(options);
        var token = new VerifyEmailToken
        {
            Email = email,
            Token = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expired ? DateTime.UtcNow.AddMinutes(-5) : DateTime.UtcNow.AddMinutes(60),
            UsedAt = used ? DateTime.UtcNow : null
        };
        await context.VerifyEmailTokens.AddAsync(token);
        await context.SaveChangesAsync();
        return token;
    }

    public async Task<ResetPasswordToken> SeedResetPasswordTokenAsync(string email, bool used = false, bool expired = false)
    {
        var options = CreateDbContextOptions();
        await using var context = new ApplicationDbContext(options);
        var token = new ResetPasswordToken
        {
            Email = email,
            Token = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expired ? DateTime.UtcNow.AddMinutes(-5) : DateTime.UtcNow.AddMinutes(60),
            UsedAt = used ? DateTime.UtcNow : null,
            CallBackUrl = $"http://localhost:5019/reset-password?email={email}&token=test"
        };
        await context.ResetPasswordTokens.AddAsync(token);
        await context.SaveChangesAsync();
        return token;
    }

    public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
    {
        var options = CreateDbContextOptions();
        await using var context = new ApplicationDbContext(options);
        return await context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(Guid userId)
    {
        var options = CreateDbContextOptions();
        await using var context = new ApplicationDbContext(options);
        return await context.Users.FindAsync(userId);
    }

    public async Task<VerifyEmailToken?> GetVerifyEmailTokenAsync(string email, string token)
    {
        var options = CreateDbContextOptions();
        await using var context = new ApplicationDbContext(options);
        return await context.VerifyEmailTokens.FirstOrDefaultAsync(t => t.Email == email && t.Token == token);
    }

    public async Task<ResetPasswordToken?> GetResetPasswordTokenAsync(string email, string token)
    {
        var options = CreateDbContextOptions();
        await using var context = new ApplicationDbContext(options);
        return await context.ResetPasswordTokens.FirstOrDefaultAsync(t => t.Email == email && t.Token == token);
    }

    public async Task<IReadOnlyList<ResetPasswordToken>> GetResetPasswordTokensByEmailAsync(string email)
    {
        var options = CreateDbContextOptions();
        await using var context = new ApplicationDbContext(options);
        return await context.ResetPasswordTokens.Where(t => t.Email == email).ToListAsync();
    }

    private DbContextOptions<ApplicationDbContext> CreateDbContextOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
    }

    public async Task DisposeAsync()
    {
        await _identityApi.DisposeAsync();
        await _postgres.DisposeAsync();
        await _network.DisposeAsync();
    }
}
