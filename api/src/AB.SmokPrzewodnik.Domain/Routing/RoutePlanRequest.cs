using System.Collections.Frozen;
using System.Collections.ObjectModel;
using AB.SmokPrzewodnik.Domain.Common;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Domain.Routing;

public sealed record RoutePlanRequest
{
    public RoutePlanRequest(
        RouteEndpoint origin,
        RouteEndpoint destination,
        IEnumerable<TravelMode> travelModes,
        IReadOnlyDictionary<Code, ConstraintLevel> constraints,
        IEnumerable<Code> requestedProfiles,
        string locale,
        IReadOnlyDictionary<Code, string> clientDataVersions)
    {
        Origin = origin ?? throw new ArgumentNullException(nameof(origin));
        Destination = destination ?? throw new ArgumentNullException(nameof(destination));

        ArgumentNullException.ThrowIfNull(travelModes);
        TravelModes = travelModes.ToFrozenSet();
        if (TravelModes.Count == 0)
        {
            throw new ArgumentException("At least one travel mode is required.", nameof(travelModes));
        }

        ArgumentNullException.ThrowIfNull(constraints);
        Constraints = new ReadOnlyDictionary<Code, ConstraintLevel>(constraints.ToDictionary());

        ArgumentNullException.ThrowIfNull(requestedProfiles);
        var profiles = requestedProfiles.ToList();
        if (profiles.Count == 0)
        {
            throw new ArgumentException("At least one route profile is required.", nameof(requestedProfiles));
        }

        if (profiles.Distinct().Count() != profiles.Count)
        {
            throw new ArgumentException("Requested route profiles must be unique.", nameof(requestedProfiles));
        }

        RequestedProfiles = profiles.AsReadOnly();
        Locale = Guard.NotBlank(locale, nameof(locale));

        ArgumentNullException.ThrowIfNull(clientDataVersions);
        ClientDataVersions = new ReadOnlyDictionary<Code, string>(clientDataVersions.ToDictionary(
            pair => pair.Key,
            pair => Guard.NotBlank(pair.Value, nameof(clientDataVersions))));
    }

    public RouteEndpoint Origin { get; }

    public RouteEndpoint Destination { get; }

    public IReadOnlySet<TravelMode> TravelModes { get; }

    public IReadOnlyDictionary<Code, ConstraintLevel> Constraints { get; }

    public IReadOnlyList<Code> RequestedProfiles { get; }

    public string Locale { get; }

    public IReadOnlyDictionary<Code, string> ClientDataVersions { get; }
}
