namespace AB.SmokPrzewodnik.Application.Common.Querying;

public sealed class InvalidCursorException : Exception
{
    public InvalidCursorException()
        : base("The pagination cursor is invalid.")
    {
    }

    public string Code => "pagination.invalid_cursor";
}
