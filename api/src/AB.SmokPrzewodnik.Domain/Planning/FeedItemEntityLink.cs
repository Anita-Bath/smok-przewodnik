using AB.SmokPrzewodnik.Domain.Spatial;

namespace AB.SmokPrzewodnik.Domain.Planning;

public sealed class FeedItemEntityLink
{
    private FeedItemEntityLink()
    {
        FeedItem = null!;
        Entity = null!;
    }

    internal FeedItemEntityLink(Guid feedItemId, Guid entityId)
    {
        FeedItemId = feedItemId;
        EntityId = entityId;
        FeedItem = null!;
        Entity = null!;
    }

    public Guid FeedItemId { get; private set; }
    public FeedItem FeedItem { get; private set; }
    public Guid EntityId { get; private set; }
    public SpatialEntity Entity { get; private set; }
}
