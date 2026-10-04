using AB.SmokPrzewodnik.Application.Navigation;
using AB.SmokPrzewodnik.Domain.Navigation;
using AB.SmokPrzewodnik.Domain.Routing;
using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Infrastructure.Navigation;

internal sealed class NavigationProgressMatcher : INavigationProgressMatcher
{
    public NavigationProgressMatch Match(RouteAlternative route, NavigationProgress progress)
    {
        var points = route.Geometry.Coordinates;
        var bestDistance = double.MaxValue;
        var bestPosition = points[0];
        var bestSegment = 0;

        for (var index = 0; index < points.Count - 1; index++)
        {
            var candidate = Project(progress.Position, points[index], points[index + 1]);
            var distance = Distance(progress.Position, candidate);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestPosition = candidate;
                bestSegment = index;
            }
        }

        var remaining = Distance(bestPosition, points[bestSegment + 1]);
        for (var index = bestSegment + 1; index < points.Count - 1; index++)
        {
            remaining += Distance(points[index], points[index + 1]);
        }

        var tolerance = Math.Max(30d, (double)(progress.AccuracyMetres ?? 0m) + 20d);
        var maneuver = route.Legs.SelectMany(leg => leg.Maneuvers).FirstOrDefault();
        return new NavigationProgressMatch(
            bestPosition,
            (decimal)remaining,
            bestDistance > tolerance,
            maneuver);
    }

    private static GeoCoordinate Project(GeoCoordinate point, GeoCoordinate start, GeoCoordinate end)
    {
        var x = (double)point.Longitude;
        var y = (double)point.Latitude;
        var x1 = (double)start.Longitude;
        var y1 = (double)start.Latitude;
        var dx = (double)(end.Longitude - start.Longitude);
        var dy = (double)(end.Latitude - start.Latitude);
        var lengthSquared = dx * dx + dy * dy;
        var t = lengthSquared == 0 ? 0 : Math.Clamp(((x - x1) * dx + (y - y1) * dy) / lengthSquared, 0, 1);
        return new GeoCoordinate((decimal)(y1 + t * dy), (decimal)(x1 + t * dx));
    }

    private static double Distance(GeoCoordinate left, GeoCoordinate right)
    {
        const double earthRadius = 6_371_000d;
        var latitude1 = DegreesToRadians((double)left.Latitude);
        var latitude2 = DegreesToRadians((double)right.Latitude);
        var latitudeDelta = latitude2 - latitude1;
        var longitudeDelta = DegreesToRadians((double)(right.Longitude - left.Longitude));
        var a = Math.Sin(latitudeDelta / 2) * Math.Sin(latitudeDelta / 2) +
                Math.Cos(latitude1) * Math.Cos(latitude2) *
                Math.Sin(longitudeDelta / 2) * Math.Sin(longitudeDelta / 2);
        return earthRadius * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double DegreesToRadians(double value) => value * Math.PI / 180d;
}
