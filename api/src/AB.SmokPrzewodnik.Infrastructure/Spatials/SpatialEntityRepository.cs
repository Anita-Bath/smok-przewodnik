using AB.SmokPrzewodnik.Application.Common.Querying;
using AB.SmokPrzewodnik.Application.Spatials;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.Spatial;
using AB.SmokPrzewodnik.Domain.Spatial.Details;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace AB.SmokPrzewodnik.Infrastructure.Spatials;

internal sealed class SpatialEntityRepository : ISpatialEntityRepository
{
    private readonly Database.DbContext _dbContext;
    private readonly ICursorCodec _cursorCodec;

    public SpatialEntityRepository(
        Database.DbContext dbContext,
        ICursorCodec cursorCodec)
    {
        _dbContext = dbContext;
        _cursorCodec = cursorCodec;
    }

    public Task<SpatialEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        _dbContext.SpatialEntities
            .AsNoTracking()
            .Include(entity => entity.Details)
            .Include(entity => entity.Translations)
            .SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);

    public async Task<CursorPage<SpatialEntity>> FindAsync(
        SpatialEntityCriteria criteria,
        CursorPageRequest page,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        ArgumentNullException.ThrowIfNull(page);

        var cursorScope = criteria.Kind == EntityKind.Event
            ? "events"
            : "spatial-entities";
        var filterHash = CreateFilterHash(criteria);
        SpatialEntityCursor? cursor = null;

        if (page.Cursor is not null)
        {
            if (!_cursorCodec.TryDecode<SpatialEntityCursor>(
                    page.Cursor,
                    cursorScope,
                    filterHash,
                    out cursor) || cursor is null ||
                criteria.FromIsDynamic && cursor.DynamicFrom is null)
            {
                throw new InvalidCursorException();
            }
        }

        var effectiveCriteria = cursor?.DynamicFrom is { } dynamicFrom
            ? new SpatialEntityCriteria(
                criteria.Kind,
                criteria.BoundingBox,
                dynamicFrom,
                criteria.To,
                criteria.Categories,
                fromIsDynamic: true)
            : criteria;
        var query = BuildQuery(effectiveCriteria);

        if (cursor is not null)
        {
            query = query.Where(entity =>
                (entity.UpdatedAt ?? entity.CreatedAt) > cursor.LastModifiedAt ||
                (entity.UpdatedAt ?? entity.CreatedAt) == cursor.LastModifiedAt &&
                entity.Id.CompareTo(cursor.LastId) > 0);
        }

        query = query
            .OrderBy(entity => entity.UpdatedAt ?? entity.CreatedAt)
            .ThenBy(entity => entity.Id);

        if (page.Limit.HasValue)
        {
            query = query.Take(page.Limit.Value + 1);
        }

        var rows = await query
            .ToListAsync(cancellationToken);

        var hasNextPage = rows.Count > page.Limit;
        var items = (page.Limit.HasValue ? rows.Take(page.Limit.Value) : rows).ToArray();
        var nextCursor = hasNextPage
            ? CreateCursor(items[^1], effectiveCriteria, cursorScope, filterHash)
            : null;

        return new CursorPage<SpatialEntity>(items, nextCursor);
    }

    internal IQueryable<SpatialEntity> BuildQuery(SpatialEntityCriteria criteria)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        IQueryable<SpatialEntity> query = _dbContext.SpatialEntities;

        if (criteria.Kind == EntityKind.Event)
        {
            IQueryable<EventDetails> events = _dbContext.Set<EventDetails>();

            if (criteria.From is { } from)
            {
                events = events.Where(details => details.EndsAt >= from);
            }

            if (criteria.To is { } to)
            {
                events = events.Where(details => details.StartsAt <= to);
            }

            if (criteria.Categories.Count > 0)
            {
                var categories = criteria.Categories.Select(category => new Code(category)).ToArray();
                events = events.Where(details =>
                    details.Categories.Any(category => categories.Contains(category.Code)));
            }

            query = query.Where(entity =>
                events.Select(details => details.SpatialEntityId).Contains(entity.Id));
        }

        if (criteria.BoundingBox is { } bounds)
        {
            var spatialIds = FindIdsWithin(bounds);
            query = query.Where(entity => spatialIds.Contains(entity.Id));
        }

        if (criteria.Kind.HasValue && criteria.Kind != EntityKind.Event)
        {
            query = query.Where(entity => entity.Kind == criteria.Kind.Value);
        }

        return query
            .AsNoTracking()
            .Include(entity => entity.Details)
            .Include(entity => entity.Translations);
    }

    private IQueryable<Guid> FindIdsWithin(BoundingBox bounds) =>
        _dbContext.Database.SqlQuery<Guid>($$"""
            SELECT entity.id AS "Value"
            FROM spatial_entities AS entity
            WHERE ST_Intersects(
                entity.geometry,
                ST_MakeEnvelope(
                    CAST({{bounds.MinLongitude}} AS double precision),
                    CAST({{bounds.MinLatitude}} AS double precision),
                    CAST({{bounds.MaxLongitude}} AS double precision),
                    CAST({{bounds.MaxLatitude}} AS double precision),
                    4326))
            """);

    private static string CreateFilterHash(SpatialEntityCriteria criteria)
    {
        if (criteria.Kind is null &&
            criteria.BoundingBox is null &&
            criteria.From is null &&
            criteria.To is null &&
            criteria.Categories.Count == 0)
        {
            return "all";
        }

        var bounds = criteria.BoundingBox is null
            ? string.Empty
            : string.Join(',',
                Format(criteria.BoundingBox.MinLongitude),
                Format(criteria.BoundingBox.MinLatitude),
                Format(criteria.BoundingBox.MaxLongitude),
                Format(criteria.BoundingBox.MaxLatitude));
        var canonical = string.Join('|',
            criteria.Kind?.ToString() ?? string.Empty,
            bounds,
            criteria.FromIsDynamic
                ? "current"
                : criteria.From?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) ?? string.Empty,
            criteria.To?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) ?? string.Empty,
            string.Join(',', criteria.Categories.Order(StringComparer.Ordinal)));

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)))
            .ToLowerInvariant();
    }

    private static string Format(decimal value) =>
        value.ToString(CultureInfo.InvariantCulture);

    private string CreateCursor(
        SpatialEntity entity,
        SpatialEntityCriteria criteria,
        string scope,
        string filterHash) =>
        _cursorCodec.Encode(new SpatialEntityCursor(
            1,
            scope,
            filterHash,
            entity.UpdatedAt ?? entity.CreatedAt,
            entity.Id,
            criteria.FromIsDynamic ? criteria.From : null));
}
