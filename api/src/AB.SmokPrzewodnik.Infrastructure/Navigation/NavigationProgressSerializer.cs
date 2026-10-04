using System.Text.Json;
using AB.SmokPrzewodnik.Domain.Navigation;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AB.SmokPrzewodnik.Infrastructure.Navigation;

internal static class NavigationProgressSerializer
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static ValueConverter<NavigationProgress?, string?> Converter { get; } = new(
        value => value == null ? null : Serialize(value),
        value => value == null ? null : Deserialize(value));

    public static ValueComparer<NavigationProgress?> Comparer { get; } = new(
        (left, right) => ReferenceEquals(left, right) || left != null && right != null && Serialize(left) == Serialize(right),
        value => value == null ? 0 : Serialize(value).GetHashCode(StringComparison.Ordinal),
        value => value == null ? null : Deserialize(Serialize(value)));

    private static string Serialize(NavigationProgress progress) => JsonSerializer.Serialize(new Document(
        progress.Position.Latitude,
        progress.Position.Longitude,
        progress.AccuracyMetres,
        progress.HeadingDegrees,
        progress.RecordedAt,
        progress.RouteLegId,
        progress.ManeuverIndex,
        progress.RemainingDistanceMetres,
        progress.IsOffRoute), Options);

    private static NavigationProgress Deserialize(string json)
    {
        var value = JsonSerializer.Deserialize<Document>(json, Options)
            ?? throw new InvalidOperationException("Could not deserialize navigation progress.");
        return new NavigationProgress(
            new GeoCoordinate(value.Latitude, value.Longitude),
            value.AccuracyMetres,
            value.HeadingDegrees,
            value.RecordedAt,
            value.RouteLegId,
            value.ManeuverIndex,
            value.RemainingDistanceMetres,
            value.IsOffRoute);
    }

    private sealed record Document(
        decimal Latitude,
        decimal Longitude,
        decimal? AccuracyMetres,
        decimal? HeadingDegrees,
        DateTimeOffset RecordedAt,
        Guid? RouteLegId,
        int? ManeuverIndex,
        decimal? RemainingDistanceMetres,
        bool IsOffRoute);
}
