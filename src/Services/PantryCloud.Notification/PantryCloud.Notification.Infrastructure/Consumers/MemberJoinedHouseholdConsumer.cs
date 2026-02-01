using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PantryCloud.Household.Application.Events;
using PantryCloud.Notification.Application;
using PantryCloud.Notification.Core.Dtos;
using PantryCloud.Notification.Core.Entities;
using PantryCloud.Notification.Core.Enums;
using PantryCloud.Notification.Infrastructure.Persistence;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Notification.Infrastructure.Consumers;

public class MemberJoinedHouseholdConsumer(
    NotificationDbContext dbContext,
    INotificationService notificationService,
    ILogger<MemberJoinedHouseholdConsumer> logger)
    : DbContextConsumerBase<MemberJoinedHouseholdEvent, NotificationDbContext>(dbContext, logger)
{
    protected override async Task HandleAsync(MemberJoinedHouseholdEvent @event, ConsumeContext context)
    {
        Logger.LogInformation(
            "Received MemberJoinedHouseholdEvent - NewMemberId: {NewMemberId}, HouseholdId: {HouseholdId}, JoinedAt: {JoinedAt}",
            @event.NewMemberId, @event.HouseholdId, @event.JoinedAt);

        // 1. Persist membership
        var existingMembership = await DbContext.UserHouseholdMemberships
            .FirstOrDefaultAsync(m => m.UserId == @event.NewMemberId, context.CancellationToken);

        if (existingMembership != null)
        {
            if (existingMembership.HouseholdId != @event.HouseholdId)
            {
                existingMembership.HouseholdId = @event.HouseholdId;
                existingMembership.JoinedAt = @event.JoinedAt;
                existingMembership.LeftAt = null;
            }
            else if (existingMembership.LeftAt != null)
            {
                existingMembership.LeftAt = null;
                existingMembership.JoinedAt = @event.JoinedAt;
            }
        }
        else
        {
            await DbContext.UserHouseholdMemberships.AddAsync(new UserHouseholdMembership
            {
                UserId = @event.NewMemberId,
                HouseholdId = @event.HouseholdId,
                JoinedAt = @event.JoinedAt
            }, context.CancellationToken);
        }

        // 2. Send welcome notification to the new member
        var welcomeNotification = new NotificationDto(
            Id: Guid.NewGuid(),
            Title: "Welcome to the Household",
            Message: "You have successfully joined the household.",
            Type: NotificationType.Success,
            CreatedAt: DateTime.UtcNow,
            UserId: @event.NewMemberId,
            CorrelationId: @event.CorrelationId);
        await notificationService.SendToUserAsync(@event.NewMemberId, welcomeNotification, context.CancellationToken);

        // 3. Send "X joined" to all other household members
        var joinedNotification = new NotificationDto(
            Id: Guid.NewGuid(),
            Title: "Member Joined Household",
            Message: $"Member {@event.MemberEmail ?? @event.NewMemberId.ToString()} joined the household.",
            Type: NotificationType.Success,
            CreatedAt: DateTime.UtcNow,
            CorrelationId: @event.CorrelationId);
        await notificationService.SendToHouseholdExceptAsync(@event.HouseholdId, @event.NewMemberId, joinedNotification, context.CancellationToken);

        Logger.LogInformation(
            "Processed MemberJoinedHouseholdEvent - NewMemberId: {NewMemberId}, HouseholdId: {HouseholdId}",
            @event.NewMemberId, @event.HouseholdId);
    }
}
