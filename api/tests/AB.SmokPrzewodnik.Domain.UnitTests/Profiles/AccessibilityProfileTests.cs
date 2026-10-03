using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.Profiles;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using Xunit;

namespace AB.SmokPrzewodnik.Domain.UnitTests.Profiles;

public sealed class AccessibilityProfileTests
{
    [Fact]
    public void Settings_StartWithEmptyPreferenceCollections()
    {
        var settings = Settings();

        Assert.Empty(settings.Constraints);
        Assert.Empty(settings.TransportCapabilities);
        Assert.Empty(settings.FeedbackChannels);
    }

    [Fact]
    public void Constraints_CanBeUpsertedAndRemoved()
    {
        var settings = Settings();
        var stairs = new Code("stairs");
        var updatedAt = DateTimeOffset.UtcNow.AddMinutes(1);

        settings.SetConstraint(stairs, ConstraintLevel.PreferAvoid, updatedAt);
        settings.SetConstraint(stairs, ConstraintLevel.MustAvoid, updatedAt.AddMinutes(1));

        Assert.Equal(ConstraintLevel.MustAvoid, settings.Constraints[stairs]);
        Assert.True(settings.RemoveConstraint(stairs, updatedAt.AddMinutes(2)));
        Assert.Empty(settings.Constraints);
    }

    [Fact]
    public void CapabilityAndFeedbackReplacement_EliminatesDuplicates()
    {
        var settings = Settings();
        var updatedAt = DateTimeOffset.UtcNow.AddMinutes(1);

        settings.ReplaceTransportCapabilities(
            [TravelMode.Walk, TravelMode.Walk, TravelMode.PublicTransport], updatedAt);
        settings.ReplaceFeedbackChannels(
            [FeedbackChannel.Visual, FeedbackChannel.Visual, FeedbackChannel.Haptic], updatedAt);

        Assert.Equal(2, settings.TransportCapabilities.Count);
        Assert.Equal(2, settings.FeedbackChannels.Count);
    }

    [Fact]
    public void Collections_AreExposedAsReadOnlyViews()
    {
        var settings = Settings();

        Assert.True(Assert.IsAssignableFrom<ICollection<KeyValuePair<Code, ConstraintLevel>>>(settings.Constraints).IsReadOnly);
        Assert.True(Assert.IsAssignableFrom<ISet<TravelMode>>(settings.TransportCapabilities).IsReadOnly);
        Assert.True(Assert.IsAssignableFrom<ISet<FeedbackChannel>>(settings.FeedbackChannels).IsReadOnly);
    }

    [Fact]
    public void Locale_CannotBeBlank()
    {
        Assert.Throws<ArgumentException>(() => new AccessibilityProfileSettings(
            " ", PresentationPreferences.Default, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void LocaleChange_IsAtomicWhenTimestampIsRejected()
    {
        var settings = Settings();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            settings.ChangePreferredLocale("en-GB", settings.UpdatedAt.AddMinutes(-1)));

        Assert.Equal("pl-PL", settings.PreferredLocale);
    }

    [Fact]
    public void PresentationPreferences_AreReplacedAsAWhole()
    {
        var settings = Settings();
        var preferences = new PresentationPreferences(true, true, true, true, 1.5m);
        var updatedAt = DateTimeOffset.UtcNow.AddMinutes(1);

        settings.ReplacePresentationPreferences(preferences, updatedAt);

        Assert.Equal(preferences, settings.PresentationPreferences);
        Assert.Equal(updatedAt, settings.UpdatedAt);
    }

    [Theory]
    [InlineData(0.49)]
    [InlineData(3.01)]
    public void PresentationPreferences_RejectInvalidTextScale(double textScale)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PresentationPreferences(false, false, false, false, (decimal)textScale));
    }

    [Fact]
    public void AccountProfile_PreservesIdentifiersAndTogglesJourneyHistory()
    {
        var profileId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var profile = new AccountAccessibilityProfile(profileId, accountId, Settings(), false, 4);

        profile.SetJourneyHistorySync(true, 5);

        Assert.Equal(profileId, profile.Id);
        Assert.Equal(accountId, profile.AccountId);
        Assert.True(profile.JourneyHistorySyncEnabled);
        Assert.Equal(5, profile.ServerVersion);
    }

    [Fact]
    public void AccountProfile_RequiresMonotonicallyIncreasingServerVersion()
    {
        var profile = new AccountAccessibilityProfile(Guid.NewGuid(), Guid.NewGuid(), Settings(), false, 4);

        Assert.Throws<ArgumentOutOfRangeException>(() => profile.SetJourneyHistorySync(true, 4));
        Assert.Throws<ArgumentOutOfRangeException>(() => profile.ReplaceSettings(Settings("en-GB"), 3));
    }

    private static AccessibilityProfileSettings Settings(string locale = "pl-PL") =>
        new(locale, PresentationPreferences.Default, DateTimeOffset.UtcNow);
}
