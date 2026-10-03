namespace AB.SmokPrzewodnik.Application.Spatials.Dtos;

public sealed record TranslationDto(
    string Locale,
    string Name,
    string Description);
