using AB.SmokPrzewodnik.Domain.Planning;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AB.SmokPrzewodnik.Infrastructure.Database.Mappings;

internal sealed class FeedItemMapping : IEntityTypeConfiguration<FeedItem>
{
    public void Configure(EntityTypeBuilder<FeedItem> builder)
    {
        builder.ToTable("feed_items");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(item => item.CityId).HasColumnName("city_id");
        builder.Property(item => item.ContentType)
            .HasColumnName("content_type")
            .HasMaxLength(64)
            .HasConversion(MappingConversions.CodeConverter);
        builder.Property(item => item.SourceId).HasColumnName("source_id");
        builder.Ignore(item => item.LinkedEntityIds);
        builder.Ignore(item => item.LinkedEntities);
        builder.Property(item => item.LocalizedContent)
            .HasColumnName("localized_content")
            .HasColumnType("jsonb")
            .HasConversion(MappingConversions.JsonConverter<LocalizedContent>());
        builder.Property(item => item.LocalizedContent).Metadata.SetValueComparer(
            MappingConversions.JsonComparer<LocalizedContent>());
        builder.Property(item => item.PublishedAt).HasColumnName("published_at");
        builder.Property(item => item.ValidUntil).HasColumnName("valid_until");
        builder.Property(item => item.AudienceTags)
            .HasColumnName("audience_tags")
            .HasColumnType("jsonb")
            .HasConversion(MappingConversions.JsonConverter<IReadOnlySet<Code>>());
        builder.Property(item => item.AudienceTags).Metadata.SetValueComparer(
            MappingConversions.JsonComparer<IReadOnlySet<Code>>());
        builder.Property(item => item.Provenance)
            .HasColumnName("provenance")
            .HasMaxLength(64)
            .HasConversion(MappingConversions.CodeConverter);

        builder.HasOne(item => item.City)
            .WithMany(city => city.FeedItems)
            .HasForeignKey(item => item.CityId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(item => item.Source)
            .WithMany(source => source.FeedItems)
            .HasForeignKey(item => item.SourceId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(item => item.EntityLinks)
            .WithOne(link => link.FeedItem)
            .HasForeignKey(link => link.FeedItemId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(item => item.EntityLinks)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(item => new { item.CityId, item.PublishedAt });
    }
}

internal sealed class FeedItemEntityLinkMapping : IEntityTypeConfiguration<FeedItemEntityLink>
{
    public void Configure(EntityTypeBuilder<FeedItemEntityLink> builder)
    {
        builder.ToTable("feed_item_entities");
        builder.HasKey(link => new { link.FeedItemId, link.EntityId });
        builder.Property(link => link.FeedItemId).HasColumnName("feed_item_id");
        builder.Property(link => link.EntityId).HasColumnName("entity_id");
        builder.HasOne(link => link.Entity)
            .WithMany(entity => entity.FeedItemLinks)
            .HasForeignKey(link => link.EntityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
