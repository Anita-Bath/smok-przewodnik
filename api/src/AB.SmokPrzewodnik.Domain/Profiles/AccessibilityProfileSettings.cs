using System.Collections.Frozen;
using System.Collections.ObjectModel;
using AB.SmokPrzewodnik.Domain.Common;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Domain.Profiles;

public sealed class AccessibilityProfileSettings
{
    private readonly Dictionary<Code, ConstraintLevel> _constraints;
    private readonly ReadOnlyDictionary<Code, ConstraintLevel> _constraintsView;

    public AccessibilityProfileSettings(
        string preferredLocale,
        PresentationPreferences presentationPreferences,
        DateTimeOffset updatedAt,
        IReadOnlyDictionary<Code, ConstraintLevel>? constraints = null,
        IEnumerable<TravelMode>? transportCapabilities = null,
        IEnumerable<FeedbackChannel>? feedbackChannels = null)
    {
        PreferredLocale = Guard.NotBlank(preferredLocale, nameof(preferredLocale));
        PresentationPreferences = presentationPreferences ?? throw new ArgumentNullException(nameof(presentationPreferences));
        UpdatedAt = updatedAt;
        _constraints = constraints?.ToDictionary() ?? [];
        _constraintsView = _constraints.AsReadOnly();
        TransportCapabilities = (transportCapabilities ?? []).ToFrozenSet();
        FeedbackChannels = (feedbackChannels ?? []).ToFrozenSet();
    }

    public string PreferredLocale { get; private set; }

    public IReadOnlyDictionary<Code, ConstraintLevel> Constraints => _constraintsView;

    public IReadOnlySet<TravelMode> TransportCapabilities { get; private set; }

    public IReadOnlySet<FeedbackChannel> FeedbackChannels { get; private set; }

    public PresentationPreferences PresentationPreferences { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public void ChangePreferredLocale(string preferredLocale, DateTimeOffset updatedAt)
    {
        var validatedLocale = Guard.NotBlank(preferredLocale, nameof(preferredLocale));
        Touch(updatedAt);
        PreferredLocale = validatedLocale;
    }

    public void SetConstraint(Code code, ConstraintLevel level, DateTimeOffset updatedAt)
    {
        EnsureCanUpdate(updatedAt);
        _constraints[code] = level;
        UpdatedAt = updatedAt;
    }

    public bool RemoveConstraint(Code code, DateTimeOffset updatedAt)
    {
        EnsureCanUpdate(updatedAt);
        var removed = _constraints.Remove(code);

        if (removed)
        {
            UpdatedAt = updatedAt;
        }

        return removed;
    }

    public void ReplaceTransportCapabilities(IEnumerable<TravelMode> capabilities, DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(capabilities);
        EnsureCanUpdate(updatedAt);
        TransportCapabilities = capabilities.ToFrozenSet();
        UpdatedAt = updatedAt;
    }

    public void ReplaceFeedbackChannels(IEnumerable<FeedbackChannel> channels, DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(channels);
        EnsureCanUpdate(updatedAt);
        FeedbackChannels = channels.ToFrozenSet();
        UpdatedAt = updatedAt;
    }

    public void ReplacePresentationPreferences(PresentationPreferences preferences, DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        EnsureCanUpdate(updatedAt);
        PresentationPreferences = preferences;
        UpdatedAt = updatedAt;
    }

    private void Touch(DateTimeOffset updatedAt)
    {
        EnsureCanUpdate(updatedAt);
        UpdatedAt = updatedAt;
    }

    private void EnsureCanUpdate(DateTimeOffset updatedAt)
    {
        if (updatedAt < UpdatedAt)
        {
            throw new ArgumentOutOfRangeException(nameof(updatedAt), "The update timestamp cannot move backwards.");
        }
    }
}
