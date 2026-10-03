using AB.SmokPrzewodnik.Domain.Common;

namespace AB.SmokPrzewodnik.Domain.ValueObjects;

public readonly record struct Code
{
    public Code(string value)
    {
        Value = Guard.NotBlank(value, nameof(value));
    }

    public string Value { get; }

    public override string ToString() => Value;
}
