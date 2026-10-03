using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.Spatial;
using AB.SmokPrzewodnik.Domain.Spatial.Details;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AB.SmokPrzewodnik.Infrastructure.Database.Mappings;

internal sealed class SpatialEntityMapping : IEntityTypeConfiguration<SpatialEntity>
{
    public void Configure(EntityTypeBuilder<SpatialEntity> builder)
    {
        builder.ToTable("spatial_entities");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(entity => entity.CityId).HasColumnName("city_id");
        builder.Property(entity => entity.Kind)
            .HasColumnName("kind")
            .HasConversion<string>()
            .HasMaxLength(32);
        builder.Property(entity => entity.Geometry)
            .HasColumnName("geometry")
            .HasColumnType("geometry(Geometry,4326)")
            .HasConversion(MappingConversions.SpatialGeometryConverter);
        builder.Property(entity => entity.Geometry).Metadata.SetValueComparer(
            MappingConversions.SpatialGeometryComparer);
        builder.Property(entity => entity.State)
            .HasColumnName("lifecycle_state")
            .HasConversion<string>()
            .HasMaxLength(32);
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at");
        builder.Property(entity => entity.UpdatedAt).HasColumnName("updated_at");

        builder.ComplexProperty(entity => entity.Confidence, confidence =>
        {
            confidence.Property(value => value.State)
                .HasColumnName("confidence_state")
                .HasConversion<string>()
                .HasMaxLength(32);
            confidence.Property(value => value.Score)
                .HasColumnName("confidence_score")
                .HasPrecision(5, 4);
            confidence.Property(value => value.EvidenceCount)
                .HasColumnName("confidence_evidence_count");
            confidence.Property(value => value.EvaluatedAt)
                .HasColumnName("confidence_evaluated_at");
        });

        builder.HasOne(entity => entity.City)
            .WithMany(city => city.SpatialEntities)
            .HasForeignKey(entity => entity.CityId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Details)
            .WithOne()
            .HasForeignKey<SpatialEntityDetails>(details => details.SpatialEntityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(entity => entity.Translations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(entity => entity.SourceLinks)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(entity => entity.AccessibilityFacts)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(entity => entity.ItineraryItems)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Ignore(entity => entity.FeedItems);
        builder.Navigation(entity => entity.FeedItemLinks)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(entity => entity.Suggestions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Ignore(entity => entity.DomainEvents);

        builder.HasIndex(entity => new { entity.CityId, entity.Kind });
        builder.HasIndex(entity => entity.Geometry).HasMethod("gist");
        builder.HasIndex(entity => new { entity.UpdatedAt, entity.Id });
    }
}

internal sealed class SpatialEntityDetailsMapping : IEntityTypeConfiguration<SpatialEntityDetails>
{
    public void Configure(EntityTypeBuilder<SpatialEntityDetails> builder)
    {
        builder.ToTable("spatial_entity_details");
        builder.Property(details => details.SpatialEntityId).HasColumnName("spatial_entity_id");
        builder.HasKey(details => details.SpatialEntityId);
        builder.Ignore(details => details.Kind);
        builder.HasDiscriminator<string>("detail_type")
            .HasValue<PlaceDetails>("place")
            .HasValue<EventDetails>("event")
            .HasValue<InfrastructureDetails>("infrastructure")
            .HasValue<ObstacleDetails>("obstacle");
    }
}

internal sealed class PlaceDetailsMapping : IEntityTypeConfiguration<PlaceDetails>
{
    public void Configure(EntityTypeBuilder<PlaceDetails> builder)
    {
        builder.Property(details => details.CategoryCode)
            .HasColumnName("place_category_code")
            .HasMaxLength(64)
            .HasConversion(MappingConversions.CodeConverter);
        builder.Property(details => details.OpeningHours)
            .HasColumnName("place_opening_hours")
            .HasMaxLength(512);
        builder.Property(details => details.Contact)
            .HasColumnName("place_contact")
            .HasColumnType("jsonb")
            .HasConversion(MappingConversions.NullableJsonConverter<ContactDetails>());
        builder.Property(details => details.Contact).Metadata.SetValueComparer(
            MappingConversions.NullableJsonComparer<ContactDetails>());
        builder.Property(details => details.Website)
            .HasColumnName("place_website")
            .HasMaxLength(2048)
            .HasConversion(MappingConversions.NullableUriConverter);
    }
}

internal sealed class EventDetailsMapping : IEntityTypeConfiguration<EventDetails>
{
    public void Configure(EntityTypeBuilder<EventDetails> builder)
    {
        builder.Ignore(details => details.CategoryCodes);
        builder.HasMany(details => details.Categories)
            .WithOne()
            .HasForeignKey(category => category.SpatialEntityId)
            .HasPrincipalKey(details => details.SpatialEntityId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(details => details.Categories)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
        builder.Property(details => details.OrganizerEntityId)
            .HasColumnName("event_organizer_entity_id");
        builder.Property(details => details.StartsAt).HasColumnName("event_starts_at");
        builder.Property(details => details.EndsAt).HasColumnName("event_ends_at");
        builder.Property(details => details.BookingUri)
            .HasColumnName("event_booking_uri")
            .HasMaxLength(2048)
            .HasConversion(MappingConversions.NullableUriConverter);
        builder.Property(details => details.Capacity).HasColumnName("event_capacity");
        builder.HasOne(details => details.Organizer)
            .WithMany()
            .HasForeignKey(details => details.OrganizerEntityId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(details => new { details.StartsAt, details.EndsAt });
    }
}

internal sealed class EventCategoryMapping : IEntityTypeConfiguration<EventCategory>
{
    public void Configure(EntityTypeBuilder<EventCategory> builder)
    {
        builder.ToTable("event_categories");
        builder.Property(category => category.SpatialEntityId).HasColumnName("spatial_entity_id");
        builder.Property(category => category.Code)
            .HasColumnName("category_code")
            .HasMaxLength(64)
            .HasConversion(MappingConversions.CodeConverter);
        builder.HasKey(category => new { category.SpatialEntityId, category.Code });
    }
}

internal sealed class InfrastructureDetailsMapping : IEntityTypeConfiguration<InfrastructureDetails>
{
    public void Configure(EntityTypeBuilder<InfrastructureDetails> builder)
    {
        builder.Property(details => details.InfrastructureCode)
            .HasColumnName("infrastructure_code")
            .HasMaxLength(64)
            .HasConversion(MappingConversions.CodeConverter);
        builder.Property(details => details.OperationalState)
            .HasColumnName("infrastructure_operational_state")
            .HasMaxLength(64)
            .HasConversion(MappingConversions.CodeConverter);
        builder.Property(details => details.MaintenanceReference)
            .HasColumnName("infrastructure_maintenance_reference")
            .HasMaxLength(256);
    }
}

internal sealed class ObstacleDetailsMapping : IEntityTypeConfiguration<ObstacleDetails>
{
    public void Configure(EntityTypeBuilder<ObstacleDetails> builder)
    {
        builder.Property(details => details.ObstacleCode)
            .HasColumnName("obstacle_code")
            .HasMaxLength(64)
            .HasConversion(MappingConversions.CodeConverter);
        builder.Property(details => details.Severity).HasColumnName("obstacle_severity");
        builder.Property(details => details.ExpectedUntil).HasColumnName("obstacle_expected_until");
        builder.Property(details => details.AffectedTravelModes)
            .HasColumnName("obstacle_affected_travel_modes")
            .HasColumnType("jsonb")
            .HasConversion(MappingConversions.JsonConverter<IReadOnlySet<TravelMode>>());
        builder.Property(details => details.AffectedTravelModes).Metadata.SetValueComparer(
            MappingConversions.JsonComparer<IReadOnlySet<TravelMode>>());
    }
}

internal sealed class EntityTranslationMapping : IEntityTypeConfiguration<EntityTranslation>
{
    public void Configure(EntityTypeBuilder<EntityTranslation> builder)
    {
        builder.ToTable("entity_translations");
        builder.HasKey(translation => translation.Id);
        builder.Property(translation => translation.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(translation => translation.EntityId).HasColumnName("entity_id");
        builder.Property(translation => translation.Locale).HasColumnName("locale").HasMaxLength(16);
        builder.Property(translation => translation.Name).HasColumnName("name").HasMaxLength(256);
        builder.Property(translation => translation.Description).HasColumnName("description");
        builder.HasOne(translation => translation.Entity)
            .WithMany(entity => entity.Translations)
            .HasForeignKey(translation => translation.EntityId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(translation => new { translation.EntityId, translation.Locale }).IsUnique();
    }
}

internal sealed class EntitySourceLinkMapping : IEntityTypeConfiguration<EntitySourceLink>
{
    public void Configure(EntityTypeBuilder<EntitySourceLink> builder)
    {
        builder.ToTable("entity_source_links");
        builder.HasKey(link => link.Id);
        builder.Property(link => link.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(link => link.EntityId).HasColumnName("entity_id");
        builder.Property(link => link.SourceId).HasColumnName("source_id");
        builder.Property(link => link.ExternalId).HasColumnName("external_id").HasMaxLength(256);
        builder.Property(link => link.RetrievedAt).HasColumnName("retrieved_at");
        builder.Property(link => link.License)
            .HasColumnName("license")
            .HasColumnType("jsonb")
            .HasConversion(MappingConversions.JsonConverter<SourceLicenseMetadata>());
        builder.Property(link => link.License).Metadata.SetValueComparer(
            MappingConversions.JsonComparer<SourceLicenseMetadata>());
        builder.Property(link => link.TransformVersion)
            .HasColumnName("transform_version")
            .HasMaxLength(64);
        builder.Property(link => link.CreatedAt).HasColumnName("created_at");
        builder.Property(link => link.UpdatedAt).HasColumnName("updated_at");
        builder.HasOne(link => link.Entity)
            .WithMany(entity => entity.SourceLinks)
            .HasForeignKey(link => link.EntityId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(link => link.Source)
            .WithMany(source => source.SourceLinks)
            .HasForeignKey(link => link.SourceId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(link => new { link.SourceId, link.ExternalId }).IsUnique();
    }
}
