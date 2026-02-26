using System.Data.Common;
using System.Security.Cryptography;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
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
    private IContainer _identityApi = null!;
    private string? _secretsTempPath;
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
    }

    public async Task InitializeAsync()
    {
        await _network.CreateAsync();
        await _postgres.StartAsync();

        await PostgresDatabaseSetup.CreateDatabaseAsync(_postgres, IntegrationConstants.Postgres.IdentityDatabase, IntegrationConstants.Postgres.User, IntegrationConstants.Postgres.DefaultDatabase);
        await PostgresDatabaseSetup.ApplyMigrationsAsync<ApplicationDbContext>(_postgres, IntegrationConstants.Postgres.IdentityDatabase, IntegrationConstants.Postgres.User, IntegrationConstants.Postgres.Password, IntegrationConstants.Postgres.Port);

        _secretsTempPath = Path.Combine(Path.GetTempPath(), "pantrycloud-identity-test-" + Guid.NewGuid().ToString("N"));
        EnsureTestJwtKeys(_secretsTempPath);

        await IdentityImageBuilder.BuildAsync();

        var connectionString = $"Host={IntegrationConstants.Postgres.Host};Port={IntegrationConstants.Postgres.Port};Database={IntegrationConstants.Postgres.IdentityDatabase};Username={IntegrationConstants.Postgres.User};Password={IntegrationConstants.Postgres.Password}";
        _identityApi = IdentityContainer.Create(
            IdentityImageBuilder.ImageName,
            _network,
            connectionString,
            _secretsTempPath
        ).Build();
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
        var session = new RefreshSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RefreshToken = refreshToken,
            ExpiresAt = expired ? DateTime.UtcNow.AddMinutes(-1) : DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            LastUsedAt = DateTime.UtcNow
        };
        await context.RefreshSessions.AddAsync(session);
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

    public async Task<RefreshSession?> GetSessionByRefreshTokenAsync(string refreshToken)
    {
        var options = CreateDbContextOptions();
        await using var context = new ApplicationDbContext(options);
        return await context.RefreshSessions.FirstOrDefaultAsync(s => s.RefreshToken == refreshToken);
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

    /// <summary>
    /// Creates a temp directory with private.pem and public.pem for the Identity container bind mount.
    /// Writes via temp file + atomic move to avoid partial reads if tests run concurrently.
    /// Caller is responsible for deleting the directory in DisposeAsync.
    /// </summary>
    private static void EnsureTestJwtKeys(string secretsPath)
    {
        Directory.CreateDirectory(secretsPath);
        var privatePath = Path.Combine(secretsPath, "private.pem");
        var publicPath = Path.Combine(secretsPath, "public.pem");
        using var rsa = RSA.Create(2048);
        var privatePem = rsa.ExportRSAPrivateKeyPem();
        var publicPem = rsa.ExportRSAPublicKeyPem();
        WriteAtomic(privatePath, privatePem);
        WriteAtomic(publicPath, publicPem);
    }

    private static void WriteAtomic(string targetPath, string contents)
    {
        var tempPath = targetPath + ".tmp." + Guid.NewGuid().ToString("N");
        try
        {
            File.WriteAllText(tempPath, contents);
            File.Move(tempPath, targetPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(tempPath))
                try { File.Delete(tempPath); } catch { /* ignore */ }
        }
    }

    public async Task DisposeAsync()
    {
        await _identityApi.DisposeAsync();
        await _postgres.DisposeAsync();
        await _network.DisposeAsync();
        if (_secretsTempPath != null && Directory.Exists(_secretsTempPath))
        {
            try
            {
                foreach (var file in Directory.EnumerateFiles(_secretsTempPath))
                    File.Delete(file);
                Directory.Delete(_secretsTempPath);
            }
            catch { /* best-effort cleanup */ }
        }
    }
}
