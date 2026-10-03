using AB.SmokPrzewodnik.Domain.Spatial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AB.SmokPrzewodnik.Infrastructure.Database.Mappings;

internal sealed class DataSourceMapping : IEntityTypeConfiguration<DataSource>
{
    public void Configure(EntityTypeBuilder<DataSource> builder)
    {
        builder.ToTable("data_sources");
        builder.HasKey(source => source.Id);
        builder.Property(source => source.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(source => source.ProviderCode)
            .HasColumnName("provider_code")
            .HasMaxLength(64)
            .HasConversion(MappingConversions.CodeConverter);
        builder.Property(source => source.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(256);
        builder.Property(source => source.SourceType)
            .HasColumnName("source_type")
            .HasMaxLength(64)
            .HasConversion(MappingConversions.CodeConverter);
        builder.Property(source => source.Website)
            .HasColumnName("website")
            .HasMaxLength(2048)
            .HasConversion(MappingConversions.NullableUriConverter);
        builder.Property(source => source.License)
            .HasColumnName("license")
            .HasColumnType("jsonb")
            .HasConversion(MappingConversions.JsonConverter<SourceLicenseMetadata>());
        builder.Property(source => source.License).Metadata.SetValueComparer(
            MappingConversions.JsonComparer<SourceLicenseMetadata>());
        builder.Property(source => source.DefaultConfidenceWeight)
            .HasColumnName("default_confidence_weight")
            .HasPrecision(5, 4);
        builder.Property(source => source.IsEnabled).HasColumnName("is_enabled");
        builder.Property(source => source.CreatedAt).HasColumnName("created_at");
        builder.Property(source => source.UpdatedAt).HasColumnName("updated_at");
        builder.HasIndex(source => source.ProviderCode).IsUnique();
        builder.Ignore(source => source.DomainEvents);
    }
}
