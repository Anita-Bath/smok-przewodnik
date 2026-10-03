using AB.SmokPrzewodnik.Domain.Planning;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AB.SmokPrzewodnik.Infrastructure.Database.Mappings;

internal sealed class SuggestionProjectionMapping : IEntityTypeConfiguration<SuggestionProjection>
{
    public void Configure(EntityTypeBuilder<SuggestionProjection> builder)
    {
        builder.ToTable("suggestion_projections");
        builder.HasKey(suggestion => new { suggestion.AccountId, suggestion.SuggestedEntityId });
        builder.Property(suggestion => suggestion.AccountId).HasColumnName("account_id");
        builder.Property(suggestion => suggestion.SuggestedEntityId)
            .HasColumnName("suggested_entity_id");
        builder.Property(suggestion => suggestion.ReasonCodes)
            .HasColumnName("reason_codes")
            .HasColumnType("jsonb")
            .HasConversion(MappingConversions.JsonConverter<IReadOnlySet<Code>>());
        builder.Property(suggestion => suggestion.ReasonCodes).Metadata.SetValueComparer(
            MappingConversions.JsonComparer<IReadOnlySet<Code>>());
        builder.Property(suggestion => suggestion.Score)
            .HasColumnName("score")
            .HasPrecision(8, 6);
        builder.Property(suggestion => suggestion.GeneratedAt).HasColumnName("generated_at");
        builder.Property(suggestion => suggestion.ExpiresAt).HasColumnName("expires_at");
        builder.HasOne(suggestion => suggestion.SuggestedEntity)
            .WithMany(entity => entity.Suggestions)
            .HasForeignKey(suggestion => suggestion.SuggestedEntityId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(suggestion => new { suggestion.AccountId, suggestion.Score });
    }
}
