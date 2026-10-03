using System.Collections.ObjectModel;
using AB.SmokPrzewodnik.Domain.Common;

namespace AB.SmokPrzewodnik.Domain.ValueObjects;

public sealed class LocalizedContent
{
    private readonly ReadOnlyDictionary<string, string> _values;

    public LocalizedContent(IEnumerable<KeyValuePair<string, string>> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var normalized = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var pair in values)
        {
            var locale = Guard.NotBlank(pair.Key, nameof(values));
            var content = Guard.NotBlank(pair.Value, nameof(values));
            if (!normalized.TryAdd(locale, content))
            {
                throw new ArgumentException($"Duplicate locale '{locale}'.", nameof(values));
            }
        }

        if (normalized.Count == 0)
        {
            throw new ArgumentException("At least one localized value is required.", nameof(values));
        }

        _values = new ReadOnlyDictionary<string, string>(normalized);
    }

    public IReadOnlyDictionary<string, string> Values => _values;
}
