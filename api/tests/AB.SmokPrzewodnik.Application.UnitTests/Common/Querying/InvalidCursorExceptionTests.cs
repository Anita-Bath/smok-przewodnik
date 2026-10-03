using AB.SmokPrzewodnik.Application.Common.Querying;
using Xunit;

namespace AB.SmokPrzewodnik.Application.UnitTests.Common.Querying;

public sealed class InvalidCursorExceptionTests
{
    [Fact]
    public void Exception_ProvidesStableSafeClientError()
    {
        const string suppliedCursor = "secret-invalid-cursor";

        var exception = new InvalidCursorException();

        Assert.Equal("pagination.invalid_cursor", exception.Code);
        Assert.Equal("The pagination cursor is invalid.", exception.Message);
        Assert.DoesNotContain(suppliedCursor, exception.Message);
    }
}
