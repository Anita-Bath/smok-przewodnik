namespace AB.SmokPrzewodnik.Application.Navigation;

public sealed record NavigationSessionCriteria(string? Hash = null,
    Guid? AccountId = null,
    Guid? SessionId = null);
