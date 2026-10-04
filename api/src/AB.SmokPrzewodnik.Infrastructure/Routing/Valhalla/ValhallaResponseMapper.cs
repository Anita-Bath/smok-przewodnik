using AB.SmokPrzewodnik.Domain.Confidence;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.Routing;
using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Infrastructure.Routing.Valhalla;

internal static class ValhallaResponseMapper
{
    public static IReadOnlyList<RouteAlternative> Map(
        ValhallaOsrmResponse response,
        ValhallaPlannedRequest plannedRequest,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt)
    {
        if (!response.Code.Equals("Ok", StringComparison.OrdinalIgnoreCase) || response.Routes.Count == 0)
        {
            throw new InvalidDataException("Valhalla returned no usable routes.");
        }

        return response.Routes
            .Select(route => Map(route, plannedRequest, createdAt, expiresAt))
            .ToArray();
    }

    private static RouteAlternative Map(
        ValhallaOsrmRoute route,
        ValhallaPlannedRequest plannedRequest,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt)
    {
        var legs = route.Legs.Select((leg, position) => new RouteLeg(
            Guid.NewGuid(),
            position,
            ToGeometry(LegCoordinates(leg, route.Geometry)),
            TimeSpan.FromSeconds((double)leg.Duration),
            leg.Distance,
            leg.Steps.Select((step, maneuverPosition) => new Maneuver(
                maneuverPosition,
                new Code(InstructionKey(step.Maneuver)),
                ToCoordinate(step.Maneuver.Location),
                step.Distance,
                TimeSpan.FromSeconds((double)step.Duration)))))
            .ToArray();

        return new RouteAlternative(
            Guid.NewGuid(),
            new Code(plannedRequest.Profile),
            ToGeometry(route.Geometry.Coordinates),
            legs,
            TimeSpan.FromSeconds((double)route.Duration),
            route.Distance,
            [new CostComponent(new Code("time_seconds"), route.Duration),
             new CostComponent(new Code("distance_metres"), route.Distance)],
            new AccessibilitySummary(
                [],
                plannedRequest.HardConstraintsRequiringValidation.Select(value => new Code(value)),
                []),
            [],
            new ConfidenceAssessment(ConfidenceState.Unverified, 0m, 0, createdAt),
            new Dictionary<Code, string> { [new Code("valhalla")] = "3.x" },
            createdAt,
            expiresAt);
    }

    private static IEnumerable<IReadOnlyList<decimal>> LegCoordinates(
        ValhallaOsrmLeg leg,
        ValhallaGeoJsonLineString fallback)
    {
        var coordinates = leg.Steps
            .SelectMany((step, index) => index == 0
                ? step.Geometry.Coordinates
                : step.Geometry.Coordinates.Skip(1))
            .ToArray();

        return coordinates.Length >= 2 ? coordinates : fallback.Coordinates;
    }

    private static SpatialGeometry ToGeometry(IEnumerable<IReadOnlyList<decimal>> coordinates) =>
        new(GeometryKind.Line, coordinates.Select(ToCoordinate));

    private static GeoCoordinate ToCoordinate(IReadOnlyList<decimal> coordinate)
    {
        if (coordinate.Count < 2)
        {
            throw new InvalidDataException("A Valhalla coordinate must contain longitude and latitude.");
        }

        return new GeoCoordinate(coordinate[1], coordinate[0]);
    }

    private static string InstructionKey(ValhallaOsrmManeuver maneuver) =>
        string.IsNullOrWhiteSpace(maneuver.Modifier)
            ? maneuver.Type
            : $"{maneuver.Type}.{maneuver.Modifier}";
}
