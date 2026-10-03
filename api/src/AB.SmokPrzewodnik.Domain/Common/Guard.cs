namespace AB.SmokPrzewodnik.Domain.Common;

internal static class Guard
{
    public static string NotBlank(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("The value cannot be empty.", parameterName);
        }

        return value.Trim();
    }

    public static decimal InRange(decimal value, decimal minimum, decimal maximum, string parameterName)
    {
        if (value < minimum || value > maximum)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, $"The value must be between {minimum} and {maximum}.");
        }

        return value;
    }

    public static void ValidTimeRange(DateTimeOffset? validFrom, DateTimeOffset? validUntil)
    {
        if (validFrom.HasValue && validUntil.HasValue && validUntil < validFrom)
        {
            throw new ArgumentException("The end of a time range cannot precede its start.", nameof(validUntil));
        }
    }
}
