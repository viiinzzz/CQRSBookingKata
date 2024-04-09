namespace BookingKata.API.Infrastructure;

public class OperationCanceledMiddleware
(
    RequestDelegate next,
    ILogger<OperationCanceledMiddleware> logger)
{
 
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Request was cancelled");

            context.Response.StatusCode = 409;
        }
    }
}