using AB.SmokPrzewodnik.Domain.Spatial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AB.SmokPrzewodnik.Infrastructure.Database.Mappings;

internal sealed class SourceAssertionMapping : IEntityTypeConfiguration<SourceAssertion>
{
    public void Configure(EntityTypeBuilder<SourceAssertion> builder)
    {
        builder.ToTable("source_assertions");
        builder.HasKey(assertion => assertion.Id);
        builder.Property(assertion => assertion.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(assertion => assertion.SourceId).HasColumnName("source_id");
        builder.Property(assertion => assertion.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(256);
        builder.Property(assertion => assertion.AssertionType)
            .HasColumnName("assertion_type")
            .HasMaxLength(64)
            .HasConversion(MappingConversions.CodeConverter);
        builder.Property(assertion => assertion.Payload)
            .HasColumnName("payload")
            .HasColumnType("jsonb")
            .HasConversion(MappingConversions.JsonConverter<SourcePayload>());
        builder.Property(assertion => assertion.Payload).Metadata.SetValueComparer(
            MappingConversions.JsonComparer<SourcePayload>());
        builder.Property(assertion => assertion.RetrievedAt).HasColumnName("retrieved_at");
        builder.Property(assertion => assertion.ValidFrom).HasColumnName("valid_from");
        builder.Property(assertion => assertion.ValidUntil).HasColumnName("valid_until");
        builder.Property(assertion => assertion.TransformVersion)
            .HasColumnName("transform_version")
            .HasMaxLength(64);
        builder.HasOne(assertion => assertion.Source)
            .WithMany(source => source.SourceAssertions)
            .HasForeignKey(assertion => assertion.SourceId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(assertion => assertion.AccessibilityFacts)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(assertion => new { assertion.SourceId, assertion.ExternalId }).IsUnique();
    }
}
