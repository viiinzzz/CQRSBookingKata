/*
 * Copyright (C) 2024 Vincent Fontaine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace BookingKata.API.Infrastructure;


public class MyDebugMiddleware
(
    RequestDelegate next,
    IConfiguration appConfig,
    ILogger<MyDebugMiddleware> logger
)
{
    private (LogLevel request, LogLevel response) logLevels = GetLogLevels(appConfig);

    private static (LogLevel request, LogLevel response) GetLogLevels(IConfiguration appConfig)
    {
        if (!Enum.TryParse<LogLevel>(appConfig["Logging:LogLevel:Default"], true, out var logLevelDefault))
        {
            logLevelDefault = LogLevel.Information;
        }

        if (!Enum.TryParse<LogLevel>(appConfig[$"Logging:LogLevel:{nameof(MyDebugMiddleware)}"], true, out var logLevel))
        {
            logLevel = logLevelDefault;
        }

        if (!Enum.TryParse<LogLevel>(appConfig[$"Logging:LogLevel:{nameof(MyDebugMiddleware)}.Request"], true, out var logLevelRequest))
        {
            logLevelRequest = logLevel;
        }

        if (!Enum.TryParse<LogLevel>(appConfig[$"Logging:LogLevel:{nameof(MyDebugMiddleware)}.Response"], true, out var logLevelResponse))
        {
            logLevelResponse = logLevel;
        }

        return (logLevelRequest, logLevelResponse);
    }

    private bool IsTraceRequest => logLevels.request == LogLevel.Trace;
    private bool IsTraceResponse => logLevels.response == LogLevel.Trace;


    private static int RequestId = 0;

    public async Task Invoke(HttpContext context)
    {
        var rid = RequestId;
        var tid = context.Request.HttpContext.TraceIdentifier;
        RequestId++;
        var t0 = DateTime.Now;
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

            if (IsTraceRequest) logger.LogInformation(@$"
        <<-( Request )--------------------------------/{rid:000000}/
        | {context.Request.Scheme.ToUpper()} {context.Request.Method} {context.Request.Path}{context.Request.QueryString}
        | ORIGIN {context.Request.Host}
        +---/{tid}/--------------------------
{ToJsonDebug(requestBodyObj)}
---");
          
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

            var dt = (DateTime.Now - t0).TotalMilliseconds;

            if (IsTraceResponse) logger.LogInformation(@$"
                +--( Response {$"{dt,6:#####0}"}ms)-----------------------/{rid:000000}/
                | {context.Request.Scheme.ToUpper()} {context.Request.Method} {context.Request.Path}{context.Request.QueryString}
                | ORIGIN {context.Request.Host}
                +---/{tid}/--------------( {context.Response.StatusCode:000} )--->>
{ToJsonDebug(responseBodyObj)}
---( Request )
{ToJsonDebug(requestBodyObj)}
---");

        }
        catch (OperationCanceledException ex)
        {
            var dt = (DateTime.Now - t0).TotalMilliseconds;

            logger.LogWarning(@$"
                !--( Canceled {dt,6:#####0}ms)-----------------------/{rid:000000}/
                | {context.Request.Scheme.ToUpper()} {context.Request.Method} {context.Request.Path}{context.Request.QueryString}
                | ORIGIN {context.Request.Host}
                !!!!!{tid}!!!!!!!!!!!!!!( {context.Response.StatusCode:000} )!!!!!X
Failure cause by {ex.GetType().Name}:
{ex.Message}
{ex.StackTrace}
---");

            context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout; //408
        }
        catch (Exception ex)
        {
            var dt = (DateTime.Now - t0).TotalMilliseconds;

            // if (IsTraceResponse)
                logger.LogError(@$"
                !--( Aborted {dt,6:#####0}ms)------------------------/{rid:000000}/
                | {context.Request.Scheme.ToUpper()} {context.Request.Method} {context.Request.Path}{context.Request.QueryString}
                | ORIGIN {context.Request.Host} 
                !!!!!{tid}!!!!!!!!!!!!!!( {context.Response.StatusCode:000} )!!!!!X
Failure cause by {ex.GetType().Name}:
{ex.Message}
{ex.StackTrace}
---");

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