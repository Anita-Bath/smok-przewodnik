using AB.SmokPrzewodnik.Domain.Enums;

namespace AB.SmokPrzewodnik.Application.Spatials.Dtos;

public sealed record ConfidenceSummaryDto(
    ConfidenceState State,
    decimal Score,
    int EvidenceCount,
    DateTimeOffset EvaluatedAt);
