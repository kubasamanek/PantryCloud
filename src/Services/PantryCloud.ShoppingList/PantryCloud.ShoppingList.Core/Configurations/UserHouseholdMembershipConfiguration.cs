using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PantryCloud.ShoppingList.Core.Entities;

namespace PantryCloud.ShoppingList.Core.Configurations;

public class UserHouseholdMembershipConfiguration : IEntityTypeConfiguration<UserHouseholdMembership>
{
    public void Configure(EntityTypeBuilder<UserHouseholdMembership> builder)
    {
        builder.ToTable("user_household_memberships");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(m => m.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(m => m.HouseholdId)
            .HasColumnName("household_id")
            .IsRequired();

        builder.Property(m => m.JoinedAt)
            .HasColumnName("joined_at")
            .IsRequired();

        builder.Property(m => m.LeftAt)
            .HasColumnName("left_at");

        builder.HasIndex(m => new { m.UserId, m.HouseholdId })
            .IsUnique()
            .HasDatabaseName("idx_user_household_unique");

        builder.HasIndex(m => m.UserId)
            .HasFilter("left_at IS NULL")
            .HasDatabaseName("idx_memberships_user_active");
    }
}

