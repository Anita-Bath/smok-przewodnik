namespace AB.SmokPrzewodnik.Domain.Entities;

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
        bool isActive);
}
