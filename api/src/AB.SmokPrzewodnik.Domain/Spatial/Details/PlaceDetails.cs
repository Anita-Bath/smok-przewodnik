using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Domain.Spatial.Details;

public sealed record ContactDetails(string? Phone, string? Email);

public sealed record PlaceDetails : SpatialEntityDetails
{
    private PlaceDetails() : base(EntityKind.Place)
    {
    }

    public PlaceDetails(
        Code categoryCode,
        string? openingHours,
        ContactDetails? contact,
        Uri? website)
        : base(EntityKind.Place)
    {
        CategoryCode = categoryCode;
        OpeningHours = openingHours;
        Contact = contact;
        Website = website;
    }

    public Code CategoryCode { get; private set; }
    public string? OpeningHours { get; private set; }
    public ContactDetails? Contact { get; private set; }
    public Uri? Website { get; private set; }
}
