using AB.SmokPrzewodnik.Domain.Navigation;
using AB.SmokPrzewodnik.Infrastructure.Navigation;
using AB.SmokPrzewodnik.Infrastructure.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AB.SmokPrzewodnik.Infrastructure.Database.Mappings;

internal sealed class NavigationSessionMapping : IEntityTypeConfiguration<NavigationSession>
{
    public void Configure(EntityTypeBuilder<NavigationSession> builder)
    {
        builder.ToTable("navigation_sessions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .IsRequired()
            .ValueGeneratedNever();
        builder.Property(x => x.Hash)
            .HasColumnName("hash")
            .HasMaxLength(43)
            .IsRequired();
        builder.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();
        builder.Property(x => x.AccountId)
            .HasColumnName("account_id");
        builder.Property(x => x.ActiveRoute)
            .HasColumnName("active_route")
            .HasColumnType("jsonb")
            .HasConversion(RoutePlanSnapshotSerializer.AlternativeConverter);
        builder.Property(x => x.ActiveRoute).Metadata.SetValueComparer(
            RoutePlanSnapshotSerializer.AlternativeComparer);
        builder.Property(x => x.EffectiveRouteRequest)
            .HasColumnName("effective_route_request")
            .HasColumnType("jsonb")
            .HasConversion(RoutePlanSnapshotSerializer.RequestConverter);
        builder.Property(x => x.EffectiveRouteRequest).Metadata.SetValueComparer(
            RoutePlanSnapshotSerializer.RequestComparer);
        builder.Property(x => x.LatestProgress)
            .HasColumnName("latest_progress")
            .HasColumnType("jsonb")
            .HasConversion(NavigationProgressSerializer.Converter);
        builder.Property(x => x.LatestProgress).Metadata.SetValueComparer(
            NavigationProgressSerializer.Comparer);
        builder.Property(x => x.NextEventSequence)
            .HasColumnName("next_event_sequence");
        builder.Property(x => x.Version)
            .HasColumnName("version")
            .IsConcurrencyToken();
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(x => x.Hash)
            .IsUnique();
        builder.HasIndex(x => x.AccountId);
        builder.HasIndex(x => x.ExpiresAt);
        builder.Ignore(x => x.DomainEvents);
    }
}
