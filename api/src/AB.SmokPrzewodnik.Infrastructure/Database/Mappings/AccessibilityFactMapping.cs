using AB.SmokPrzewodnik.Domain.Spatial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AB.SmokPrzewodnik.Infrastructure.Database.Mappings;

internal sealed class AccessibilityFactMapping : IEntityTypeConfiguration<AccessibilityFact>
{
    public void Configure(EntityTypeBuilder<AccessibilityFact> builder)
    {
        builder.ToTable("accessibility_facts");
        builder.HasKey(fact => fact.Id);
        builder.Property(fact => fact.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Ignore(fact => fact.Target);
        builder.Property(fact => fact.TargetKind).HasColumnName("target_kind").HasMaxLength(32);
        builder.Property(fact => fact.SpatialEntityId).HasColumnName("spatial_entity_id");
        builder.Property(fact => fact.GraphRoutingProvider).HasColumnName("graph_routing_provider").HasMaxLength(128);
        builder.Property(fact => fact.GraphVersion).HasColumnName("graph_version").HasMaxLength(128);
        builder.Property(fact => fact.GraphElementType).HasColumnName("graph_element_type").HasConversion<string>().HasMaxLength(32);
        builder.Property(fact => fact.GraphExternalElementId).HasColumnName("graph_external_element_id").HasMaxLength(256);
        builder.Property(fact => fact.AttributeCode)
            .HasColumnName("attribute_code")
            .HasMaxLength(64)
            .HasConversion(MappingConversions.CodeConverter);
        builder.Ignore(fact => fact.Value);
        builder.Property(fact => fact.ValueKind).HasColumnName("value_kind").HasMaxLength(32);
        builder.Property(fact => fact.BooleanValue).HasColumnName("boolean_value");
        builder.Property(fact => fact.NumberValue).HasColumnName("number_value");
        builder.Property(fact => fact.NumberUnitCode).HasColumnName("number_unit_code").HasMaxLength(32);
        builder.Property(fact => fact.CodeValue).HasColumnName("code_value").HasMaxLength(128);
        builder.Property(fact => fact.TextValue).HasColumnName("text_value");
        builder.Ignore(fact => fact.Evidence);
        builder.Property(fact => fact.EvidenceKind).HasColumnName("evidence_kind").HasMaxLength(32);
        builder.Property(fact => fact.SourceAssertionId).HasColumnName("source_assertion_id");
        builder.Property(fact => fact.ObservationId).HasColumnName("observation_id");
        builder.Property(fact => fact.ObservedAt).HasColumnName("observed_at");
        builder.Property(fact => fact.ValidFrom).HasColumnName("valid_from");
        builder.Property(fact => fact.ValidUntil).HasColumnName("valid_until");
        builder.Property(fact => fact.ConfidenceWeight)
            .HasColumnName("confidence_weight")
            .HasPrecision(5, 4);
        builder.HasOne(fact => fact.SpatialEntity)
            .WithMany(entity => entity.AccessibilityFacts)
            .HasForeignKey(fact => fact.SpatialEntityId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(fact => fact.SourceAssertion)
            .WithMany(assertion => assertion.AccessibilityFacts)
            .HasForeignKey(fact => fact.SourceAssertionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(fact => new { fact.SpatialEntityId, fact.AttributeCode });
    }
}
