using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PantryCloud.Notification.Core.Entities;

namespace PantryCloud.Notification.Core.Configurations;

public class UserNotificationConfiguration : IEntityTypeConfiguration<UserNotification>
{
    public void Configure(EntityTypeBuilder<UserNotification> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.UserId).IsRequired();
        builder.Property(u => u.SourceNotificationId).IsRequired();
        builder.Property(u => u.Title).IsRequired().HasMaxLength(500);
        builder.Property(u => u.Message).IsRequired();
        builder.Property(u => u.Type).IsRequired();
        builder.Property(u => u.CreatedAt).IsRequired();

        builder.HasIndex(u => u.UserId);
        builder.HasIndex(u => new { u.UserId, u.CreatedAt });
    }
}
