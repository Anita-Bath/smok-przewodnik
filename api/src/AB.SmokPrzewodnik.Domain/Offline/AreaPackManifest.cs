using System.Text.RegularExpressions;

namespace AB.SmokPrzewodnik.Domain.Offline;

public sealed partial record AreaPackManifest
{
    public AreaPackManifest(
        Guid cityId,
        Guid areaId,
        string version,
        DateTimeOffset createdAt,
        string minimumAppVersion,
        IEnumerable<AreaPackResource> resources,
        string? baseVersionForDelta)
    {
        CityId = RequireId(cityId, nameof(cityId));
        AreaId = RequireId(areaId, nameof(areaId));
        Version = RequireSemanticVersion(version, nameof(version));
        MinimumAppVersion = RequireSemanticVersion(minimumAppVersion, nameof(minimumAppVersion));
        BaseVersionForDelta = baseVersionForDelta is null
            ? null
            : RequireSemanticVersion(baseVersionForDelta, nameof(baseVersionForDelta));

        if (string.Equals(Version, BaseVersionForDelta, StringComparison.Ordinal))
        {
            throw new ArgumentException("A delta pack cannot use its own version as its base.", nameof(baseVersionForDelta));
        }

        ArgumentNullException.ThrowIfNull(resources);
        var resourceList = resources.ToList();
        if (resourceList.Count == 0)
        {
            throw new ArgumentException("An area pack requires at least one resource.", nameof(resources));
        }

        if (resourceList.Any(resource => resource.ExpiresAt < createdAt))
        {
            throw new ArgumentException("A resource cannot expire before its manifest is created.", nameof(resources));
        }

        CreatedAt = createdAt;
        Resources = resourceList.AsReadOnly();
    }

    public Guid CityId { get; }
    public Guid AreaId { get; }
    public string Version { get; }
    public DateTimeOffset CreatedAt { get; }
    public string MinimumAppVersion { get; }
    public IReadOnlyList<AreaPackResource> Resources { get; }
    public string? BaseVersionForDelta { get; }

    private static Guid RequireId(Guid id, string parameterName)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("The identifier cannot be empty.", parameterName);
        }

        return id;
    }

    private static string RequireSemanticVersion(string? value, string parameterName)
    {
        if (value is null || !SemanticVersionPattern().IsMatch(value))
        {
            throw new ArgumentException("The value must be a semantic version.", parameterName);
        }

        return value;
    }

    [GeneratedRegex("^(0|[1-9][0-9]*)\\.(0|[1-9][0-9]*)\\.(0|[1-9][0-9]*)(?:-[0-9A-Za-z.-]+)?(?:\\+[0-9A-Za-z.-]+)?$", RegexOptions.CultureInvariant)]
    private static partial Regex SemanticVersionPattern();
}
