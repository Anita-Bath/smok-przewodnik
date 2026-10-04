using AB.SmokPrzewodnik.Domain.Navigation;
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

        builder.HasIndex(x => x.Hash)
            .IsUnique();
    }
}
