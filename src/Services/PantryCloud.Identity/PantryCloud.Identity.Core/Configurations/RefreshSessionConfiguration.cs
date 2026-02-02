using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PantryCloud.Identity.Core.Entities;

namespace PantryCloud.Identity.Core.Configurations;

internal sealed class RefreshSessionConfiguration : IEntityTypeConfiguration<RefreshSession>
{
    public void Configure(EntityTypeBuilder<RefreshSession> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.RefreshToken).IsRequired();
        builder.Property(s => s.DeviceName).HasMaxLength(200);

        builder.HasIndex(s => new { s.UserId, s.RefreshToken });

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
