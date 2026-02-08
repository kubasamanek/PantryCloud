namespace PantryCloud.Web.Services.Sessions;

public interface ISessionsApi
{
    Task<ListSessionsResponse?> ListSessionsAsync(CancellationToken cancellationToken = default);
    Task<bool> RevokeSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<bool> RevokeAllOtherSessionsAsync(CancellationToken cancellationToken = default);
}
