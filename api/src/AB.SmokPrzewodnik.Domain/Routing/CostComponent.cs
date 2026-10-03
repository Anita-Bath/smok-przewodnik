using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Domain.Routing;

public sealed record CostComponent
{
    public CostComponent(Code code, decimal value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }

        Code = code;
        Value = value;
    }

    public Code Code { get; }

    public decimal Value { get; }
}
