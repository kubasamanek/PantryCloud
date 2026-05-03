using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace PantryCloud.Notification.Infrastructure.Hubs;

/// <summary>
/// SignalR hub for real-time notifications to connected clients.
/// Clients connect with JWT (query param access_token) and are auto-joined to their user group.
/// </summary>
[Authorize]
public class NotificationHub(ILogger<NotificationHub> logger) : Hub
{
    private const string NotificationGroupPrefix = "user_";

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? Context.User?.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!string.IsNullOrEmpty(userId))
        {
            var groupName = $"{NotificationGroupPrefix}{userId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            logger.LogInformation("Client {ConnectionId} auto-joined group {GroupName} for user {UserId}",
                Context.ConnectionId, groupName, userId);
        }
        else
        {
            logger.LogWarning("Client {ConnectionId} connected without user identity", Context.ConnectionId);
        }
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

