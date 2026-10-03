using System.Collections.ObjectModel;
using AB.SmokPrzewodnik.Domain.Common;
using AB.SmokPrzewodnik.Domain.Confidence;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.Planning;
using AB.SmokPrzewodnik.Domain.Spatial.Details;
using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Domain.Spatial;

public sealed class SpatialEntity : AggregateRoot<Guid>
{
    private readonly List<EntityTranslation> _translations = [];
    private readonly List<EntitySourceLink> _sourceLinks = [];
    private readonly List<AccessibilityFact> _accessibilityFacts = [];
    private readonly List<ItineraryItem> _itineraryItems = [];
    private readonly List<FeedItem> _feedItems = [];
    private readonly List<SuggestionProjection> _suggestions = [];
    private readonly ReadOnlyCollection<EntityTranslation> _translationView;
    private readonly ReadOnlyCollection<EntitySourceLink> _sourceLinkView;
    private readonly ReadOnlyCollection<AccessibilityFact> _accessibilityFactView;
    private readonly ReadOnlyCollection<ItineraryItem> _itineraryItemView;
    private readonly ReadOnlyCollection<FeedItem> _feedItemView;
    private readonly ReadOnlyCollection<SuggestionProjection> _suggestionView;

    public SpatialEntity(
        Guid id,
        Guid cityId,
        EntityKind kind,
        SpatialGeometry geometry,
        LifecycleState state,
        ConfidenceAssessment confidence,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? updatedAt = null)
        : base(id, createdAt, updatedAt)
    {
        if (cityId == Guid.Empty)
        {
            throw new ArgumentException("City ID cannot be empty.", nameof(cityId));
        }

        CityId = cityId;
        Kind = kind;
        Geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
        State = state;
        Confidence = confidence ?? throw new ArgumentNullException(nameof(confidence));
        _translationView = _translations.AsReadOnly();
        _sourceLinkView = _sourceLinks.AsReadOnly();
        _accessibilityFactView = _accessibilityFacts.AsReadOnly();
        _itineraryItemView = _itineraryItems.AsReadOnly();
        _feedItemView = _feedItems.AsReadOnly();
        _suggestionView = _suggestions.AsReadOnly();
    }

    public Guid CityId { get; }
    public City City { get; private set; } = null!;
    public EntityKind Kind { get; }
    public SpatialGeometry Geometry { get; private set; }
    public LifecycleState State { get; private set; }
    public ConfidenceAssessment Confidence { get; private set; }
    public SpatialEntityDetails? Details { get; private set; }
    public IReadOnlyCollection<EntityTranslation> Translations => _translationView;
    public IReadOnlyCollection<EntitySourceLink> SourceLinks => _sourceLinkView;
    public IReadOnlyCollection<AccessibilityFact> AccessibilityFacts => _accessibilityFactView;
    public IReadOnlyCollection<ItineraryItem> ItineraryItems => _itineraryItemView;
    public IReadOnlyCollection<FeedItem> FeedItems => _feedItemView;
    public IReadOnlyCollection<SuggestionProjection> Suggestions => _suggestionView;

    public static SpatialEntity Create(
        Guid cityId,
        EntityKind kind,
        SpatialGeometry geometry,
        ConfidenceAssessment confidence) =>
        new(Guid.NewGuid(), cityId, kind, geometry, LifecycleState.Active, confidence);

    public void SetDetails(SpatialEntityDetails details, DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(details);
        if (details.Kind != Kind)
        {
            throw new ArgumentException("Detail type must match the spatial entity kind.", nameof(details));
        }

        Details = details;
        MarkUpdated(updatedAt);
    }

    public void UpsertTranslation(EntityTranslation translation, DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(translation);
        if (translation.EntityId != Id)
        {
            throw new ArgumentException("Translation belongs to a different spatial entity.", nameof(translation));
        }

        var existing = _translations.FindIndex(item =>
            string.Equals(item.Locale, translation.Locale, StringComparison.OrdinalIgnoreCase));
        if (existing >= 0)
        {
            _translations[existing] = translation;
        }
        else
        {
            _translations.Add(translation);
        }

        MarkUpdated(updatedAt);
    }

    public void AddSourceLink(EntitySourceLink sourceLink, DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(sourceLink);
        if (sourceLink.EntityId != Id)
        {
            throw new ArgumentException("Source link belongs to a different spatial entity.", nameof(sourceLink));
        }

        if (_sourceLinks.Any(item => item.SourceId == sourceLink.SourceId &&
            string.Equals(item.ExternalId, sourceLink.ExternalId, StringComparison.Ordinal)))
        {
            throw new InvalidOperationException("The external source record is already linked.");
        }

        _sourceLinks.Add(sourceLink);
        MarkUpdated(updatedAt);
    }

    public void ChangeLifecycle(LifecycleState state, DateTimeOffset updatedAt)
    {
        State = state;
        MarkUpdated(updatedAt);
    }

    public void UpdateConfidence(ConfidenceAssessment confidence, DateTimeOffset updatedAt)
    {
        Confidence = confidence ?? throw new ArgumentNullException(nameof(confidence));
        MarkUpdated(updatedAt);
    }

    public void UpdateGeometry(SpatialGeometry geometry, DateTimeOffset updatedAt)
    {
        Geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
        MarkUpdated(updatedAt);
    }
}
