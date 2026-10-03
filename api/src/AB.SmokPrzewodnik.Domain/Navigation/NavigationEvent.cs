using System.Collections.Frozen;
using System.Collections.ObjectModel;
using AB.SmokPrzewodnik.Domain.Common;
using AB.SmokPrzewodnik.Domain.Routing;
using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Domain.Navigation;

public sealed record NavigationEvent
{
    public NavigationEvent(
        Guid sessionId,
        long sequence,
        NavigationEventType eventType,
        NavigationUrgency urgency,
        Maneuver? maneuver,
        Code? hazardCode,
        Code? landmarkCode,
        decimal? remainingDistanceMetres,
        IReadOnlyDictionary<string, string> localizedParameters,
        IEnumerable<Code> supportedFeedbackPatterns)
    {
        if (sessionId == Guid.Empty)
        {
            throw new ArgumentException("The navigation session identifier cannot be empty.", nameof(sessionId));
        }

        if (sequence <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sequence));
        }

        if (remainingDistanceMetres < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(remainingDistanceMetres));
        }

        ArgumentNullException.ThrowIfNull(localizedParameters);
        ArgumentNullException.ThrowIfNull(supportedFeedbackPatterns);
        SessionId = sessionId;
        Sequence = sequence;
        EventType = eventType;
        Urgency = urgency;
        Maneuver = maneuver;
        HazardCode = hazardCode;
        LandmarkCode = landmarkCode;
        RemainingDistanceMetres = remainingDistanceMetres;
        LocalizedParameters = new ReadOnlyDictionary<string, string>(localizedParameters.ToDictionary(
            pair => Guard.NotBlank(pair.Key, nameof(localizedParameters)),
            pair => Guard.NotBlank(pair.Value, nameof(localizedParameters)),
            StringComparer.Ordinal));
        SupportedFeedbackPatterns = supportedFeedbackPatterns.ToFrozenSet();
    }

    public Guid SessionId { get; }
    public long Sequence { get; }
    public NavigationEventType EventType { get; }
    public NavigationUrgency Urgency { get; }
    public Maneuver? Maneuver { get; }
    public Code? HazardCode { get; }
    public Code? LandmarkCode { get; }
    public decimal? RemainingDistanceMetres { get; }
    public IReadOnlyDictionary<string, string> LocalizedParameters { get; }
    public IReadOnlySet<Code> SupportedFeedbackPatterns { get; }
}
