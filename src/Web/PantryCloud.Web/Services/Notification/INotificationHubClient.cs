using Microsoft.AspNetCore.SignalR.Client;

namespace PantryCloud.Web.Services.Notification;

/// <summary>
/// SignalR hub client for real-time notifications. Start when user is authenticated; stop on logout.
/// </summary>
public interface INotificationHubClient
{
    /// <summary>
    /// Fired when a notification is received from the server.
    /// </summary>
    event Action<NotificationMessage>? OnNotification;

    /// <summary>
    /// Fired when connection state changes (for UI indicator).
    /// </summary>
    event Action<HubConnectionState>? OnStateChanged;

    HubConnectionState State { get; }

    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}
