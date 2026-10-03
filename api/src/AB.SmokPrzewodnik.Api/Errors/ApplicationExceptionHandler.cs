using AB.SmokPrzewodnik.Application.Common.Querying;
using Microsoft.AspNetCore.Diagnostics;

namespace AB.SmokPrzewodnik.Api.Errors;

internal sealed class ApplicationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not InvalidCursorException invalidCursor)
        {
            return false;
        }

        await Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid pagination cursor.",
                detail: invalidCursor.Message,
                extensions: new Dictionary<string, object?>
                {
                    ["code"] = invalidCursor.Code
                })
            .ExecuteAsync(httpContext);

        return true;
    }
}
