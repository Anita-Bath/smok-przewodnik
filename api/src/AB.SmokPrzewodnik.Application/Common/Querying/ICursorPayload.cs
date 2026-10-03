namespace AB.SmokPrzewodnik.Application.Common.Querying;

public interface ICursorPayload
{
    int Version { get; }
    string Scope { get; }
    string FilterHash { get; }
}
