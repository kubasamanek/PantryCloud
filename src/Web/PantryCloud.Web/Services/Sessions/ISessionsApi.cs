namespace PantryCloud.Web.Services.Sessions;

public interface ISessionsApi
{
    Task<ListSessionsResponse?> ListSessionsAsync(CancellationToken cancellationToken = default);
    Task<bool> RevokeSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<bool> RevokeAllOtherSessionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Revoke the current session on the server (logout). Call before clearing tokens in the browser.
    /// </summary>
    Task<bool> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
}
