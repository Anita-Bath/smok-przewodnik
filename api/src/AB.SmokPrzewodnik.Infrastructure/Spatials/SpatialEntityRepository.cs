using AB.SmokPrzewodnik.Application.Common.Querying;
using AB.SmokPrzewodnik.Application.Spatials;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.Spatial;
using AB.SmokPrzewodnik.Domain.Spatial.Details;
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

        var rows = await query
            .OrderBy(entity => entity.UpdatedAt ?? entity.CreatedAt)
            .ThenBy(entity => entity.Id)
            .Take(page.Limit + 1)
            .ToListAsync(cancellationToken);

        var hasNextPage = rows.Count > page.Limit;
        var items = rows.Take(page.Limit).ToArray();
        var nextCursor = hasNextPage
            ? CreateCursor(items[^1], effectiveCriteria, cursorScope, filterHash)
            : null;

        return new CursorPage<SpatialEntity>(items, nextCursor);
    }

    internal IQueryable<SpatialEntity> BuildQuery(SpatialEntityCriteria criteria)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        IQueryable<SpatialEntity> query;
        if (criteria.Kind == EntityKind.Event)
        {
            var from = criteria.From;
            var to = criteria.To;
            var categories = criteria.Categories.Order(StringComparer.Ordinal).ToArray();
            var minLongitude = criteria.BoundingBox?.MinLongitude ?? -180m;
            var minLatitude = criteria.BoundingBox?.MinLatitude ?? -90m;
            var maxLongitude = criteria.BoundingBox?.MaxLongitude ?? 180m;
            var maxLatitude = criteria.BoundingBox?.MaxLatitude ?? 90m;

            query = _dbContext.SpatialEntities.FromSqlInterpolated($$"""
                SELECT entity.*
                FROM spatial_entities AS entity
                INNER JOIN spatial_entity_details AS details
                    ON details.spatial_entity_id = entity.id
                    AND details.detail_type = 'event'
                WHERE (
                    CAST({{from}} AS timestamp with time zone) IS NULL
                    OR details.event_ends_at >= CAST({{from}} AS timestamp with time zone))
                  AND (
                    CAST({{to}} AS timestamp with time zone) IS NULL
                    OR details.event_starts_at <= CAST({{to}} AS timestamp with time zone))
                  AND (
                    cardinality(CAST({{categories}} AS text[])) = 0
                    OR EXISTS (
                        SELECT 1
                        FROM event_categories AS category
                        WHERE category.spatial_entity_id = entity.id
                          AND category.category_code = ANY(CAST({{categories}} AS text[]))))
                  AND ST_Intersects(
                    entity.geometry,
                    ST_MakeEnvelope(
                        CAST({{minLongitude}} AS double precision),
                        CAST({{minLatitude}} AS double precision),
                        CAST({{maxLongitude}} AS double precision),
                        CAST({{maxLatitude}} AS double precision),
                        4326))
                """);
        }
        else if (criteria.BoundingBox is { } bounds)
        {
            query = _dbContext.SpatialEntities.FromSqlInterpolated($$"""
                SELECT entity.*
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
        }
        else
        {
            query = _dbContext.SpatialEntities;
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
