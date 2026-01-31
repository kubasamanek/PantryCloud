using Microsoft.Extensions.Logging;
using PantryCloud.SharedKernel.Identity;

namespace PantryCloud.SharedKernel.Services;

/// <summary>
/// Base class for services that need user context and logging.
/// Provides common dependencies that most services require.
/// </summary>
/// <typeparam name="TService">The type of the service (for logger).</typeparam>
public abstract class BaseService<TService>
    where TService : class
{
    /// <summary>
    /// Gets the user context for accessing current user information.
    /// </summary>
    private IUserContext UserContext { get; }

    /// <summary>
    /// Gets the logger for the service.
    /// </summary>
    protected ILogger<TService> Logger { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseService{TService}"/> class.
    /// </summary>
    /// <param name="userContext">The user context.</param>
    /// <param name="logger">The logger.</param>
    protected BaseService(IUserContext userContext, ILogger<TService> logger)
    {
        UserContext = userContext;
        Logger = logger;
    }

    /// <summary>
    /// Gets the current user ID.
    /// </summary>
    protected Guid UserId => UserContext.UserId;

    /// <summary>
    /// Gets the current user email.
    /// </summary>
    protected string UserEmail => UserContext.Email;
}
