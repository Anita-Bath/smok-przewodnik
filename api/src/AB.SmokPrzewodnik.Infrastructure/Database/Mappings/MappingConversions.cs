using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using AB.SmokPrzewodnik.Domain.Spatial;
using AB.SmokPrzewodnik.Domain.Profiles;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;

namespace AB.SmokPrzewodnik.Infrastructure.Database.Mappings;

internal static class MappingConversions
{
    private static readonly GeometryFactory GeometryFactory =
        new(new PrecisionModel(), 4326);

    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    public static ValueConverter<Code, string> CodeConverter { get; } =
        new(code => code.Value, value => new Code(value));

    public static ValueConverter<Uri?, string?> NullableUriConverter { get; } =
        new(uri => uri == null ? null : uri.ToString(), value => value == null ? null : new Uri(value));

    public static ValueConverter<SpatialGeometry, Geometry> SpatialGeometryConverter { get; } =
        new(value => ToGeometry(value), value => ToSpatialGeometry(value));

    public static ValueComparer<SpatialGeometry> SpatialGeometryComparer { get; } =
        new(
            (left, right) => left == right || left != null && left.Equals(right),
            value => value.GetHashCode(),
            value => new SpatialGeometry(value.Kind, value.Coordinates));

    public static ValueConverter<T, string> JsonConverter<T>() where T : notnull =>
        new(
            value => Serialize(value),
            value => Deserialize<T>(value));

    public static ValueComparer<T> JsonComparer<T>() where T : notnull =>
        new(
            (left, right) => Serialize(left) == Serialize(right),
            value => Serialize(value).GetHashCode(StringComparison.Ordinal),
            value => Deserialize<T>(Serialize(value)));

    public static ValueConverter<T?, string?> NullableJsonConverter<T>() where T : class =>
        new(
            value => value == null ? null : Serialize(value),
            value => value == null ? null : Deserialize<T>(value));

    public static ValueComparer<T?> NullableJsonComparer<T>() where T : class =>
        new(
            (left, right) => left == null && right == null ||
                left != null && right != null && Serialize(left) == Serialize(right),
            value => value == null ? 0 : Serialize(value).GetHashCode(StringComparison.Ordinal),
            value => value == null ? null : Deserialize<T>(Serialize(value)));

    private static string Serialize<T>(T value) => value switch
    {
        AccessibilityProfileSettings settings => JsonSerializer.Serialize(
            new AccessibilityProfileSettingsDocument(
                settings.PreferredLocale,
                new SortedDictionary<string, ConstraintLevel>(
                    settings.Constraints.ToDictionary(pair => pair.Key.Value, pair => pair.Value),
                    StringComparer.Ordinal),
                settings.TransportCapabilities.Order().ToArray(),
                settings.FeedbackChannels.Order().ToArray(),
                settings.PresentationPreferences,
                settings.UpdatedAt),
            JsonOptions),
        LocalizedContent content => JsonSerializer.Serialize(
            new SortedDictionary<string, string>(
                content.Values.ToDictionary(pair => pair.Key, pair => pair.Value),
                StringComparer.Ordinal),
            JsonOptions),
        IReadOnlySet<Code> codes => JsonSerializer.Serialize(
            codes.OrderBy(code => code.Value, StringComparer.Ordinal).ToArray(),
            JsonOptions),
        IReadOnlySet<TravelMode> modes => JsonSerializer.Serialize(modes.Order().ToArray(), JsonOptions),
        _ => JsonSerializer.Serialize(value, JsonOptions)
    };

    private static T Deserialize<T>(string value)
    {
        object? result;
        if (typeof(T) == typeof(AccessibilityProfileSettings))
        {
            var document = JsonSerializer.Deserialize<AccessibilityProfileSettingsDocument>(value, JsonOptions)
                ?? throw new InvalidOperationException("Could not deserialize accessibility profile settings.");
            result = new AccessibilityProfileSettings(
                document.PreferredLocale,
                document.PresentationPreferences,
                document.UpdatedAt,
                document.Constraints.ToDictionary(pair => new Code(pair.Key), pair => pair.Value),
                document.TransportCapabilities,
                document.FeedbackChannels);
        }
        else if (typeof(T) == typeof(LocalizedContent))
        {
            var values = JsonSerializer.Deserialize<Dictionary<string, string>>(value, JsonOptions)
                ?? throw new InvalidOperationException("Could not deserialize localized content.");
            result = new LocalizedContent(values);
        }
        else if (typeof(T) == typeof(IReadOnlySet<Code>))
        {
            result = JsonSerializer.Deserialize<HashSet<Code>>(value, JsonOptions);
        }
        else if (typeof(T) == typeof(IReadOnlySet<TravelMode>))
        {
            result = JsonSerializer.Deserialize<HashSet<TravelMode>>(value, JsonOptions);
        }
        else if (typeof(T) == typeof(IReadOnlyList<Guid>))
        {
            result = JsonSerializer.Deserialize<List<Guid>>(value, JsonOptions);
        }
        else
        {
            result = JsonSerializer.Deserialize<T>(value, JsonOptions);
        }

        return result is T typed
            ? typed
            : throw new InvalidOperationException($"Could not deserialize {typeof(T).Name}.");
    }

    private static Geometry ToGeometry(SpatialGeometry value)
    {
        var coordinates = value.Coordinates
            .Select(coordinate => new Coordinate(
                Convert.ToDouble(coordinate.Longitude),
                Convert.ToDouble(coordinate.Latitude)))
            .ToArray();

        return value.Kind switch
        {
            Domain.Enums.GeometryKind.Point => GeometryFactory.CreatePoint(coordinates[0]),
            Domain.Enums.GeometryKind.Line => GeometryFactory.CreateLineString(coordinates),
            Domain.Enums.GeometryKind.Polygon => GeometryFactory.CreatePolygon(coordinates),
            _ => throw new ArgumentOutOfRangeException(nameof(value))
        };
    }

    private static SpatialGeometry ToSpatialGeometry(Geometry value)
    {
        var kind = value switch
        {
            Point => Domain.Enums.GeometryKind.Point,
            LineString => Domain.Enums.GeometryKind.Line,
            Polygon => Domain.Enums.GeometryKind.Polygon,
            _ => throw new InvalidOperationException($"Unsupported geometry type '{value.GeometryType}'.")
        };
        var coordinates = value.Coordinates
            .Select(coordinate => new GeoCoordinate(
                Convert.ToDecimal(coordinate.Y),
                Convert.ToDecimal(coordinate.X)))
            .ToArray();

        return new SpatialGeometry(kind, coordinates);
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var resolver = new DefaultJsonTypeInfoResolver();
        resolver.Modifiers.Add(ConfigurePolymorphism);

        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            TypeInfoResolver = resolver
        };
        options.Converters.Add(new CodeJsonConverter());
        return options;
    }

    private static void ConfigurePolymorphism(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Type == typeof(AccessibilityFactTarget))
        {
            typeInfo.PolymorphismOptions = Polymorphism(
                (typeof(AccessibilityFactTarget.SpatialEntity), "spatialEntity"),
                (typeof(AccessibilityFactTarget.GraphElement), "graphElement"));
        }
        else if (typeInfo.Type == typeof(AccessibilityValue))
        {
            typeInfo.PolymorphismOptions = Polymorphism(
                (typeof(AccessibilityValue.Boolean), "boolean"),
                (typeof(AccessibilityValue.Number), "number"),
                (typeof(AccessibilityValue.Code), "code"),
                (typeof(AccessibilityValue.Text), "text"));
        }
        else if (typeInfo.Type == typeof(EvidenceReference))
        {
            typeInfo.PolymorphismOptions = Polymorphism(
                (typeof(EvidenceReference.SourceAssertion), "sourceAssertion"),
                (typeof(EvidenceReference.Observation), "observation"));
        }
    }

    private static JsonPolymorphismOptions Polymorphism(
        params (Type Type, string Discriminator)[] derivedTypes)
    {
        var options = new JsonPolymorphismOptions
        {
            TypeDiscriminatorPropertyName = "$type"
        };
        foreach (var (type, discriminator) in derivedTypes)
        {
            options.DerivedTypes.Add(new JsonDerivedType(type, discriminator));
        }

        return options;
    }

    private sealed class CodeJsonConverter : JsonConverter<Code>
    {
        public override Code Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            new(reader.GetString() ?? throw new JsonException("Code cannot be null."));

        public override void Write(Utf8JsonWriter writer, Code value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value.Value);

        public override Code ReadAsPropertyName(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) =>
            new(reader.GetString() ?? throw new JsonException("Code cannot be null."));

        public override void WriteAsPropertyName(
            Utf8JsonWriter writer,
            Code value,
            JsonSerializerOptions options) =>
            writer.WritePropertyName(value.Value);
    }

    private sealed record AccessibilityProfileSettingsDocument(
        string PreferredLocale,
        IReadOnlyDictionary<string, ConstraintLevel> Constraints,
        IReadOnlyList<TravelMode> TransportCapabilities,
        IReadOnlyList<FeedbackChannel> FeedbackChannels,
        PresentationPreferences PresentationPreferences,
        DateTimeOffset UpdatedAt);
}
