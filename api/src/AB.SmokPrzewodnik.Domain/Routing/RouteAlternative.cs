using System.Collections.Frozen;
using System.Collections.ObjectModel;
using AB.SmokPrzewodnik.Domain.Confidence;
using AB.SmokPrzewodnik.Domain.Common;
using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Domain.Routing;

public sealed record RouteAlternative
{
    public RouteAlternative(
        Guid id,
        Code labelKey,
        SpatialGeometry geometry,
        IEnumerable<RouteLeg> legs,
        TimeSpan duration,
        decimal distanceMetres,
        IEnumerable<CostComponent> generalizedCostBreakdown,
        AccessibilitySummary accessibilitySummary,
        IEnumerable<Code> preferenceTradeoffs,
        ConfidenceAssessment confidenceSummary,
        IReadOnlyDictionary<Code, string> sourceVersions,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("The route identifier cannot be empty.", nameof(id));
        }

        if (duration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration));
        }

        if (distanceMetres < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(distanceMetres));
        }

        if (expiresAt <= createdAt)
        {
            throw new ArgumentException("A route must expire after it is created.", nameof(expiresAt));
        }

        ArgumentNullException.ThrowIfNull(geometry);
        ArgumentNullException.ThrowIfNull(legs);
        var orderedLegs = legs.ToList();
        if (orderedLegs.Count == 0)
        {
            throw new ArgumentException("A route requires at least one leg.", nameof(legs));
        }

        if (orderedLegs.Select(leg => leg.Id).Distinct().Count() != orderedLegs.Count ||
            orderedLegs.Where((leg, index) => leg.Position != index).Any())
        {
            throw new ArgumentException("Route legs must be unique and have contiguous positions.", nameof(legs));
        }

        ArgumentNullException.ThrowIfNull(generalizedCostBreakdown);
        ArgumentNullException.ThrowIfNull(preferenceTradeoffs);
        ArgumentNullException.ThrowIfNull(sourceVersions);

        Id = id;
        LabelKey = labelKey;
        Geometry = geometry;
        Legs = orderedLegs.AsReadOnly();
        Duration = duration;
        DistanceMetres = distanceMetres;
        GeneralizedCostBreakdown = generalizedCostBreakdown.ToList().AsReadOnly();
        AccessibilitySummary = accessibilitySummary ?? throw new ArgumentNullException(nameof(accessibilitySummary));
        PreferenceTradeoffs = preferenceTradeoffs.ToFrozenSet();
        ConfidenceSummary = confidenceSummary ?? throw new ArgumentNullException(nameof(confidenceSummary));
        SourceVersions = new ReadOnlyDictionary<Code, string>(sourceVersions.ToDictionary(
            pair => pair.Key,
            pair => Guard.NotBlank(pair.Value, nameof(sourceVersions))));
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }

    public Guid Id { get; }
    public Code LabelKey { get; }
    public SpatialGeometry Geometry { get; }
    public IReadOnlyList<RouteLeg> Legs { get; }
    public TimeSpan Duration { get; }
    public decimal DistanceMetres { get; }
    public IReadOnlyList<CostComponent> GeneralizedCostBreakdown { get; }
    public AccessibilitySummary AccessibilitySummary { get; }
    public IReadOnlySet<Code> PreferenceTradeoffs { get; }
    public ConfidenceAssessment ConfidenceSummary { get; }
    public IReadOnlyDictionary<Code, string> SourceVersions { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset ExpiresAt { get; }
}
