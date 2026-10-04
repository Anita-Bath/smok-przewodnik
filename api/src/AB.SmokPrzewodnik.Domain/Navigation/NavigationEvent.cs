using System.Collections.Frozen;
using System.Collections.ObjectModel;
using AB.SmokPrzewodnik.Domain.Common;
using AB.SmokPrzewodnik.Domain.Routing;
using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Domain.Navigation;

public sealed record NavigationEvent
{
    private NavigationEvent()
    {
        LocalizedParameters = null!;
        SupportedFeedbackPatterns = null!;
    }

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
        IEnumerable<Code> supportedFeedbackPatterns,
        DateTimeOffset? createdAt = null)
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
        CreatedAt = createdAt ?? DateTimeOffset.UtcNow;
    }

    public Guid SessionId { get; private set; }
    public long Sequence { get; private set; }
    public NavigationEventType EventType { get; private set; }
    public NavigationUrgency Urgency { get; private set; }
    public Maneuver? Maneuver { get; private set; }
    public Code? HazardCode { get; private set; }
    public Code? LandmarkCode { get; private set; }
    public decimal? RemainingDistanceMetres { get; private set; }
    public IReadOnlyDictionary<string, string> LocalizedParameters { get; private set; }
    public IReadOnlySet<Code> SupportedFeedbackPatterns { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
}
