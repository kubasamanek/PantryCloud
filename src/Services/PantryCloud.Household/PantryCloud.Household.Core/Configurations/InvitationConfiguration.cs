using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PantryCloud.Household.Core.Entities;

namespace PantryCloud.Household.Core.Configurations;

public class HouseholdInvitationConfiguration : IEntityTypeConfiguration<HouseholdInvitation>
{
    public void Configure(EntityTypeBuilder<HouseholdInvitation> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Code)
            .IsRequired();

        builder.Property(i => i.ExpiresAt)
            .IsRequired();

        builder.HasIndex(i => i.Code)
            .IsUnique();
    }
}