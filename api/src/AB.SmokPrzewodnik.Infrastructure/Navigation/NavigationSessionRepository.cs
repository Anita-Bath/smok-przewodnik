using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AB.SmokPrzewodnik.Application.Common.Querying;
using AB.SmokPrzewodnik.Application.Navigation;
using AB.SmokPrzewodnik.Common;
using AB.SmokPrzewodnik.Domain.Navigation;
using Microsoft.EntityFrameworkCore;

namespace AB.SmokPrzewodnik.Infrastructure.Navigation;

internal sealed class NavigationSessionRepository : INavigationSessionRepository
{
    private readonly Database.DbContext _dbContext;
    private readonly ICursorCodec _cursorCodec;

    public NavigationSessionRepository(
        Database.DbContext dbContext,
        ICursorCodec cursorCodec)
    {
        _dbContext = dbContext;
        _cursorCodec = cursorCodec;
    }

    public Task<NavigationSession?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        _dbContext.NavigationSessions
            .AsNoTracking()
            .SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);

    public async Task<CursorPage<NavigationSession>> FindAsync(
        NavigationSessionCriteria criteria,
        CursorPageRequest page,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        ArgumentNullException.ThrowIfNull(page);

        var cursorScope = "navigation-sessions";
        var filterHash = CreateFilterHash(criteria);
        NavigationSessionCursor? cursor = null;

        if (page.Cursor is not null)
        {
            if (!_cursorCodec.TryDecode<NavigationSessionCursor>(
                    page.Cursor,
                    cursorScope,
                    filterHash,
                    out cursor) || cursor is null)
            {
                throw new InvalidCursorException();
            }
        }

        var query = BuildQuery(criteria);

        if (page.Limit.HasValue)
        {
            query = query.Take(page.Limit.Value + 1);
        }

        var rows = await query
            .ToListAsync(cancellationToken);

        var hasNextPage = rows.Count > page.Limit;
        var items = (page.Limit.HasValue ? rows.Take(page.Limit.Value) : rows).ToArray();
        var nextCursor = hasNextPage
            ? CreateCursor(items[^1], criteria, cursorScope, filterHash)
            : null;

        return new CursorPage<NavigationSession>(items, nextCursor);
    }

    public async Task<NavigationSession> InsertAsync(string hash, Guid? accountId)
    {
        var session = new NavigationSession(Guid.NewGuid(),
            hash, DateTimeOffset.UtcNow.Add(AppConsts.NavigationTokenValidityDuration), accountId);

        _dbContext.NavigationSessions.Add(session);
        await _dbContext.SaveChangesAsync();

        return session;
    }

    internal IQueryable<NavigationSession> BuildQuery(NavigationSessionCriteria criteria)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        IQueryable<NavigationSession> query = _dbContext.NavigationSessions;

        if (!string.IsNullOrWhiteSpace(criteria.Hash))
        {
            query = query.Where(x => x.Hash == criteria.Hash);
        }

        if (criteria.AccountId.HasValue)
        {
            query = query.Where(x => x.AccountId.HasValue && x.AccountId.Value == criteria.AccountId.Value);
        }

        if (criteria.SessionId.HasValue)
        {
            query = query.Where(x => x.Id == criteria.SessionId.Value);
        }

        return query
            .Where(x => x.ExpiresAt > DateTimeOffset.UtcNow)
            .AsNoTracking();
    }

    private static string CreateFilterHash(NavigationSessionCriteria criteria)
    {
        if (criteria.Hash is null &&
                             criteria.AccountId is null &&
                             criteria.SessionId is null)
        {
            return "all";
        }

        var canonical = string.Join('|',
            criteria.Hash ?? string.Empty,
            criteria.AccountId?.ToString() ?? string.Empty,
            criteria.SessionId?.ToString() ?? string.Empty);

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)))
            .ToLowerInvariant();
    }

    private static string Format(decimal value) =>
        value.ToString(CultureInfo.InvariantCulture);

    private string CreateCursor(
        NavigationSession entity,
        NavigationSessionCriteria criteria,
        string scope,
        string filterHash) =>
        _cursorCodec.Encode(new NavigationSessionCursor(
            1,
            scope,
            filterHash));
}
