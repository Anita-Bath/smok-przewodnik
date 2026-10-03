using AB.SmokPrzewodnik.Domain.Spatial;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AB.SmokPrzewodnik.Infrastructure.Database.Mappings;

internal sealed class CityMapping : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.ToTable("cities");
        builder.HasKey(city => city.Id);
        builder.Property(city => city.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(city => city.Code)
            .HasColumnName("code")
            .HasMaxLength(64)
            .HasConversion(MappingConversions.CodeConverter);
        builder.Property(city => city.Name)
            .HasColumnName("name")
            .HasColumnType("jsonb")
            .HasConversion(MappingConversions.JsonConverter<LocalizedContent>());
        builder.Property(city => city.Name).Metadata.SetValueComparer(
            MappingConversions.JsonComparer<LocalizedContent>());
        builder.Property(city => city.DefaultLocale)
            .HasColumnName("default_locale")
            .HasMaxLength(16);
        builder.Property(city => city.TimeZone)
            .HasColumnName("time_zone")
            .HasMaxLength(128);
        builder.Property(city => city.IsActive).HasColumnName("is_active");
        builder.Property(city => city.CreatedAt).HasColumnName("created_at");
        builder.Property(city => city.UpdatedAt).HasColumnName("updated_at");
        builder.HasIndex(city => city.Code).IsUnique();
        builder.Ignore(city => city.DomainEvents);
    }
}
