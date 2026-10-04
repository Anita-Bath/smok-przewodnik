using System.Text.Json;
using AB.SmokPrzewodnik.Domain.Routing;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AB.SmokPrzewodnik.Infrastructure.Navigation;

internal static class NavigationEventValueConverters
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static ValueConverter<Maneuver?, string?> ManeuverConverter { get; } = new(
        value => value == null ? null : SerializeManeuver(value),
        value => value == null ? null : DeserializeManeuver(value));

    public static ValueComparer<Maneuver?> ManeuverComparer { get; } = new(
        (left, right) => ReferenceEquals(left, right) || left != null && right != null && SerializeManeuver(left) == SerializeManeuver(right),
        value => value == null ? 0 : SerializeManeuver(value).GetHashCode(StringComparison.Ordinal),
        value => value == null ? null : DeserializeManeuver(SerializeManeuver(value)));

    public static ValueConverter<IReadOnlyDictionary<string, string>, string> ParametersConverter { get; } = new(
        value => SerializeParameters(value), value => DeserializeParameters(value));

    public static ValueComparer<IReadOnlyDictionary<string, string>> ParametersComparer { get; } = new(
        (left, right) => ReferenceEquals(left, right) || left != null && right != null && SerializeParameters(left) == SerializeParameters(right),
        value => value == null ? 0 : SerializeParameters(value).GetHashCode(StringComparison.Ordinal),
        value => value == null ? null! : DeserializeParameters(SerializeParameters(value)));

    public static ValueConverter<IReadOnlySet<Code>, string> PatternsConverter { get; } = new(
        value => SerializePatterns(value), value => DeserializePatterns(value));

    public static ValueComparer<IReadOnlySet<Code>> PatternsComparer { get; } = new(
        (left, right) => ReferenceEquals(left, right) || left != null && right != null && SerializePatterns(left) == SerializePatterns(right),
        value => value == null ? 0 : SerializePatterns(value).GetHashCode(StringComparison.Ordinal),
        value => value == null ? null! : DeserializePatterns(SerializePatterns(value)));

    private static string SerializeManeuver(Maneuver value) => JsonSerializer.Serialize(new ManeuverDocument(
        value.Position, value.InstructionKey.Value, value.Location.Latitude, value.Location.Longitude,
        value.DistanceMetres, value.Duration), Options);

    private static Maneuver DeserializeManeuver(string json)
    {
        var value = JsonSerializer.Deserialize<ManeuverDocument>(json, Options)!;
        return new Maneuver(value.Position, new Code(value.InstructionKey),
            new GeoCoordinate(value.Latitude, value.Longitude), value.DistanceMetres, value.Duration);
    }

    private static string SerializeParameters(IReadOnlyDictionary<string, string> value) =>
        JsonSerializer.Serialize(value, Options);
    private static IReadOnlyDictionary<string, string> DeserializeParameters(string value) =>
        JsonSerializer.Deserialize<Dictionary<string, string>>(value, Options)!;
    private static string SerializePatterns(IReadOnlySet<Code> value) =>
        JsonSerializer.Serialize(value.Select(item => item.Value).Order(StringComparer.Ordinal), Options);
    private static IReadOnlySet<Code> DeserializePatterns(string value) =>
        JsonSerializer.Deserialize<string[]>(value, Options)!.Select(item => new Code(item)).ToHashSet();

    private sealed record ManeuverDocument(
        int Position, string InstructionKey, decimal Latitude, decimal Longitude,
        decimal DistanceMetres, TimeSpan Duration);
}
