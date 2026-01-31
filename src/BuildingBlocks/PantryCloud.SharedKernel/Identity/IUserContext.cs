namespace PantryCloud.SharedKernel.Identity;

/// <summary>
/// Provides access to the current authenticated user's information.
/// </summary>
public interface IUserContext
{
    /// <summary>
    /// Gets the current user's unique identifier.
    /// </summary>
    Guid UserId { get; }
    
    /// <summary>
    /// Gets the current user's email address.
    /// </summary>
    string Email { get; }
}
