using MassTransit;
using Microsoft.Extensions.Logging;
using PantryCloud.Household.Application.Events;
using PantryCloud.Notification.Application;
using PantryCloud.Notification.Core.Dtos;
using PantryCloud.Notification.Core.Enums;
using PantryCloud.Notification.Infrastructure.Persistence;
using PantryCloud.SharedKernel.Messaging;

namespace PantryCloud.Notification.Infrastructure.Consumers;

public class OwnershipTransferredConsumer(
    NotificationDbContext dbContext,
    INotificationService notificationService,
    ILogger<OwnershipTransferredConsumer> logger)
    : DbContextConsumerBase<OwnershipTransferredEvent, NotificationDbContext>(dbContext, logger)
{
    protected override async Task HandleAsync(OwnershipTransferredEvent @event, ConsumeContext context)
    {
        Logger.LogInformation(
            "Received OwnershipTransferredEvent - HouseholdId: {HouseholdId}, PreviousOwnerId: {PreviousOwnerId}, NewOwnerId: {NewOwnerId}",
            @event.HouseholdId, @event.PreviousOwnerId, @event.NewOwnerId);

        var newOwnerDisplay = @event.NewOwnerEmail ?? @event.NewOwnerId.ToString();
        var notification = new NotificationDto(
            Id: Guid.NewGuid(),
            Title: "Ownership Transferred",
            Message: $"{newOwnerDisplay} is now the household owner.",
            Type: NotificationType.Success,
            CreatedAt: DateTime.UtcNow,
            CorrelationId: @event.CorrelationId);
        await notificationService.SendToHouseholdAsync(@event.HouseholdId, notification, context.CancellationToken);

        Logger.LogInformation(
            "Processed OwnershipTransferredEvent - HouseholdId: {HouseholdId}, NewOwnerId: {NewOwnerId}",
            @event.HouseholdId, @event.NewOwnerId);
    }
}
