using System.Collections.ObjectModel;
using AB.SmokPrzewodnik.Domain.Common;
using AB.SmokPrzewodnik.Domain.Planning;
using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Domain.Spatial;

public sealed class City : AggregateRoot<Guid>
{
    private readonly List<SpatialEntity> _spatialEntities = [];
    private readonly List<FeedItem> _feedItems = [];
    private readonly ReadOnlyCollection<SpatialEntity> _spatialEntityView;
    private readonly ReadOnlyCollection<FeedItem> _feedItemView;

    public Code Code { get; }
    public LocalizedContent Name { get; private set; }
    public string DefaultLocale { get; private set; }
    public string TimeZone { get; private set; }
    public bool IsActive { get; private set; }
    public IReadOnlyCollection<SpatialEntity> SpatialEntities => _spatialEntityView;
    public IReadOnlyCollection<FeedItem> FeedItems => _feedItemView;

    public City(
        Guid id,
        Code code,
        LocalizedContent name,
        string defaultLocale,
        string timeZone,
        bool isActive) : base(id)
    {
        Code = code;
        Name = name;
        DefaultLocale = defaultLocale;
        TimeZone = timeZone;
        IsActive = isActive;
        _spatialEntityView = _spatialEntities.AsReadOnly();
        _feedItemView = _feedItems.AsReadOnly();
    }
}
