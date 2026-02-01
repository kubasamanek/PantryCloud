using System.Data.Common;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Xunit;

namespace PantryCloud.SharedKernel.Testing.Infrastructure;

/// <summary>
/// Base class for integration tests that provides database cleanup and common infrastructure.
/// Inherit from this in your service-specific base test class.
/// </summary>
/// <typeparam name="TProgram">The Program class of the service under test</typeparam>
/// <typeparam name="TDbContext">The DbContext type for the service</typeparam>
public abstract class BaseIntegrationTest<TProgram, TDbContext>(
    IntegrationTestWebAppFactory<TProgram, TDbContext> factory) : IAsyncLifetime
    where TProgram : class
    where TDbContext : DbContext
{
    protected readonly HttpClient HttpClient = factory.CreateClient();
    private Respawner _respawner = null!;

    public async Task InitializeAsync()
    {
        // Initialize Respawner for database cleanup
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var connection = dbContext.Database.GetDbConnection();
        
        await connection.OpenAsync();
        
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore = ["__EFMigrationsHistory"]
        });
    }

    public async Task DisposeAsync()
    {
        await ResetDatabaseAsync();
    }

    /// <summary>
    /// Resets the database to a clean state between tests.
    /// </summary>
    private async Task ResetDatabaseAsync()
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var connection = dbContext.Database.GetDbConnection();
        
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    /// <summary>
    /// Deserializes JSON response to the specified type.
    /// </summary>
    protected async Task<T?> GetFromJsonAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    /// <summary>
    /// Gets a database connection for executing raw SQL or using with Respawner.
    /// </summary>
    protected async Task<DbConnection> GetDatabaseConnectionAsync()
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var connection = dbContext.Database.GetDbConnection();
        await connection.OpenAsync();
        return connection;
    }
}

