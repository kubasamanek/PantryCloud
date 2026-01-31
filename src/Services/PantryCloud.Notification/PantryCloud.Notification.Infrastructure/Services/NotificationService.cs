using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PantryCloud.Notification.Application;
using PantryCloud.Notification.Core.Dtos;
using PantryCloud.Notification.Infrastructure.Hubs;

namespace PantryCloud.Notification.Infrastructure.Services;

public class NotificationService(
    IHubContext<NotificationHub> hubContext,
    IHouseholdMembershipRepository membershipRepository,
    ILogger<NotificationService> logger) : INotificationService
{
    private const string NotificationMethod = "ReceiveNotification";
    private const string NotificationGroupPrefix = "user_";

    public async Task SendToUserAsync(Guid userId, NotificationDto notification, CancellationToken cancellationToken = default)
    {
        var groupName = $"{NotificationGroupPrefix}{userId}";
        logger.LogInformation(
            "Sending notification to user {UserId} - Id: {NotificationId}, Title: {Title}",
            userId, notification.Id, notification.Title);
        await hubContext.Clients.Group(groupName).SendAsync(NotificationMethod, notification, cancellationToken);
    }

    public async Task SendToHouseholdAsync(Guid householdId, NotificationDto notification, CancellationToken cancellationToken = default)
    {
        var memberIds = await membershipRepository.GetHouseholdMemberIdsAsync(householdId, cancellationToken);
        if (memberIds.Count == 0)
        {
            logger.LogWarning("No active members found for household {HouseholdId}, notification {NotificationId} not sent",
                householdId, notification.Id);
            return;
        }
        logger.LogInformation(
            "Sending notification to household {HouseholdId} ({MemberCount} members) - Id: {NotificationId}, Title: {Title}",
            householdId, memberIds.Count, notification.Id, notification.Title);
        await SendToUserIdsAsync(memberIds, notification, cancellationToken);
    }

    public async Task SendToHouseholdExceptAsync(Guid householdId, Guid excludeUserId, NotificationDto notification, CancellationToken cancellationToken = default)
    {
        var memberIds = await membershipRepository.GetHouseholdMemberIdsExceptAsync(householdId, excludeUserId, cancellationToken);
        if (memberIds.Count == 0)
        {
            logger.LogDebug("No other members in household {HouseholdId} (excluding {ExcludeUserId}), notification {NotificationId} not sent",
                householdId, excludeUserId, notification.Id);
            return;
        }
        logger.LogInformation(
            "Sending notification to household {HouseholdId} excluding {ExcludeUserId} ({MemberCount} recipients) - Id: {NotificationId}, Title: {Title}",
            householdId, excludeUserId, memberIds.Count, notification.Id, notification.Title);
        await SendToUserIdsAsync(memberIds, notification, cancellationToken);
    }

    private async Task SendToUserIdsAsync(IReadOnlyList<Guid> userIds, NotificationDto notification, CancellationToken cancellationToken)
    {
        var groupNames = userIds.Select(id => $"{NotificationGroupPrefix}{id}").ToList();
        await hubContext.Clients.Groups(groupNames).SendAsync(NotificationMethod, notification, cancellationToken);
    }

    public async Task BroadcastNotificationAsync(NotificationDto notification, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Broadcasting notification to all clients - Id: {NotificationId}, Title: {Title}",
            notification.Id, notification.Title);
        await hubContext.Clients.All.SendAsync(NotificationMethod, notification, cancellationToken);
    }
}
