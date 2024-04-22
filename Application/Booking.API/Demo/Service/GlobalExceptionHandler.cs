using System.Net.Mime;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace BookingKata.API;

internal class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger,
    ProblemDetailsFactory problemDetailsFactory) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext,
        Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Whoopsie");

        var problemDetails = problemDetailsFactory.CreateProblemDetails(httpContext,
            statusCode: StatusCodes.Status500InternalServerError,
            detail: exception.Message);

        httpContext.Response.ContentType = MediaTypeNames.Application.ProblemJson;
        httpContext.Response.StatusCode = (int)problemDetails.Status;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}