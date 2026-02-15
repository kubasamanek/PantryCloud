using Microsoft.EntityFrameworkCore;
using PantryCloud.Notification.Core.Configurations;
using PantryCloud.Notification.Core.Entities;

namespace PantryCloud.Notification.Infrastructure.Persistence;

public class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public DbSet<UserHouseholdMembership> UserHouseholdMemberships => Set<UserHouseholdMembership>();
    public DbSet<UserNotification> UserNotifications => Set<UserNotification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(UserHouseholdMembershipConfiguration).Assembly);
        base.OnModelCreating(builder);
    }
}
