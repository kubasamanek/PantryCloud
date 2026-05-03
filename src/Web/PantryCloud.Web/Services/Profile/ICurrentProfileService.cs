namespace PantryCloud.Web.Services.Profile;

/// <summary>
/// Caches the current user's display name from the household profile
/// </summary>
public interface ICurrentProfileService
{
    string? DisplayName { get; }
    Task EnsureLoadedAsync(CancellationToken cancellationToken = default);
    Task RefreshAsync(CancellationToken cancellationToken = default);
    event Action? ProfileUpdated;
}
