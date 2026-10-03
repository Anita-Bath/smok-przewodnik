namespace AB.SmokPrzewodnik.Domain.Spatial;

public sealed record SourceLicenseMetadata(
    string? LicenseCode,
    string? AttributionText,
    Uri? AttributionUri);
