using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace PantryCloud.Notification.Infrastructure.Hubs;

/// <summary>
/// SignalR hub for real-time notifications to connected clients.
/// </summary>
public class NotificationHub(ILogger<NotificationHub> logger) : Hub
{
    private const string NotificationGroupPrefix = "user_";

    public override async Task OnConnectedAsync()
    {
        logger.LogInformation("Client connected - ConnectionId: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogInformation("Client disconnected - ConnectionId: {ConnectionId}, Exception: {Exception}",
            Context.ConnectionId, exception?.Message);
        await base.OnDisconnectedAsync(exception);
    }
    
    public async Task JoinUserGroup(string userId)
    {
        var groupName = $"{NotificationGroupPrefix}{userId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        logger.LogInformation("Client {ConnectionId} joined group {GroupName}", Context.ConnectionId, groupName);
    }
    
    public async Task LeaveUserGroup(string userId)
    {
        var groupName = $"{NotificationGroupPrefix}{userId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        logger.LogInformation("Client {ConnectionId} left group {GroupName}", Context.ConnectionId, groupName);
    }
}

