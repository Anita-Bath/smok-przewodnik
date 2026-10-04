namespace AB.SmokPrzewodnik.Application.Navigation.Dtos;

public sealed record NavigationEventsResponse(
    IReadOnlyList<NavigationEventDto> Items,
    long LatestSequence,
    bool HasMore);
