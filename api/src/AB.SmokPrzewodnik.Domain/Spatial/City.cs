using AB.SmokPrzewodnik.Domain.Common;
using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Domain.Spatial;

public sealed class City : AggregateRoot<Guid>
{
    public Code Code { get; }
    public LocalizedContent Name { get; private set; }
    public string DefaultLocale { get; private set; }
    public string TimeZone { get; private set; }
    public bool IsActive { get; private set; }

    public City(
        Guid id,
        Code code,
        LocalizedContent name,
        string defaultLocale,
        string timeZone,
        bool isActive) : base(id)
    {
        Code = code;
        Name = name;
        DefaultLocale = defaultLocale;
        TimeZone = timeZone;
        IsActive = isActive;
    }
}
