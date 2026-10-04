using AB.SmokPrzewodnik.Domain.Common;

namespace AB.SmokPrzewodnik.Domain.Routing;

public sealed class RoutePlan : AggregateRoot<Guid>
{
    private RoutePlan()
    {
        Request = null!;
        Alternatives = [];
    }

    public RoutePlan(
        Guid id,
        Guid? accountId,
        RoutePlanRequest request,
        IEnumerable<RouteAlternative> alternatives,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt)
        : base(ValidateId(id), createdAt)
    {
        if (expiresAt <= createdAt)
        {
            throw new ArgumentException("A route plan must expire after it is created.", nameof(expiresAt));
        }

        Request = request ?? throw new ArgumentNullException(nameof(request));
        ArgumentNullException.ThrowIfNull(alternatives);

        var alternativeSnapshot = alternatives.ToList();
        if (alternativeSnapshot.Count == 0)
        {
            throw new ArgumentException("A route plan requires at least one alternative.", nameof(alternatives));
        }

        if (alternativeSnapshot.Select(alternative => alternative.Id).Distinct().Count() != alternativeSnapshot.Count)
        {
            throw new ArgumentException("Route alternatives must have unique identifiers.", nameof(alternatives));
        }

        if (alternativeSnapshot.Any(alternative => alternative.ExpiresAt < expiresAt))
        {
            throw new ArgumentException("Route alternatives cannot expire before their route plan.", nameof(alternatives));
        }

        AccountId = accountId;
        Alternatives = alternativeSnapshot.AsReadOnly();
        ExpiresAt = expiresAt;
    }

    public Guid? AccountId { get; private set; }

    public RoutePlanRequest Request { get; private set; }

    public IReadOnlyList<RouteAlternative> Alternatives { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public RouteAlternative? FindAlternative(Guid alternativeId) =>
        Alternatives.SingleOrDefault(alternative => alternative.Id == alternativeId);

    public bool IsExpired(DateTimeOffset now) => now >= ExpiresAt;

    private static Guid ValidateId(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("The route-plan identifier cannot be empty.", nameof(id));
        }

        return id;
    }
}
