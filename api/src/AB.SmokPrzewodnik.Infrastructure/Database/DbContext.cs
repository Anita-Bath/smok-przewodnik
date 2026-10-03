using AB.SmokPrzewodnik.Domain.Planning;
using AB.SmokPrzewodnik.Domain.Profiles;
using AB.SmokPrzewodnik.Domain.Spatial;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace AB.SmokPrzewodnik.Infrastructure.Database;

internal sealed class DbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbSet<SpatialEntity> SpatialEntities => Set<SpatialEntity>();
    public DbSet<SourceAssertion> SourceAssertions => Set<SourceAssertion>();
    public DbSet<AccessibilityFact> AccessibilityFacts => Set<AccessibilityFact>();

    public DbSet<AccountAccessibilityProfile> AccessibilityProfiles =>
      Set<AccountAccessibilityProfile>();

    public DbSet<Itinerary> Itineraries => Set<Itinerary>();

    public DbSet<FeedItem> FeedItems => Set<FeedItem>();
    public DbSet<SuggestionProjection> Suggestions => Set<SuggestionProjection>();

    public DbSet<City> Cities => Set<City>();
    public DbSet<DataSource> DataSources => Set<DataSource>();

    private readonly IConfiguration _configuration;

    public DbContext(IConfiguration configuration, DbContextOptions<DbContext> options) : base(options)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var connString = _configuration.GetConnectionString("Default");
        ArgumentNullException.ThrowIfNull(connString);

        options.UseNpgsql(connString, builder =>
        {
            builder.UseNetTopologySuite();
        });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DbContext).Assembly);
    }
}
