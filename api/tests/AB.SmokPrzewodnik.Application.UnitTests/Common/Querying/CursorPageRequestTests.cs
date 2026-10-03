using AB.SmokPrzewodnik.Application.Common.Querying;
using Xunit;

namespace AB.SmokPrzewodnik.Application.UnitTests.Common.Querying;

public sealed class CursorPageRequestTests
{
    [Fact]
    public void Constructor_UsesTenAsDefaultLimit()
    {
        var request = new CursorPageRequest();

        Assert.Equal(10, request.Limit);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(201)]
    public void Constructor_RejectsLimitOutsideSupportedRange(int limit)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CursorPageRequest(limit: limit));
    }
}
