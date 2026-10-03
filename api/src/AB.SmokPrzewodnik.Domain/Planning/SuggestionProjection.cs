using System.Collections.Frozen;
using AB.SmokPrzewodnik.Domain.Common;
using AB.SmokPrzewodnik.Domain.Spatial;
using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Domain.Planning;

public sealed record SuggestionProjection
{
    private SuggestionProjection()
    {
        ReasonCodes = FrozenSet<Code>.Empty;
    }

    public SuggestionProjection(
        Guid accountId,
        Guid suggestedEntityId,
        IEnumerable<Code> reasonCodes,
        decimal score,
        DateTimeOffset generatedAt,
        DateTimeOffset expiresAt)
    {
        AccountId = RequireId(accountId, nameof(accountId));
        SuggestedEntityId = RequireId(suggestedEntityId, nameof(suggestedEntityId));
        ArgumentNullException.ThrowIfNull(reasonCodes);
        ReasonCodes = reasonCodes.ToFrozenSet();
        if (ReasonCodes.Count == 0)
        {
            throw new ArgumentException("At least one reason code is required.", nameof(reasonCodes));
        }

        Score = Guard.InRange(score, 0m, 1m, nameof(score));
        if (expiresAt < generatedAt)
        {
            throw new ArgumentException("Expiry cannot precede generation.", nameof(expiresAt));
        }

        GeneratedAt = generatedAt;
        ExpiresAt = expiresAt;
    }

    public Guid AccountId { get; private set; }

    public Guid SuggestedEntityId { get; private set; }

    public SpatialEntity SuggestedEntity { get; private set; } = null!;

    public IReadOnlySet<Code> ReasonCodes { get; private set; }

    public decimal Score { get; private set; }

    public DateTimeOffset GeneratedAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    private static Guid RequireId(Guid id, string parameterName)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("The identifier cannot be empty.", parameterName);
        }

        return id;
    }
}
