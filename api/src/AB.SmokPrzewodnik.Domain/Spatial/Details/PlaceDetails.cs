using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Domain.Spatial.Details;

public sealed record ContactDetails(string? Phone, string? Email);

public sealed record PlaceDetails(
    Code CategoryCode,
    string? OpeningHours,
    ContactDetails? Contact,
    Uri? Website) : SpatialEntityDetails(EntityKind.Place);
