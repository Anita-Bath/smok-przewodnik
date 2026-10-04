using AB.SmokPrzewodnik.Domain.Common;
using AB.SmokPrzewodnik.Domain.Routing;
using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Domain.Navigation;

public sealed class NavigationSession : AggregateRoot<Guid>
{
    private NavigationSession()
    {
        Hash = null!;
        ActiveRoute = null!;
        EffectiveRouteRequest = null!;
    }

    public NavigationSession(
        Guid id,
        string hash,
        Guid? accountId,
        RouteAlternative activeRoute,
        RoutePlanRequest effectiveRouteRequest,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt)
        : base(ValidateId(id), createdAt)
    {
        if (string.IsNullOrWhiteSpace(hash))
        {
            throw new ArgumentException("The navigation token hash cannot be blank.", nameof(hash));
        }

        if (expiresAt <= createdAt)
        {
            throw new ArgumentException("A navigation session must expire after it is created.", nameof(expiresAt));
        }

        Hash = hash;
        AccountId = accountId;
        ActiveRoute = activeRoute ?? throw new ArgumentNullException(nameof(activeRoute));
        EffectiveRouteRequest = effectiveRouteRequest ?? throw new ArgumentNullException(nameof(effectiveRouteRequest));
        ExpiresAt = expiresAt;
        NextEventSequence = 1;
    }

    public string Hash { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public Guid? AccountId { get; private set; }
    public RouteAlternative ActiveRoute { get; private set; }
    public RoutePlanRequest EffectiveRouteRequest { get; private set; }
    public NavigationProgress? LatestProgress { get; private set; }
    public long NextEventSequence { get; private set; }
    public long Version { get; private set; }

    public bool IsExpired(DateTimeOffset now) => now >= ExpiresAt;

    public bool TryUpdateProgress(NavigationProgress progress, DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(progress);
        if (LatestProgress is not null && progress.RecordedAt <= LatestProgress.RecordedAt)
        {
            return false;
        }

        LatestProgress = progress;
        Version++;
        MarkUpdated(updatedAt);
        return true;
    }

    public NavigationEvent AppendEvent(
        NavigationEventType eventType,
        NavigationUrgency urgency,
        Maneuver? maneuver,
        Code? hazardCode,
        Code? landmarkCode,
        decimal? remainingDistanceMetres,
        IReadOnlyDictionary<string, string> localizedParameters,
        IEnumerable<Code> supportedFeedbackPatterns,
        DateTimeOffset createdAt)
    {
        var navigationEvent = new NavigationEvent(
            Id,
            NextEventSequence++,
            eventType,
            urgency,
            maneuver,
            hazardCode,
            landmarkCode,
            remainingDistanceMetres,
            localizedParameters,
            supportedFeedbackPatterns,
            createdAt);
        Version++;
        MarkUpdated(createdAt);
        return navigationEvent;
    }

    public bool ReplaceRoute(
        RouteAlternative route,
        RoutePlanRequest request,
        DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(route);
        ArgumentNullException.ThrowIfNull(request);
        if (ActiveRoute.Id == route.Id)
        {
            return false;
        }

        ActiveRoute = route;
        EffectiveRouteRequest = request;
        if (LatestProgress is not null)
        {
            LatestProgress = LatestProgress with
            {
                RouteLegId = null,
                ManeuverIndex = null,
                RemainingDistanceMetres = null,
                IsOffRoute = false
            };
        }

        Version++;
        MarkUpdated(updatedAt);
        return true;
    }

    private static Guid ValidateId(Guid id) => id == Guid.Empty
        ? throw new ArgumentException("The navigation session identifier cannot be empty.", nameof(id))
        : id;
}
