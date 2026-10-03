namespace AB.SmokPrzewodnik.Domain.Entities;

public abstract record AccessibilityValue
{
    public sealed record Boolean(bool Value) : AccessibilityValue;
    public sealed record Number(decimal Value, string? UnitCode)
        : AccessibilityValue;
    public sealed record Code(TaxonomyCode Value) :
    AccessibilityValue;
    public sealed record Text(string Value) : AccessibilityValue;
}
