using System.IO;
using System.Net;

namespace BookingKata.API.Infrastructure;

public class OperationCanceledMiddleware
(
    RequestDelegate next,
    ILogger<OperationCanceledMiddleware> logger)
{

    private static int RequestId = 0;

    public async Task Invoke(HttpContext context)
    {
        var rid = RequestId;
        var tid = context.Request.HttpContext.TraceIdentifier;
        RequestId++;

        // var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
        // await using var memoryStream = new MemoryStream();
        // new StreamWriter(memoryStream).Write(body);
        // // memoryStream.Seek(0, SeekOrigin.Begin);
        // memoryStream.Position = 0;
        // context.Response.Body = memoryStream;

        // var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        // await using var responseBodyMemoryStream = new MemoryStream();
        // new StreamWriter(responseBodyMemoryStream).Write(responseBody);
        // // responseBodyMemoryStream.Seek(0, SeekOrigin.Begin);
        // responseBodyMemoryStream.Position = 0;
        // context.Response.Body = responseBodyMemoryStream;

        try
        {
            Console.WriteLine(@$"
        <<<---------------------------------------------{rid:000000}
        | HTTP {context.Request.Method} {context.Request.Path}{context.Request.QueryString} ({context.Response.StatusCode})
        |          from={context.Request.Host} {tid}
        +-----------------------------------------------------
");
        // | {body}
// ");

            await next(context);

            Console.WriteLine(@$"
                +-----------------------------------------------{rid:000000}
                | HTTP {context.Request.Method} {context.Request.Path}{context.Request.QueryString} ({context.Response.StatusCode})
                |          from={context.Request.Host} {tid}
                +-------------------------------------------------->>>
");

        }
        catch (OperationCanceledException)
        {
            Console.Error.WriteLine(@$"
                !!!!!!!!!!!!!!!  Canceled  !!!!!!!!!!!!!!!!!!!!!{rid:000000}
                | HTTP {context.Request.Method} {context.Request.Path}{context.Request.QueryString} ({context.Response.StatusCode})
                |          from={context.Request.Host} {tid}
                !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!XXX
");

            logger.LogError("Request was cancelled");

            context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout; //408
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(@$"
                !!!!!!!!!!!!!!!!  Aborted  !!!!!!!!!!!!!!!!!!!!!{rid:000000}
                | HTTP {context.Request.Method} {context.Request.Path}{context.Request.QueryString} ({context.Response.StatusCode})
                |          from={context.Request.Host} {tid}
                !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!XXX
Failure cause by {ex.GetType().Name}:
{ex.Message}
{ex.StackTrace}");

            logger.LogError(@$"Request was aborted
Failure cause by {ex.GetType().Name}:
{ex.Message}
{ex.StackTrace}");

            context.Response.StatusCode = (int)HttpStatusCode.Conflict; //409
        }
    }
}