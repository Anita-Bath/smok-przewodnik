namespace AB.SmokPrzewodnik.Domain.Entities;

public sealed record SourceLicenseMetadata(string? LicenseCode,
    string? AttributionText,
    Uri? AttributionUri);
