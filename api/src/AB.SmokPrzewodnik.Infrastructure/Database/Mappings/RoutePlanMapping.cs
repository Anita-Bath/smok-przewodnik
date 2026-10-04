using AB.SmokPrzewodnik.Domain.Routing;
using AB.SmokPrzewodnik.Infrastructure.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AB.SmokPrzewodnik.Infrastructure.Database.Mappings;

internal sealed class RoutePlanMapping : IEntityTypeConfiguration<RoutePlan>
{
    public void Configure(EntityTypeBuilder<RoutePlan> builder)
    {
        builder.ToTable("route_plans");
        builder.HasKey(routePlan => routePlan.Id);

        builder.Property(routePlan => routePlan.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        builder.Property(routePlan => routePlan.AccountId)
            .HasColumnName("account_id");
        builder.Property(routePlan => routePlan.Request)
            .HasColumnName("request")
            .HasColumnType("jsonb")
            .HasConversion(RoutePlanSnapshotSerializer.RequestConverter);
        builder.Property(routePlan => routePlan.Request).Metadata.SetValueComparer(
            RoutePlanSnapshotSerializer.RequestComparer);
        builder.Property(routePlan => routePlan.Alternatives)
            .HasColumnName("alternatives")
            .HasColumnType("jsonb")
            .HasConversion(RoutePlanSnapshotSerializer.AlternativesConverter);
        builder.Property(routePlan => routePlan.Alternatives).Metadata.SetValueComparer(
            RoutePlanSnapshotSerializer.AlternativesComparer);
        builder.Property(routePlan => routePlan.CreatedAt)
            .HasColumnName("created_at");
        builder.Property(routePlan => routePlan.UpdatedAt)
            .HasColumnName("updated_at");
        builder.Property(routePlan => routePlan.ExpiresAt)
            .HasColumnName("expires_at");

        builder.HasIndex(routePlan => routePlan.AccountId);
        builder.HasIndex(routePlan => routePlan.ExpiresAt);
        builder.Ignore(routePlan => routePlan.DomainEvents);
    }
}
