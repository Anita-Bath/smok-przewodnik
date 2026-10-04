using AB.SmokPrzewodnik.Domain.Navigation;

namespace AB.SmokPrzewodnik.Application.Navigation.Mappers;

internal static class NavigationSessionMapper
{
    public static NavigationSessionDto ToDto(NavigationSession session) => new(session.Id, session.Hash, session.ExpiresAt, session.AccountId);
}
