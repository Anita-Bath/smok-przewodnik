using AB.SmokPrzewodnik.Application.Navigation.Dtos;
using AB.SmokPrzewodnik.Application.Navigation.Mappers;
using MediatR;

namespace AB.SmokPrzewodnik.Application.Navigation.Queries;

public sealed class GetNavigationEventsQueryHandler(
    INavigationSessionRepository sessions,
    TimeProvider timeProvider) : IRequestHandler<GetNavigationEventsQuery, NavigationEventsResponse>
{
    private const int BatchSize = 100;

    public async Task<NavigationEventsResponse> Handle(
        GetNavigationEventsQuery query,
        CancellationToken cancellationToken)
    {
        if (query.AfterSequence < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(query.AfterSequence));
        }

        var session = await sessions.GetByIdAsync(query.SessionId, cancellationToken)
            ?? throw new NavigationSessionNotFoundException(query.SessionId);
        if (session.IsExpired(timeProvider.GetUtcNow()))
        {
            throw new NavigationSessionExpiredException(query.SessionId);
        }

        var events = await sessions.GetEventsAfterAsync(
            query.SessionId, query.AfterSequence, BatchSize + 1, cancellationToken);
        var hasMore = events.Count > BatchSize;
        var items = events.Take(BatchSize).Select(NavigationEventMapper.ToDto).ToArray();
        return new NavigationEventsResponse(items, session.NextEventSequence - 1, hasMore);
    }
}
