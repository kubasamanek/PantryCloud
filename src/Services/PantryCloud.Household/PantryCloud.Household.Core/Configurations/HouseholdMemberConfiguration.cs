using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PantryCloud.Household.Core.Entities;

namespace PantryCloud.Household.Core.Configurations;

public class HouseholdMemberConfiguration : IEntityTypeConfiguration<HouseholdMember>
{
    public void Configure(EntityTypeBuilder<HouseholdMember> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.UserId).IsRequired();
        builder.Property(m => m.JoinedAt).IsRequired();

        builder.Property(m => m.Role)
            .HasConversion<string>()
            .IsRequired();

        builder.HasIndex(m => m.UserId).IsUnique(); // One household per user
    }
}