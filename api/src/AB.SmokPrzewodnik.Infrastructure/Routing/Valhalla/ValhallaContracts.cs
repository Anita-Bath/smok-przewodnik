using System.Text.Json;
using System.Text.Json.Serialization;
using AB.SmokPrzewodnik.Domain.Enums;

namespace AB.SmokPrzewodnik.Infrastructure.Routing.Valhalla;

internal sealed record ValhallaPlannedRequest(
    TravelMode TravelMode,
    string Profile,
    ValhallaRouteRequest Request,
    IReadOnlySet<string> HardConstraintsRequiringValidation);

internal sealed record ValhallaRouteRequest(
    [property: JsonPropertyName("locations")] IReadOnlyList<ValhallaLocation> Locations,
    [property: JsonPropertyName("costing")] string Costing,
    [property: JsonPropertyName("costing_options")] IReadOnlyDictionary<string, IReadOnlyDictionary<string, JsonElement>> CostingOptions,
    [property: JsonPropertyName("units")] string Units,
    [property: JsonPropertyName("language")] string Language,
    [property: JsonPropertyName("alternates")] int Alternates,
    [property: JsonPropertyName("format")] string Format = "osrm",
    [property: JsonPropertyName("shape_format")] string ShapeFormat = "geojson");

internal sealed record ValhallaLocation(
    [property: JsonPropertyName("lat")] decimal Latitude,
    [property: JsonPropertyName("lon")] decimal Longitude);

internal sealed record ValhallaOsrmResponse(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("routes")] IReadOnlyList<ValhallaOsrmRoute> Routes);

internal sealed record ValhallaOsrmRoute(
    [property: JsonPropertyName("geometry")] ValhallaGeoJsonLineString Geometry,
    [property: JsonPropertyName("distance")] decimal Distance,
    [property: JsonPropertyName("duration")] decimal Duration,
    [property: JsonPropertyName("legs")] IReadOnlyList<ValhallaOsrmLeg> Legs);

internal sealed record ValhallaOsrmLeg(
    [property: JsonPropertyName("distance")] decimal Distance,
    [property: JsonPropertyName("duration")] decimal Duration,
    [property: JsonPropertyName("steps")] IReadOnlyList<ValhallaOsrmStep> Steps);

internal sealed record ValhallaOsrmStep(
    [property: JsonPropertyName("distance")] decimal Distance,
    [property: JsonPropertyName("duration")] decimal Duration,
    [property: JsonPropertyName("geometry")] ValhallaGeoJsonLineString Geometry,
    [property: JsonPropertyName("maneuver")] ValhallaOsrmManeuver Maneuver);

internal sealed record ValhallaOsrmManeuver(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("modifier")] string? Modifier,
    [property: JsonPropertyName("location")] IReadOnlyList<decimal> Location);

internal sealed record ValhallaGeoJsonLineString(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("coordinates")] IReadOnlyList<IReadOnlyList<decimal>> Coordinates);

internal static class ValhallaJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}
