using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PantryCloud.SharedKernel.Identity;

namespace PantryCloud.SharedKernel.Services;

/// <summary>
/// Base class for services that need a DbContext, user context, and logging.
/// Inherits from <see cref="BaseService{TService}"/> and adds DbContext support.
/// </summary>
/// <typeparam name="TService">The type of the service (for logger).</typeparam>
/// <typeparam name="TDbContext">The type of the DbContext.</typeparam>
public abstract class BaseDbContextService<TService, TDbContext> : BaseService<TService>
    where TService : class
    where TDbContext : DbContext
{
    /// <summary>
    /// Gets the DbContext for database operations.
    /// </summary>
    protected TDbContext DbContext { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseDbContextService{TService, TDbContext}"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="userContext">The user context.</param>
    /// <param name="logger">The logger.</param>
    protected BaseDbContextService(
        TDbContext dbContext,
        IUserContext userContext,
        ILogger<TService> logger)
        : base(userContext, logger)
    {
        DbContext = dbContext;
    }
}


