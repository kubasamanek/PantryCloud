using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PantryCloud.SharedKernel.Outbox;

public class ProcessedInboxMessageConfiguration : IEntityTypeConfiguration<ProcessedInboxMessage>
{
    public void Configure(EntityTypeBuilder<ProcessedInboxMessage> builder)
    {
        builder.ToTable("ProcessedInboxMessages");

        // guarantees uniqueness without a surrogate key.
        builder.HasKey(m => m.MessageId);

        builder.Property(m => m.EventType)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(m => m.ProcessedAt).IsRequired();
    }
}
