
using System.Dynamic;
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

      
        try
        {
            ExpandoObject? requestBodyObj = null;
            MemoryStream? requestBodyMemoryStream = null;
            if (context.Request.Body.CanRead) 
            {
                var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
                requestBodyMemoryStream = new MemoryStream();
                {
                    var requestBodyCopy = new StreamWriter(requestBodyMemoryStream, Encoding.UTF8);
                    await requestBodyCopy.WriteAsync(requestBody);
                    await requestBodyCopy.FlushAsync();
                    requestBodyMemoryStream.Seek(0, SeekOrigin.Begin);

                }
                context.Request.Body = requestBodyMemoryStream;
                requestBodyObj = requestBody.FromJsonToExpando();
            }

            Console.WriteLine(@$"
        <<<{tid}---------------------/{rid:000000}/
        | HTTP {context.Request.Method} {context.Request.Path}{context.Request.QueryString}
        |          from={context.Request.Host}
        +--( Request )----------------------------( {context.Response.StatusCode:000} )-----
        | {ToJsonDebug(requestBodyObj)}
            ");

            await next(context);

            requestBodyMemoryStream?.Dispose();

            ExpandoObject? responseBodyObj = null;
            if (context.Response.Body.CanRead)
            {
                var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
                await using var responseBodyMemoryStream = new MemoryStream();
                {
                    var responseBodyCopy = new StreamWriter(responseBodyMemoryStream, Encoding.UTF8);
                    await responseBodyCopy.WriteAsync(responseBody);
                    await responseBodyCopy.FlushAsync();
                    responseBodyMemoryStream.Seek(0, SeekOrigin.Begin);

                }
                context.Response.Body = responseBodyMemoryStream;
                responseBodyObj = responseBody.FromJsonToExpando();
            }

            Console.WriteLine(@$"
                +--{tid}---------------------/{rid:000000}/
                | HTTP {context.Request.Method} {context.Request.Path}{context.Request.QueryString}
                |          from={context.Request.Host}
                +--( Response )---------------------------( {context.Response.StatusCode:000} )-->>>
                | {ToJsonDebug(responseBodyObj)}
            ");

        }
        catch (OperationCanceledException ex)
        {
            Console.Error.WriteLine(@$"
                !--{tid}---------------------/{rid:000000}/
                | HTTP {context.Request.Method} {context.Request.Path}{context.Request.QueryString}
                |          from={context.Request.Host}
                !!!( Canceled )!!!!!!!!!!!!!!!!!!!!!!!!!!( {context.Response.StatusCode:000} )!!!XXX
Failure cause by {ex.GetType().Name}:
{ex.Message}
{ex.StackTrace}");

            logger.LogError("Request was cancelled");

            context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout; //408
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(@$"
                !--{tid}---------------------/{rid:000000}/
                | HTTP {context.Request.Method} {context.Request.Path}{context.Request.QueryString}
                |          from={context.Request.Host} 
                !!!( Aborted )!!!!!!!!!!!!!!!!!!!!!!!!!!!( {context.Response.StatusCode:000} )!!!XXX
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



    private static string ToJsonDebug(ExpandoObject? response)
    {
        try
        {
            if (response == null)
            {
                return response.ToJson();
            }

            var message = ((IDictionary<string, object>)response!)["Message"]?.ToString();

            if (message == null)
            {
                return response.ToJson(true);
            }

            var messageObj = message?.FromJson();

            var patchedNotification = response.PatchRelax(new
            {
                Message = messageObj
            });

            return patchedNotification.ToJson(true);
        }
        catch (Exception ex)
        {
            return response.ToJson(true);
        }
    }


}