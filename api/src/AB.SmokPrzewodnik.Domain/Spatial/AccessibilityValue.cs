using AB.SmokPrzewodnik.Domain.Common;
using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Domain.Spatial;

public abstract record AccessibilityValue
{
    private AccessibilityValue()
    {
    }

    public sealed record Boolean(bool Value) : AccessibilityValue;

    public sealed record Number(decimal Value, string? UnitCode) : AccessibilityValue;

    public sealed record Code(AB.SmokPrzewodnik.Domain.ValueObjects.Code Value) : AccessibilityValue;

    public sealed record Text : AccessibilityValue
    {
        public Text(string value)
        {
            Value = Guard.NotBlank(value, nameof(value));
        }

        public string Value { get; }
    }
}
