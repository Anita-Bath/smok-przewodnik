namespace AB.SmokPrzewodnik.Domain.Entities;

public sealed class DataSource : AggregateRoot<Guid>
{
    public Code ProviderCode { get; }
    public string DisplayName { get; private set; }
    public Code SourceType { get; }
    public Uri? Website { get; private set; }
    public SourceLicenseMetadata License { get; private set; }
    public decimal DefaultConfidenceWeight { get; private set; }
    public bool IsEnabled { get; private set; }

    public DataSource(
        Guid id,
        Code providerCode,
        string displayName,
        Code sourceType,
        Uri? website,
        SourceLicenseMetadata license,
        decimal defaultConfidenceWeight,
        bool isEnabled);
}
