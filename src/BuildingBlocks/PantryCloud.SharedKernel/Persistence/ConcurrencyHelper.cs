using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PantryCloud.SharedKernel.Persistence;

/// <summary>
/// Helper methods for handling concurrency conflicts in Entity Framework Core.
/// </summary>
public static class ConcurrencyHelper
{
    /// <summary>
    /// Handles a <see cref="DbUpdateConcurrencyException"/> by logging and returning an appropriate error.
    /// </summary>
    /// <typeparam name="T">The type of the entity or result.</typeparam>
    /// <param name="exception">The concurrency exception.</param>
    /// <param name="logger">The logger to use.</param>
    /// <param name="entityName">The name of the entity (for logging).</param>
    /// <param name="entityId">The ID of the entity (for logging).</param>
    /// <param name="error">The error to return.</param>
    /// <returns>An ErrorOr result with the specified error.</returns>
    public static ErrorOr<T> HandleConcurrencyException<T>(
        DbUpdateConcurrencyException exception,
        ILogger logger,
        string entityName,
        Guid? entityId = null,
        Error? error = null)
    {
        var entityIdMessage = entityId.HasValue ? $" {entityName}Id: {entityId.Value}" : "";
        logger.LogWarning(
            exception,
            "Concurrency conflict detected for {EntityName}{EntityIdMessage}",
            entityName,
            entityIdMessage);

        return error ?? Error.Conflict(
            code: $"{entityName}.ConcurrencyConflict",
            description: $"The {entityName} was modified by another user. Please refresh and try again.");
    }

    /// <summary>
    /// Executes a database operation and handles concurrency exceptions.
    /// </summary>
    /// <typeparam name="T">The return type.</typeparam>
    /// <param name="operation">The database operation to execute.</param>
    /// <param name="logger">The logger to use.</param>
    /// <param name="entityName">The name of the entity (for logging).</param>
    /// <param name="entityId">The ID of the entity (for logging).</param>
    /// <param name="error">The error to return on concurrency conflict.</param>
    /// <returns>An ErrorOr result.</returns>
    public static async Task<ErrorOr<T>> ExecuteWithConcurrencyHandling<T>(
        Func<Task<T>> operation,
        ILogger logger,
        string entityName,
        Guid? entityId = null,
        Error? error = null)
    {
        try
        {
            var result = await operation();
            return result;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return HandleConcurrencyException<T>(ex, logger, entityName, entityId, error);
        }
    }
}


