using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PantryCloud.Household.Application.Events;
using PantryCloud.Notification.Application;
using PantryCloud.Notification.Core.Dtos;
using PantryCloud.Notification.Core.Enums;
using PantryCloud.Notification.Infrastructure.Persistence;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Notification.Infrastructure.Consumers;

public class MemberLeftHouseholdConsumer(
    NotificationDbContext dbContext,
    INotificationService notificationService,
    ILogger<MemberLeftHouseholdConsumer> logger)
    : DbContextConsumerBase<MemberLeftHouseholdEvent, NotificationDbContext>(dbContext, logger)
{
    protected override async Task HandleAsync(MemberLeftHouseholdEvent @event, ConsumeContext context)
    {
        Logger.LogInformation(
            "Received MemberLeftHouseholdEvent - MemberId: {MemberId}, HouseholdId: {HouseholdId}, LeftAt: {LeftAt}",
            @event.MemberId, @event.HouseholdId, @event.LeftAt);

        // 1. Update membership (set LeftAt)
        var membership = await DbContext.UserHouseholdMemberships
            .FirstOrDefaultAsync(m => m.UserId == @event.MemberId && m.HouseholdId == @event.HouseholdId, context.CancellationToken);

        if (membership != null)
        {
            membership.LeftAt = @event.LeftAt;
            Logger.LogInformation("Updated membership LeftAt for UserId: {MemberId}, HouseholdId: {HouseholdId}",
                @event.MemberId, @event.HouseholdId);
        }
        else
        {
            Logger.LogWarning("No membership found for MemberId: {MemberId}, HouseholdId: {HouseholdId}",
                @event.MemberId, @event.HouseholdId);
        }

        // 2. Send "X left" to remaining household members
        var notification = new NotificationDto(
            Id: Guid.NewGuid(),
            Title: "Member Left Household",
            Message: $"Member {@event.MemberEmail ?? @event.MemberId.ToString()} left the household.",
            Type: NotificationType.Success,
            CreatedAt: DateTime.UtcNow,
            CorrelationId: @event.CorrelationId);
        await notificationService.SendToHouseholdExceptAsync(@event.HouseholdId, @event.MemberId, notification, context.CancellationToken);

        Logger.LogInformation(
            "Processed MemberLeftHouseholdEvent - MemberId: {MemberId}, HouseholdId: {HouseholdId}",
            @event.MemberId, @event.HouseholdId);
    }
}
