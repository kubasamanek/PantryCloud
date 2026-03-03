using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PantryCloud.SharedKernel.Outbox;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.EventType)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(m => m.Payload)
            .IsRequired();

        builder.Property(m => m.OccurredAt).IsRequired();
        builder.Property(m => m.ProcessedAt);
        builder.Property(m => m.Error).HasMaxLength(2048);

        // The relay worker queries only unprocessed rows.
        builder.HasIndex(m => m.ProcessedAt)
            .HasFilter("\"ProcessedAt\" IS NULL")
            .HasDatabaseName("IX_OutboxMessages_Unprocessed");
    }
}
