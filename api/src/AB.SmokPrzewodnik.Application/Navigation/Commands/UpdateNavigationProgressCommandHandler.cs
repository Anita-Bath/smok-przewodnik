using AB.SmokPrzewodnik.Application.Navigation.Dtos;
using AB.SmokPrzewodnik.Application.Spatials.Dtos;
using AB.SmokPrzewodnik.Application.Navigation.Mappers;
using AB.SmokPrzewodnik.Domain.Navigation;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using MediatR;

namespace AB.SmokPrzewodnik.Application.Navigation.Commands;

public sealed class UpdateNavigationProgressCommandHandler(
    INavigationSessionRepository sessions,
    INavigationProgressMatcher matcher,
    TimeProvider timeProvider,
    INavigationEventPublisher? eventPublisher = null) : IRequestHandler<UpdateNavigationProgressCommand, NavigationProgressResponse>
{
    private const int MaximumAttempts = 3;

    public async Task<NavigationProgressResponse> Handle(
        UpdateNavigationProgressCommand command,
        CancellationToken cancellationToken)
    {
        Validate(command.Progress, timeProvider.GetUtcNow());

        for (var attempt = 1; attempt <= MaximumAttempts; attempt++)
        {
            var session = await sessions.GetByIdAsync(command.SessionId, cancellationToken)
                ?? throw new NavigationSessionNotFoundException(command.SessionId);
            var now = timeProvider.GetUtcNow();
            if (session.IsExpired(now))
            {
                throw new NavigationSessionExpiredException(session.Id);
            }

            ValidateHint(session, command.Progress);
            if (session.LatestProgress is not null &&
                command.Progress.RecordedAt <= session.LatestProgress.RecordedAt)
            {
                var previous = session.LatestProgress;
                return Response(previous, previous.Position, true, session.NextEventSequence - 1);
            }

            var observed = new NavigationProgress(
                new GeoCoordinate(command.Progress.Position.Latitude, command.Progress.Position.Longitude),
                command.Progress.AccuracyMetres,
                command.Progress.HeadingDegrees,
                command.Progress.RecordedAt,
                command.Progress.RouteLegId,
                command.Progress.ManeuverIndex,
                null,
                false);
            var match = matcher.Match(session.ActiveRoute, observed);
            var progress = observed with
            {
                RemainingDistanceMetres = match.RemainingDistanceMetres,
                IsOffRoute = match.IsOffRoute
            };
            session.TryUpdateProgress(progress, now);
            var events = CreateEvents(session, match, now);

            try
            {
                await sessions.UpdateAsync(session, events, cancellationToken);
                foreach (var navigationEvent in events)
                {
                    try
                    {
                        await (eventPublisher ?? new NullNavigationEventPublisher()).PublishAsync(
                            session.Id,
                            NavigationEventMapper.ToDto(navigationEvent),
                            cancellationToken);
                    }
                    catch
                    {
                        // Durable HTTP recovery remains authoritative when live publication fails.
                    }
                }
                return Response(progress, match.MatchedPosition, false, session.NextEventSequence - 1);
            }
            catch (NavigationConcurrencyException) when (attempt < MaximumAttempts)
            {
                // Reload the aggregate and retry sequence allocation.
            }
        }

        throw new NavigationConcurrencyException();
    }

    private static IReadOnlyCollection<NavigationEvent> CreateEvents(
        NavigationSession session,
        NavigationProgressMatch match,
        DateTimeOffset createdAt)
    {
        if (match.RemainingDistanceMetres <= 15m)
        {
            return [session.AppendEvent(
                NavigationEventType.Arrival, NavigationUrgency.Immediate, null, null, null,
                match.RemainingDistanceMetres, new Dictionary<string, string>(),
                [new Code("audio"), new Code("haptic")], createdAt)];
        }

        if (match.IsOffRoute)
        {
            return [session.AppendEvent(
                NavigationEventType.Hazard, NavigationUrgency.Advisory, null, new Code("off_route"), null,
                match.RemainingDistanceMetres, new Dictionary<string, string>(),
                [new Code("audio"), new Code("haptic"), new Code("visual")], createdAt)];
        }

        return match.NextManeuver is null
            ? []
            : [session.AppendEvent(
                NavigationEventType.Maneuver, NavigationUrgency.Advisory, match.NextManeuver, null, null,
                match.RemainingDistanceMetres, new Dictionary<string, string>(),
                [new Code("audio"), new Code("haptic"), new Code("visual")], createdAt)];
    }

    private static void Validate(UpdateNavigationProgressRequest progress, DateTimeOffset now)
    {
        _ = new GeoCoordinate(progress.Position.Latitude, progress.Position.Longitude);
        if (progress.AccuracyMetres < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(progress.AccuracyMetres));
        }

        if (progress.HeadingDegrees is < 0 or >= 360)
        {
            throw new ArgumentOutOfRangeException(nameof(progress.HeadingDegrees));
        }

        if (progress.RecordedAt > now.AddMinutes(2))
        {
            throw new ArgumentException("The progress timestamp is implausibly far in the future.");
        }
    }

    private static void ValidateHint(NavigationSession session, UpdateNavigationProgressRequest progress)
    {
        if (!progress.RouteLegId.HasValue && !progress.ManeuverIndex.HasValue)
        {
            return;
        }

        var leg = progress.RouteLegId.HasValue
            ? session.ActiveRoute.Legs.SingleOrDefault(item => item.Id == progress.RouteLegId)
            : null;
        if (progress.RouteLegId.HasValue && leg is null ||
            progress.ManeuverIndex is { } index && (leg is null || index < 0 || index >= leg.Maneuvers.Count))
        {
            throw new ArgumentException("The route-relative progress hint is not part of the active route.");
        }
    }

    private static NavigationProgressResponse Response(
        NavigationProgress progress,
        GeoCoordinate matched,
        bool ignored,
        long latestSequence) => new(
        new GeoCoordinateDto(progress.Position.Latitude, progress.Position.Longitude),
        new GeoCoordinateDto(matched.Latitude, matched.Longitude),
        progress.RemainingDistanceMetres ?? 0,
        progress.IsOffRoute,
        ignored,
        latestSequence);
}
