using AB.SmokPrzewodnik.Domain.Profiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AB.SmokPrzewodnik.Infrastructure.Database.Mappings;

internal sealed class AccountAccessibilityProfileMapping
    : IEntityTypeConfiguration<AccountAccessibilityProfile>
{
    public void Configure(EntityTypeBuilder<AccountAccessibilityProfile> builder)
    {
        builder.ToTable("accessibility_profiles");
        builder.HasKey(profile => profile.Id);
        builder.Property(profile => profile.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(profile => profile.AccountId).HasColumnName("account_id");
        builder.Property(profile => profile.Settings)
            .HasColumnName("settings")
            .HasColumnType("jsonb")
            .HasConversion(MappingConversions.JsonConverter<AccessibilityProfileSettings>());
        builder.Property(profile => profile.Settings).Metadata.SetValueComparer(
            MappingConversions.JsonComparer<AccessibilityProfileSettings>());
        builder.Property(profile => profile.JourneyHistorySyncEnabled)
            .HasColumnName("journey_history_sync_enabled");
        builder.Property(profile => profile.ServerVersion).HasColumnName("server_version");
        builder.Property(profile => profile.CreatedAt).HasColumnName("created_at");
        builder.Property(profile => profile.UpdatedAt).HasColumnName("updated_at");
        builder.HasIndex(profile => profile.AccountId).IsUnique();
        builder.Ignore(profile => profile.DomainEvents);
    }
}
