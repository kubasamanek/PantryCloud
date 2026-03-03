using Microsoft.EntityFrameworkCore;
using PantryCloud.Notification.Core.Configurations;
using PantryCloud.Notification.Core.Entities;
using PantryCloud.SharedKernel.Outbox;

namespace PantryCloud.Notification.Infrastructure.Persistence;

public class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public DbSet<UserHouseholdMembership> UserHouseholdMemberships => Set<UserHouseholdMembership>();
    public DbSet<UserNotification> UserNotifications => Set<UserNotification>();
    public DbSet<ProcessedInboxMessage> ProcessedInboxMessages => Set<ProcessedInboxMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(UserHouseholdMembershipConfiguration).Assembly);
        builder.ApplyConfiguration(new ProcessedInboxMessageConfiguration());
        base.OnModelCreating(builder);
    }
}
