using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PantryCloud.Household.Core.Entities;

namespace PantryCloud.Household.Core.Configurations;

public class MemberPreferenceConfiguration : IEntityTypeConfiguration<MemberPreference>
{
    private static readonly JsonSerializerOptions JsonOptions = new();

    public void Configure(EntityTypeBuilder<MemberPreference> builder)
    {
        builder.HasKey(p => p.UserId);

        builder.Property(p => p.DietaryProfile)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(p => p.ExcludedIngredients)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<string>>(v, JsonOptions) ?? new List<string>())
            .HasColumnType("text");
    }
}
