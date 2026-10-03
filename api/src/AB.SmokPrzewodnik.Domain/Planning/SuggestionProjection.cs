using System.Collections.Frozen;
using AB.SmokPrzewodnik.Domain.Common;
using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Domain.Planning;

public sealed record SuggestionProjection
{
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

    public Guid AccountId { get; }

    public Guid SuggestedEntityId { get; }

    public IReadOnlySet<Code> ReasonCodes { get; }

    public decimal Score { get; }

    public DateTimeOffset GeneratedAt { get; }

    public DateTimeOffset ExpiresAt { get; }

    private static Guid RequireId(Guid id, string parameterName)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("The identifier cannot be empty.", parameterName);
        }

        return id;
    }
}
