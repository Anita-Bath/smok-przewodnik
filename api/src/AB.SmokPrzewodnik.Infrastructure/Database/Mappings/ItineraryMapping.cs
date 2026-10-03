using AB.SmokPrzewodnik.Domain.Planning;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AB.SmokPrzewodnik.Infrastructure.Database.Mappings;

internal sealed class ItineraryMapping : IEntityTypeConfiguration<Itinerary>
{
    public void Configure(EntityTypeBuilder<Itinerary> builder)
    {
        builder.ToTable("itineraries");
        builder.HasKey(itinerary => itinerary.Id);
        builder.Property(itinerary => itinerary.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(itinerary => itinerary.AccountId).HasColumnName("account_id");
        builder.Property(itinerary => itinerary.LocalizedTitle)
            .HasColumnName("localized_title")
            .HasColumnType("jsonb")
            .HasConversion(MappingConversions.JsonConverter<LocalizedContent>());
        builder.Property(itinerary => itinerary.LocalizedTitle).Metadata.SetValueComparer(
            MappingConversions.JsonComparer<LocalizedContent>());
        builder.Property(itinerary => itinerary.StartsAt).HasColumnName("starts_at");
        builder.Property(itinerary => itinerary.EndsAt).HasColumnName("ends_at");
        builder.Property(itinerary => itinerary.TimeZone)
            .HasColumnName("time_zone")
            .HasMaxLength(128);
        builder.Property<List<Guid>>("_routeLegReferences")
            .HasColumnName("route_leg_references")
            .HasColumnType("jsonb")
            .HasConversion(MappingConversions.JsonConverter<List<Guid>>());
        builder.Property<List<Guid>>("_routeLegReferences").Metadata.SetValueComparer(
            MappingConversions.JsonComparer<List<Guid>>());
        builder.Property(itinerary => itinerary.CreatedAt).HasColumnName("created_at");
        builder.Property(itinerary => itinerary.UpdatedAt).HasColumnName("updated_at");
        builder.Ignore(itinerary => itinerary.RouteLegReferences);
        builder.Ignore(itinerary => itinerary.DomainEvents);

        builder.HasMany(itinerary => itinerary.Items)
            .WithOne()
            .HasForeignKey("itinerary_id")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(itinerary => itinerary.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(itinerary => new { itinerary.AccountId, itinerary.StartsAt });
    }
}

internal sealed class ItineraryItemMapping : IEntityTypeConfiguration<ItineraryItem>
{
    public void Configure(EntityTypeBuilder<ItineraryItem> builder)
    {
        builder.ToTable("itinerary_items");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property<Guid>("itinerary_id").HasColumnName("itinerary_id");
        builder.Property(item => item.Position).HasColumnName("position");
        builder.Property(item => item.EntityId).HasColumnName("entity_id");
        builder.Property(item => item.PlannedArrival).HasColumnName("planned_arrival");
        builder.Property(item => item.PlannedDeparture).HasColumnName("planned_departure");
        builder.Property(item => item.Note).HasColumnName("note").HasMaxLength(1024);
        builder.HasOne(item => item.Entity)
            .WithMany(entity => entity.ItineraryItems)
            .HasForeignKey(item => item.EntityId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex("itinerary_id", nameof(ItineraryItem.Position)).IsUnique();
    }
}
