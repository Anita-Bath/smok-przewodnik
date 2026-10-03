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
        var query = ApplyCriteria(
            _dbContext.SpatialEntities
                .AsNoTracking()
                .Include(entity => entity.Translations),
            effectiveCriteria);

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

    private static IQueryable<SpatialEntity> ApplyCriteria(
        IQueryable<SpatialEntity> query,
        SpatialEntityCriteria criteria)
    {
        if (criteria.Kind.HasValue)
        {
            query = query.Where(entity => entity.Kind == criteria.Kind.Value);
        }

        if (criteria.BoundingBox is { } bounds)
        {
            query = query.Where(entity => entity.Geometry.Coordinates.Any(coordinate =>
                coordinate.Longitude >= bounds.MinLongitude &&
                coordinate.Longitude <= bounds.MaxLongitude &&
                coordinate.Latitude >= bounds.MinLatitude &&
                coordinate.Latitude <= bounds.MaxLatitude));
        }

        if (criteria.From.HasValue)
        {
            query = query.Where(entity =>
                ((EventDetails)entity.Details!).EndsAt >= criteria.From.Value);
        }

        if (criteria.To.HasValue)
        {
            query = query.Where(entity =>
                ((EventDetails)entity.Details!).StartsAt <= criteria.To.Value);
        }

        if (criteria.Categories.Count > 0)
        {
            query = query.Where(entity =>
                ((EventDetails)entity.Details!).CategoryCodes.Any(category =>
                    criteria.Categories.Contains(category.Value.ToLower())));
        }

        return query;
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
