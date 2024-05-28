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

using Newtonsoft.Json.Linq;
using System;
using System.IO;

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
        var rid = RequestId++;
        var tid = context.Request.HttpContext.TraceIdentifier;
        var t0 = DateTime.Now;

        try
        {

            ExpandoObject? requestBodyObj = null;

            if (IsTraceRequest)
            {
                MemoryStream? requestBodyMemoryStream;
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

                logger.LogInformation(@$"
        <<-( Request )--------------------------------/{rid:000000}/
        | {context.Request.Scheme.ToUpper()} {context.Request.Method} {context.Request.Path}{context.Request.QueryString}
        | ORIGIN {context.Request.Host}
        +---/{tid}/--------------------------
{ToJsonDebug(requestBodyObj)}
---");
            }

            if (!IsTraceResponse)
            {
                //
                //
                await next(context);
                //
                //

                return;
            }

            var originalBodyStream = context.Response.Body;
            await using var responseBodyMemoryStream = new MemoryStream();
            context.Response.Body = responseBodyMemoryStream;

            //
            //
            await next(context);
            //
            //

            responseBodyMemoryStream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(context.Response.Body, Encoding.UTF8).ReadToEndAsync();
            responseBodyMemoryStream.Seek(0, SeekOrigin.Begin);


            ExpandoObject? responseBodyObj = null;

            if (context.Response.ContentType == "application/json")
            {
                responseBodyObj = responseBody.FromJsonToExpando();
            }
            else
            {
                if (!string.IsNullOrEmpty(responseBody))
                {
                    responseBodyObj = new ExpandoObject();
                    ((IDictionary<string, object>)responseBodyObj!)[context.Response.ContentType ?? "text"] =
                        responseBody.Replace("\r", "").Split('\n');
                }
            }

            var dt = (DateTime.Now - t0).TotalMilliseconds;
            var statusStr = Enum.GetName(typeof(HttpStatusCode), context.Response.StatusCode);
                
            logger.LogInformation(@$"
                +--( Response {$"{dt,6:#####0}"}ms)-----------------------/{rid:000000}/
                | {context.Request.Scheme.ToUpper()} {context.Request.Method} {context.Request.Path}{context.Request.QueryString}
                | ORIGIN {context.Request.Host}
                +---/{tid}/--------------( {context.Response.StatusCode:000} )--->>
{(responseBodyObj == null ? $"({statusStr})" : "")}{ToJsonDebug(responseBodyObj)}
---( Request )
{ToJsonDebug(requestBodyObj)}
---");

            await responseBodyMemoryStream.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;
        }
        catch (OperationCanceledException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout; //408

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
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Conflict; //409

            if (ex.InnerException != null) ex = ex.InnerException;

            var dt = (DateTime.Now - t0).TotalMilliseconds;

            logger.LogError(@$"
                !--( Aborted {dt,6:#####0}ms)------------------------/{rid:000000}/
                | {context.Request.Scheme.ToUpper()} {context.Request.Method} {context.Request.Path}{context.Request.QueryString}
                | ORIGIN {context.Request.Host} 
                !!!!!{tid}!!!!!!!!!!!!!!( {context.Response.StatusCode:000} )!!!!!X
Failure cause by {ex.GetType().Name}:
{ex.Message}
{ex.StackTrace}
---");
        }
    }



    private static string ToJsonDebug(ExpandoObject? response)
    {
        try
        {
            if (response == null)
            {
                return "(empty)";
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