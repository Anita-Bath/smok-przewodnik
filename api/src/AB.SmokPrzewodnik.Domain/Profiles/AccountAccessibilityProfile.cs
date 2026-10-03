using AB.SmokPrzewodnik.Domain.Common;

namespace AB.SmokPrzewodnik.Domain.Profiles;

public sealed class AccountAccessibilityProfile : AggregateRoot<Guid>
{
    public AccountAccessibilityProfile(
        Guid id,
        Guid accountId,
        AccessibilityProfileSettings settings,
        bool journeyHistorySyncEnabled,
        long serverVersion)
        : base(RequireId(id, nameof(id)))
    {
        AccountId = RequireId(accountId, nameof(accountId));
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        ServerVersion = RequireNonnegativeVersion(serverVersion);
        JourneyHistorySyncEnabled = journeyHistorySyncEnabled;
    }

    public Guid AccountId { get; }

    public AccessibilityProfileSettings Settings { get; private set; }

    public bool JourneyHistorySyncEnabled { get; private set; }

    public long ServerVersion { get; private set; }

    public static AccountAccessibilityProfile Create(Guid accountId, AccessibilityProfileSettings settings) =>
        new(Guid.NewGuid(), accountId, settings, false, 0);

    public void ReplaceSettings(AccessibilityProfileSettings settings, long serverVersion)
    {
        ArgumentNullException.ThrowIfNull(settings);
        EnsureNewerVersion(serverVersion);
        Settings = settings;
        ServerVersion = serverVersion;
    }

    public void SetJourneyHistorySync(bool enabled, long serverVersion)
    {
        EnsureNewerVersion(serverVersion);
        JourneyHistorySyncEnabled = enabled;
        ServerVersion = serverVersion;
    }

    private void EnsureNewerVersion(long serverVersion)
    {
        if (serverVersion <= ServerVersion)
        {
            throw new ArgumentOutOfRangeException(nameof(serverVersion), "The server version must increase.");
        }
    }

    private static Guid RequireId(Guid id, string parameterName)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("The identifier cannot be empty.", parameterName);
        }

        return id;
    }

    private static long RequireNonnegativeVersion(long serverVersion)
    {
        if (serverVersion < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(serverVersion));
        }

        return serverVersion;
    }
}
