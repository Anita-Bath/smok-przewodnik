using AB.SmokPrzewodnik.Application.Common.Querying;

namespace AB.SmokPrzewodnik.Infrastructure.Navigation;

internal sealed record NavigationSessionCursor(int Version,
    string Scope,
    string FilterHash) : ICursorPayload;
