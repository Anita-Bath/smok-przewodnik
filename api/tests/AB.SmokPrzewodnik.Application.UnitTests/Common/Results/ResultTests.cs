using AB.SmokPrzewodnik.Application.Common.Errors;
using AB.SmokPrzewodnik.Application.Common.Results;
using Xunit;

namespace AB.SmokPrzewodnik.Application.UnitTests.Common.Results;

public sealed class ResultTests
{
    [Fact]
    public void Success_IsSuccessfulAndHasNoError()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void Failure_IsFailedAndExposesProvidedError()
    {
        var error = new Error("routing.no_route", "No route satisfies every required constraint.");

        var result = Result.Failure(error);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Failure_WithNoneError_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Result.Failure(Error.None));
    }
}
