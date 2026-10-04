using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.Routing;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using AB.SmokPrzewodnik.Infrastructure.Routing.Valhalla;
using Xunit;

namespace AB.SmokPrzewodnik.Application.UnitTests.Routing;

public sealed class ValhallaRequestMapperTests
{
    [Fact]
    public void Map_WalkingRequest_UsesCoordinatesLocaleAndAlternates()
    {
        var request = CreateRequest([TravelMode.Walk], profiles: ["fastest"]);

        var mapped = Assert.Single(ValhallaRequestMapper.Map(request, 2));

        Assert.Equal("pedestrian", mapped.Request.Costing);
        Assert.Equal("pl-PL", mapped.Request.Language);
        Assert.Equal("kilometers", mapped.Request.Units);
        Assert.Equal(2, mapped.Request.Alternates);
        Assert.Equal(50.0617m, mapped.Request.Locations[0].Latitude);
        Assert.Equal(19.9373m, mapped.Request.Locations[0].Longitude);
        Assert.Equal("fastest", mapped.Profile);
    }

    [Fact]
    public void Map_WheelchairRequest_UsesWheelchairPedestrianType()
    {
        var request = CreateRequest([TravelMode.MobilityAid], profiles: ["wheelchair", "easiest"]);

        var mapped = Assert.Single(ValhallaRequestMapper.Map(request, 1));
        var options = mapped.Request.CostingOptions["pedestrian"];

        Assert.Equal("wheelchair", options["type"].GetString());
        Assert.Equal(43200, options["step_penalty"].GetDecimal());
        Assert.Contains("stairs", mapped.HardConstraintsRequiringValidation);
    }

    [Fact]
    public void Map_BlindRequest_UsesBlindPedestrianType()
    {
        var request = CreateRequest([TravelMode.Walk], profiles: ["blind", "easiest"]);

        var mapped = Assert.Single(ValhallaRequestMapper.Map(request, 1));

        Assert.Equal("blind", mapped.Request.CostingOptions["pedestrian"]["type"].GetString());
    }

    [Fact]
    public void Map_CombinedBlindAndWheelchair_UsesBlindTypeWithExplicitWheelchairOptions()
    {
        var request = CreateRequest(
            [TravelMode.MobilityAid],
            profiles: ["blind", "wheelchair", "easiest"]);

        var mapped = Assert.Single(ValhallaRequestMapper.Map(request, 1));
        var options = mapped.Request.CostingOptions["pedestrian"];

        Assert.Equal("blind", options["type"].GetString());
        Assert.Equal(43200, options["step_penalty"].GetDecimal());
        Assert.Equal(0.8m, options["walking_speed"].GetDecimal());
        Assert.Contains("stairs", mapped.HardConstraintsRequiringValidation);
    }

    [Fact]
    public void Map_BicycleConstraints_MapPreferencesAndFlagHardValidation()
    {
        var request = CreateRequest(
            [TravelMode.Bicycle],
            constraints: new Dictionary<string, ConstraintLevel>
            {
                ["hills"] = ConstraintLevel.PreferAvoid,
                ["poor_surface"] = ConstraintLevel.MustAvoid,
                ["crowding"] = ConstraintLevel.MustAvoid
            },
            profiles: ["easiest"]);

        var mapped = Assert.Single(ValhallaRequestMapper.Map(request, 1));
        var options = mapped.Request.CostingOptions["bicycle"];

        Assert.Equal(0.1m, options["use_hills"].GetDecimal());
        Assert.Equal(1m, options["avoid_bad_surfaces"].GetDecimal());
        Assert.Contains("poor_surface", mapped.HardConstraintsRequiringValidation);
        Assert.Contains("crowding", mapped.HardConstraintsRequiringValidation);
    }

    [Fact]
    public void Map_PublicTransport_UsesMultimodalCosting()
    {
        var request = CreateRequest([TravelMode.PublicTransport], profiles: ["fewest_transfers"]);

        var mapped = Assert.Single(ValhallaRequestMapper.Map(request, 1));

        Assert.Equal("multimodal", mapped.Request.Costing);
        Assert.Equal(0m, mapped.Request.CostingOptions["transit"]["use_transfers"].GetDecimal());
    }

    private static RoutePlanRequest CreateRequest(
        IReadOnlyCollection<TravelMode> modes,
        IReadOnlyDictionary<string, ConstraintLevel>? constraints = null,
        IReadOnlyCollection<string>? profiles = null) =>
        new(
            new RouteEndpoint.Coordinate(new GeoCoordinate(50.0617m, 19.9373m)),
            new RouteEndpoint.Coordinate(new GeoCoordinate(50.067m, 19.945m)),
            modes,
            (constraints ?? new Dictionary<string, ConstraintLevel>())
                .ToDictionary(pair => new Code(pair.Key), pair => pair.Value),
            (profiles ?? ["fastest"]).Select(profile => new Code(profile)),
            "pl-PL",
            new Dictionary<Code, string>());
}
