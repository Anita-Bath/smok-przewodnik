using AB.SmokPrzewodnik.Domain.Navigation;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using AB.SmokPrzewodnik.Infrastructure.Navigation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AB.SmokPrzewodnik.Infrastructure.Database.Mappings;

internal sealed class NavigationEventMapping : IEntityTypeConfiguration<NavigationEvent>
{
    public void Configure(EntityTypeBuilder<NavigationEvent> builder)
    {
        builder.ToTable("navigation_events");
        builder.HasKey(item => new { item.SessionId, item.Sequence });
        builder.Property(item => item.SessionId).HasColumnName("session_id");
        builder.Property(item => item.Sequence).HasColumnName("sequence");
        builder.Property(item => item.EventType).HasColumnName("event_type").HasConversion<string>();
        builder.Property(item => item.Urgency).HasColumnName("urgency").HasConversion<string>();
        builder.Property(item => item.Maneuver).HasColumnName("maneuver").HasColumnType("jsonb")
            .HasConversion(NavigationEventValueConverters.ManeuverConverter);
        builder.Property(item => item.Maneuver).Metadata.SetValueComparer(NavigationEventValueConverters.ManeuverComparer);
        builder.Property(item => item.HazardCode).HasColumnName("hazard_code")
            .HasConversion(value => value.HasValue ? value.Value.Value : null,
                value => value == null ? null : new Code(value));
        builder.Property(item => item.LandmarkCode).HasColumnName("landmark_code")
            .HasConversion(value => value.HasValue ? value.Value.Value : null,
                value => value == null ? null : new Code(value));
        builder.Property(item => item.RemainingDistanceMetres).HasColumnName("remaining_distance_metres");
        builder.Property(item => item.LocalizedParameters).HasColumnName("localized_parameters").HasColumnType("jsonb")
            .HasConversion(NavigationEventValueConverters.ParametersConverter);
        builder.Property(item => item.LocalizedParameters).Metadata.SetValueComparer(NavigationEventValueConverters.ParametersComparer);
        builder.Property(item => item.SupportedFeedbackPatterns).HasColumnName("supported_feedback_patterns").HasColumnType("jsonb")
            .HasConversion(NavigationEventValueConverters.PatternsConverter);
        builder.Property(item => item.SupportedFeedbackPatterns).Metadata.SetValueComparer(NavigationEventValueConverters.PatternsComparer);
        builder.Property(item => item.CreatedAt).HasColumnName("created_at");
        builder.HasOne<NavigationSession>().WithMany().HasForeignKey(item => item.SessionId).OnDelete(DeleteBehavior.Cascade);
    }
}
