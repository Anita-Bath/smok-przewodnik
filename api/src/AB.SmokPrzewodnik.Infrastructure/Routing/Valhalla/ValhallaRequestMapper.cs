using System.Text.Json;
using AB.SmokPrzewodnik.Application.Routing;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.Routing;

namespace AB.SmokPrzewodnik.Infrastructure.Routing.Valhalla;

internal static class ValhallaRequestMapper
{
    private static readonly HashSet<string> CapabilityProfiles =
        new(["blind", "wheelchair"], StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyList<ValhallaPlannedRequest> Map(
        RoutePlanRequest request,
        int alternates)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (alternates < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(alternates));
        }

        var locations = new[]
        {
            ToLocation(request.Origin),
            ToLocation(request.Destination)
        };
        var profileValues = request.RequestedProfiles.Select(profile => profile.Value).ToArray();
        var routingProfiles = profileValues
            .Where(profile => !CapabilityProfiles.Contains(profile))
            .DefaultIfEmpty("fastest")
            .ToArray();
        var isBlind = profileValues.Contains("blind", StringComparer.OrdinalIgnoreCase);
        var isWheelchair = profileValues.Contains("wheelchair", StringComparer.OrdinalIgnoreCase) ||
            request.TravelModes.Contains(TravelMode.MobilityAid);

        return request.TravelModes
            .Order()
            .SelectMany(mode => routingProfiles.Select(profile => Map(
                mode,
                profile,
                locations,
                request,
                isBlind,
                isWheelchair,
                alternates)))
            .ToArray();
    }

    private static ValhallaPlannedRequest Map(
        TravelMode mode,
        string profile,
        IReadOnlyList<ValhallaLocation> locations,
        RoutePlanRequest request,
        bool isBlind,
        bool isWheelchair,
        int alternates)
    {
        var costing = CostingFor(mode);
        var options = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        var hardConstraints = request.Constraints
            .Where(pair => pair.Value == ConstraintLevel.MustAvoid)
            .Select(pair => pair.Key.Value)
            .ToHashSet(StringComparer.Ordinal);

        if (costing == "pedestrian")
        {
            if (isBlind)
            {
                options["type"] = Json("blind");
            }
            else if (isWheelchair)
            {
                options["type"] = Json("wheelchair");
            }

            if (isWheelchair)
            {
                options["step_penalty"] = Json(43200m);
                options["walking_speed"] = Json(0.8m);
                hardConstraints.Add("stairs");
            }
        }

        ApplyProfile(profile, costing, options);
        ApplyConstraints(request.Constraints, costing, options);

        IReadOnlyDictionary<string, IReadOnlyDictionary<string, JsonElement>> costingOptions =
            new Dictionary<string, IReadOnlyDictionary<string, JsonElement>>(StringComparer.Ordinal)
            {
                [costing == "multimodal" ? "transit" : costing] = options
            };

        return new ValhallaPlannedRequest(
            mode,
            profile,
            new ValhallaRouteRequest(
                locations,
                costing,
                costingOptions,
                "kilometers",
                request.Locale,
                alternates),
            hardConstraints);
    }

    private static void ApplyProfile(
        string profile,
        string costing,
        IDictionary<string, JsonElement> options)
    {
        if (profile.Equals("easiest", StringComparison.OrdinalIgnoreCase) &&
            costing is "pedestrian" or "bicycle")
        {
            options["use_hills"] = Json(0.25m);
        }
        else if (profile.Equals("fewest_transfers", StringComparison.OrdinalIgnoreCase) &&
                 costing == "multimodal")
        {
            options["use_transfers"] = Json(0m);
        }
        else if (profile.Equals("shortest", StringComparison.OrdinalIgnoreCase))
        {
            options["shortest"] = Json(true);
        }
    }

    private static void ApplyConstraints(
        IReadOnlyDictionary<Domain.ValueObjects.Code, ConstraintLevel> constraints,
        string costing,
        IDictionary<string, JsonElement> options)
    {
        foreach (var (code, level) in constraints)
        {
            if (level == ConstraintLevel.Allowed)
            {
                continue;
            }

            var strongest = level == ConstraintLevel.MustAvoid;
            switch (code.Value)
            {
                case "stairs" when costing == "pedestrian":
                    options["step_penalty"] = Json(strongest ? 43200m : 1800m);
                    break;
                case "elevators" when costing == "pedestrian":
                    options["elevator_penalty"] = Json(strongest ? 43200m : 1800m);
                    break;
                case "hills" when costing is "pedestrian" or "bicycle":
                    options["use_hills"] = Json(strongest ? 0m : 0.1m);
                    break;
                case "unlit" when costing == "pedestrian":
                    options["use_lit"] = Json(1m);
                    break;
                case "poor_surface" when costing == "bicycle":
                    options["avoid_bad_surfaces"] = Json(strongest ? 1m : 0.75m);
                    break;
            }
        }
    }

    private static string CostingFor(TravelMode mode) => mode switch
    {
        TravelMode.Walk or TravelMode.MobilityAid => "pedestrian",
        TravelMode.Bicycle => "bicycle",
        TravelMode.Micromobility => "motor_scooter",
        TravelMode.PublicTransport => "multimodal",
        TravelMode.Car => "auto",
        _ => throw new RoutePlanningException(
            RoutePlanningFailureKind.InvalidRequest,
            $"Travel mode '{mode}' is not supported by the routing provider.")
    };

    private static ValhallaLocation ToLocation(RouteEndpoint endpoint) => endpoint switch
    {
        RouteEndpoint.Coordinate coordinate =>
            new ValhallaLocation(coordinate.Value.Latitude, coordinate.Value.Longitude),
        _ => throw new RoutePlanningException(
            RoutePlanningFailureKind.InvalidRequest,
            "Route entity endpoints must be resolved to coordinates before calling the routing provider.")
    };

    private static JsonElement Json<T>(T value) => JsonSerializer.SerializeToElement(value);
}
