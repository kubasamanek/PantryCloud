using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PantryCloud.Household.Core.Configurations;

public class HouseholdConfiguration : IEntityTypeConfiguration<Entities.Household>
{
    public void Configure(EntityTypeBuilder<Entities.Household> builder)
    {
        builder.HasKey(h => h.Id);

        builder.Property(h => h.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasMany(h => h.Members)
            .WithOne()
            .HasForeignKey(m => m.HouseholdId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(h => h.Invitations)
            .WithOne()
            .HasForeignKey(i => i.HouseholdId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}