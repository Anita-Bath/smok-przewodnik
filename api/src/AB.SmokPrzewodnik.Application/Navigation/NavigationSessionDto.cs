namespace AB.SmokPrzewodnik.Application.Navigation;

public sealed record NavigationSessionDto(Guid Id,
    string Hash,
    DateTimeOffset ExpiresAt,
    Guid? AccountId);
