using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PantryCloud.ShoppingList.Core.Entities;

namespace PantryCloud.ShoppingList.Core.Configurations;

public class ShoppingListConfiguration : IEntityTypeConfiguration<Entities.ShoppingList>
{
    public void Configure(EntityTypeBuilder<Entities.ShoppingList> builder)
    {
        builder.ToTable("shopping_lists");

        builder.HasKey(sl => sl.Id);

        builder.Property(sl => sl.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(sl => sl.HouseholdId)
            .HasColumnName("household_id")
            .IsRequired();

        builder.Property(sl => sl.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(sl => sl.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired();

        builder.Property(sl => sl.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(sl => sl.ModifiedBy)
            .HasColumnName("modified_by");

        builder.Property(sl => sl.ModifiedAt)
            .HasColumnName("modified_at");

        builder.HasIndex(sl => sl.HouseholdId)
            .HasDatabaseName("idx_shopping_lists_household");

        builder.HasMany(sl => sl.Items)
            .WithOne(i => i.ShoppingList)
            .HasForeignKey(i => i.ShoppingListId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

