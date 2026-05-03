using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PantryCloud.Audit.Core.Entities;

namespace PantryCloud.Audit.Core.Configurations;

public class UserHouseholdMembershipConfiguration : IEntityTypeConfiguration<UserHouseholdMembership>
{
    public void Configure(EntityTypeBuilder<UserHouseholdMembership> builder)
    {
        builder.HasKey(u => u.UserId);

        builder.Property(u => u.HouseholdId).IsRequired();
        builder.Property(u => u.JoinedAt).IsRequired();

        builder.HasIndex(u => u.HouseholdId);
        builder.HasIndex(u => new { u.HouseholdId, u.LeftAt });
    }
}
