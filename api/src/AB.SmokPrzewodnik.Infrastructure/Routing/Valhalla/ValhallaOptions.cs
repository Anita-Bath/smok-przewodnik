namespace AB.SmokPrzewodnik.Infrastructure.Routing.Valhalla;

internal sealed class ValhallaOptions
{
    public const string SectionName = "Valhalla";

    public Uri BaseUrl { get; init; } = new("http://localhost:8002");

    public int TimeoutSeconds { get; init; } = 10;

    public int Alternates { get; init; } = 2;
}
