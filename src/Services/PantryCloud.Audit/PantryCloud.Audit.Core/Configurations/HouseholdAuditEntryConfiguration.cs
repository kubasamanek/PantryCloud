using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PantryCloud.Audit.Core.Entities;

namespace PantryCloud.Audit.Core.Configurations;

public class HouseholdAuditEntryConfiguration : IEntityTypeConfiguration<HouseholdAuditEntry>
{
    public void Configure(EntityTypeBuilder<HouseholdAuditEntry> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.HouseholdId).IsRequired();
        builder.Property(e => e.ActionType).IsRequired().HasMaxLength(50);
        builder.Property(e => e.EntityType).IsRequired().HasMaxLength(50);
        builder.Property(e => e.OccurredAt).IsRequired();
        builder.Property(e => e.EventId).IsRequired();

        builder.HasIndex(e => new { e.HouseholdId, e.OccurredAt });
        builder.HasIndex(e => e.OccurredAt);
        builder.HasIndex(e => e.EventId).IsUnique();
    }
}
