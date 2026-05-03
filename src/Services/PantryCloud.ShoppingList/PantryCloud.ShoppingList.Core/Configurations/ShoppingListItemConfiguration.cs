using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PantryCloud.ShoppingList.Core.Entities;

namespace PantryCloud.ShoppingList.Core.Configurations;

public class ShoppingListItemConfiguration : IEntityTypeConfiguration<ShoppingListItem>
{
    public void Configure(EntityTypeBuilder<ShoppingListItem> builder)
    {
        builder.ToTable("shopping_list_items");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(i => i.ShoppingListId)
            .HasColumnName("shopping_list_id")
            .IsRequired();

        builder.Property(i => i.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.Quantity)
            .HasColumnName("quantity")
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(i => i.Unit)
            .HasColumnName("unit")
            .IsRequired();

        builder.Property(i => i.IsChecked)
            .HasColumnName("is_checked")
            .IsRequired();

        builder.Property(i => i.CheckedBy)
            .HasColumnName("checked_by");

        builder.Property(i => i.CheckedAt)
            .HasColumnName("checked_at");

        builder.Property(i => i.Source)
            .HasColumnName("source")
            .IsRequired();

        builder.Property(i => i.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired();

        builder.Property(i => i.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(i => i.ModifiedBy)
            .HasColumnName("modified_by");

        builder.Property(i => i.ModifiedAt)
            .HasColumnName("modified_at");

        builder.Property(i => i.RowVersion)
            .HasColumnName("row_version")
            .IsRowVersion()
            .IsRequired()
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        builder.HasIndex(i => i.ShoppingListId)
            .HasDatabaseName("idx_shopping_list_items_list");
    }
}

