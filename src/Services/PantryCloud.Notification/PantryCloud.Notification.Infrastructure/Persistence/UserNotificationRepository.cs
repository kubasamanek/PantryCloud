using Microsoft.EntityFrameworkCore;
using PantryCloud.Notification.Application;
using PantryCloud.Notification.Core.Dtos;
using PantryCloud.Notification.Core.Entities;
using PantryCloud.Notification.Core.Enums;

namespace PantryCloud.Notification.Infrastructure.Persistence;

public class UserNotificationRepository(NotificationDbContext dbContext) : IUserNotificationRepository
{
    public async Task AddAsync(Guid userId, NotificationDto notification, CancellationToken cancellationToken = default)
    {
        var entity = new UserNotification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SourceNotificationId = notification.Id,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type,
            CreatedAt = notification.CreatedAt
        };
        await dbContext.UserNotifications.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddForUsersAsync(IReadOnlyList<Guid> userIds, NotificationDto notification, CancellationToken cancellationToken = default)
    {
        if (userIds.Count == 0) return;
        var entities = userIds.Select(userId => new UserNotification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SourceNotificationId = notification.Id,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type,
            CreatedAt = notification.CreatedAt
        }).ToList();
        await dbContext.UserNotifications.AddRangeAsync(entities, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<NotificationDto>> GetByUserIdAsync(Guid userId, int limit = 50, DateTime? since = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.UserNotifications
            .AsNoTracking()
            .Where(u => u.UserId == userId);
        if (since.HasValue)
            query = query.Where(u => u.CreatedAt >= since.Value);
        var list = await query
            .OrderByDescending(u => u.CreatedAt)
            .Take(limit)
            .Select(u => new NotificationDto(
                u.SourceNotificationId,
                u.Title,
                u.Message,
                u.Type,
                u.CreatedAt,
                u.UserId,
                null))
            .ToListAsync(cancellationToken);
        return list;
    }
}
