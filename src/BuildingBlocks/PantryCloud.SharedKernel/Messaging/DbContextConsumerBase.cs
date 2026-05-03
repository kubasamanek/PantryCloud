using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.SharedKernel.Messaging;

/// <summary>
/// Base class for MassTransit consumers that need a DbContext.
/// This is a convenience class that wraps ConsumerBase with a non-nullable DbContext.
/// </summary>
/// <typeparam name="TEvent">The type of the integration event.</typeparam>
/// <typeparam name="TDbContext">The type of the DbContext.</typeparam>
public abstract class DbContextConsumerBase<TEvent, TDbContext> : ConsumerBase<TEvent, TDbContext>
    where TEvent : IntegrationEvent
    where TDbContext : DbContext
{
    /// <summary>
    /// Gets the DbContext for database operations (non-nullable).
    /// </summary>
    protected new TDbContext DbContext => base.DbContext!;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbContextConsumerBase{TEvent, TDbContext}"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="logger">The logger.</param>
    protected DbContextConsumerBase(TDbContext dbContext, ILogger logger)
        : base(dbContext, logger)
    {
    }
}
