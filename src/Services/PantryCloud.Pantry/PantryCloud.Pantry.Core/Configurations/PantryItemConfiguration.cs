using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PantryCloud.Pantry.Core.Entities;
using PantryCloud.Pantry.Core.Enums;

namespace PantryCloud.Pantry.Core.Configurations;

public class PantryItemConfiguration : IEntityTypeConfiguration<PantryItem>
{
    public void Configure(EntityTypeBuilder<PantryItem> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Quantity)
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(p => p.Unit)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.Category)
            .HasMaxLength(100);

        builder.Property(p => p.Notes)
            .HasMaxLength(1000);

        builder.Property(p => p.ImageUrl)
            .HasMaxLength(500);

        builder.Property(p => p.HouseholdId)
            .IsRequired();

        builder.Property(p => p.CreatedBy)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.RowVersion)
            .IsRowVersion()
            .IsRequired()
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        builder.HasIndex(p => p.HouseholdId);
        builder.HasIndex(p => new { p.HouseholdId, p.Category });
        builder.HasIndex(p => new { p.HouseholdId, p.Name });
    }
}

