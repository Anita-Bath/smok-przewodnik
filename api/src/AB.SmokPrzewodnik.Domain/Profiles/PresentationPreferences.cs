using AB.SmokPrzewodnik.Domain.Common;

namespace AB.SmokPrzewodnik.Domain.Profiles;

public sealed record PresentationPreferences
{
    public static PresentationPreferences Default { get; } = new(false, false, false, false, 1m);

    public PresentationPreferences(
        bool highContrast,
        bool largeText,
        bool reducedMotion,
        bool simplifiedGuidance,
        decimal textScale)
    {
        HighContrast = highContrast;
        LargeText = largeText;
        ReducedMotion = reducedMotion;
        SimplifiedGuidance = simplifiedGuidance;
        TextScale = Guard.InRange(textScale, 0.5m, 3m, nameof(textScale));
    }

    public bool HighContrast { get; }

    public bool LargeText { get; }

    public bool ReducedMotion { get; }

    public bool SimplifiedGuidance { get; }

    public decimal TextScale { get; }
}
