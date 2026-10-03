using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Domain.Routing;

public abstract record RouteEndpoint
{
    private RouteEndpoint()
    {
    }

    public sealed record Coordinate : RouteEndpoint
    {
        public Coordinate(GeoCoordinate value)
        {
            Value = value;
        }

        public GeoCoordinate Value { get; }
    }

    public sealed record Entity : RouteEndpoint
    {
        public Entity(Guid entityId)
        {
            if (entityId == Guid.Empty)
            {
                throw new ArgumentException("The entity identifier cannot be empty.", nameof(entityId));
            }

            EntityId = entityId;
        }

        public Guid EntityId { get; }
    }
}
