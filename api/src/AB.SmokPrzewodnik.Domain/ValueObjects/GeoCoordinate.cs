namespace AB.SmokPrzewodnik.Domain.ValueObjects;

public readonly record struct GeoCoordinate
{
    public GeoCoordinate(decimal latitude, decimal longitude)
    {
        if (latitude is < -90 or > 90)
        {
            throw new ArgumentOutOfRangeException(nameof(latitude));
        }

        if (longitude is < -180 or > 180)
        {
            throw new ArgumentOutOfRangeException(nameof(longitude));
        }

        Latitude = latitude;
        Longitude = longitude;
    }

    public decimal Latitude { get; }

    public decimal Longitude { get; }
}
