using AB.SmokPrzewodnik.Domain.Common;

namespace AB.SmokPrzewodnik.Domain.Navigation;

public sealed class NavigationSession : Entity<Guid>
{
    public string Hash { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public Guid? AccountId { get; set; }

    public NavigationSession(Guid id, string hash, DateTimeOffset expiresAt, Guid? accountId) : base(id)
    {
        Hash = hash;
        ExpiresAt = expiresAt;
        AccountId = accountId;
    }
}
