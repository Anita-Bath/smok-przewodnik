namespace AB.SmokPrzewodnik.Application.Routing;

public enum RoutePlanningFailureKind
{
    InvalidRequest = 1,
    InvalidResponse,
    Unavailable
}

public sealed class RoutePlanningException : Exception
{
    public RoutePlanningException(
        RoutePlanningFailureKind kind,
        string message,
        Exception? innerException = null)
        : base(message, innerException)
    {
        Kind = kind;
    }

    public RoutePlanningFailureKind Kind { get; }
}
