﻿/*
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

using System.Drawing;
using System.Dynamic;
using System.Net.Mime;
using System.Text;
using static VinZ.Common.MiniAnsi;

namespace VinZ.Common;


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

        var scheme = $"{Bg(Color.Gray)}{Fg(Color.Black)}{context.Request.Scheme.ToUpper()}{Rs}";

        var method = context.Request.Method switch
        {
            "GET" => $"{Bg(Color.DeepSkyBlue)}{Fg(Color.Black)}{context.Request.Method}{Rs}",
            "POST" => $"{Bg(Color.MediumSeaGreen)}{Fg(Color.Black)}{context.Request.Method}{Rs}",
            "PUT" => $"{Bg(Color.Orange)}{Fg(Color.Black)}{context.Request.Method}{Rs}",
            "DELETE" => $"{Bg(Color.IndianRed)}{Fg(Color.Black)}{context.Request.Method}{Rs}",
            _ => $"{Bg(Color.Gray)}{Fg(Color.Black)}{context.Request.Method}{Rs}"
        };

        try
        {
            var requestHeaders = string.Join(Environment.NewLine, context.Request.Headers
                .Where(h => h.Key.StartsWith('_'))
                .Select(h => $"{Faint}---{Rs} {Fg(Color.DarkMagenta)}{h.Key}{Rs} {Faint}={Rs} {Bold}{Fg(Color.DarkMagenta)}{h.Value}{Rs}"));

            ExpandoObject? requestBodyObj = null;

            if (IsTraceRequest)
            {
                MemoryStream? requestBodyMemoryStream;
                if (context.Request.Body.CanRead)
                {
                    var requestMime = context.Request.ContentType  == null ? new ContentType(MediaTypeNames.Text.Plain) : new ContentType(context.Request.ContentType);
                    var requestEncoding = requestMime.CharSet == null ? Encoding.UTF8 : Encoding.GetEncoding(requestMime.CharSet);
                    var requestType = requestMime.MediaType;

                    var requestBody = await new StreamReader(context.Request.Body, requestEncoding).ReadToEndAsync();
                    requestBodyMemoryStream = new MemoryStream();
                    {

                        var requestBodyCopy = new StreamWriter(requestBodyMemoryStream, requestEncoding);
                        await requestBodyCopy.WriteAsync(requestBody);
                        await requestBodyCopy.FlushAsync();
                        requestBodyMemoryStream.Seek(0, SeekOrigin.Begin);

                    }
                    context.Request.Body = requestBodyMemoryStream;
                    requestBodyObj = requestBody.FromJsonToExpando();
                }

                logger.LogInformation(@$"
        <<-( {Bold}Request{Rs} )--------------------------------/{rid:000000}/
        | {scheme}{method} {Href(context.Request.Path + context.Request.QueryString)}
        | {Faint}ORIGIN {context.Request.Host}{Rs}
        +---/{tid}/--------------------------
{requestHeaders}
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

            var responseHeaders = string.Join(Environment.NewLine, context.Response.Headers
                .Where(h => h.Key.StartsWith('_'))
                .Select(h => $"{Faint}---{Rs} {Fg(Color.DarkMagenta)}{h.Key}{Rs} {Faint}={Rs} {Bold}{Fg(Color.DarkMagenta)}{h.Value}{Rs}"));

            var responseMime = context.Response.ContentType == null ? new ContentType(MediaTypeNames.Text.Plain) : new ContentType(context.Response.ContentType);
            var responseEncoding = responseMime.CharSet == null ? Encoding.UTF8 : Encoding.GetEncoding(responseMime.CharSet);
            var responseType = responseMime.MediaType;

            responseBodyMemoryStream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(context.Response.Body, responseEncoding).ReadToEndAsync();
            responseBodyMemoryStream.Seek(0, SeekOrigin.Begin);


            ExpandoObject? responseBodyObj = null;

            if (responseType == MediaTypeNames.Application.Json)
            {
                responseBodyObj = responseBody.FromJsonToExpando();
            }
            else
            {
                if (!string.IsNullOrEmpty(responseBody))
                {
                    responseBodyObj = new ExpandoObject();
                    var responseBodyDict = (IDictionary<string, object>)responseBodyObj!;

                    var bodyLines = new[]{ $"({responseBody.Length} char{(responseBody.Length > 1 ? "s": "")})" };

                    if (responseType.StartsWith("text") && !responseType.EndsWith("html"))
                    {
                        bodyLines = responseBody.Replace("\r", "").Split('\n');
                    }

                    responseBodyDict[responseType] = bodyLines;
                }
            }

            var dt = (DateTime.Now - t0).TotalMilliseconds;
            var statusStr = Enum.GetName(typeof(HttpStatusCode), context.Response.StatusCode);
            var statusOk = context.Response.StatusCode is >= 200 and < 300;
            // var status = (HttpStatusCode)context.Response.StatusCode;

            logger.LogInformation(@$"
                +--( {Fg(Color.Green)}Response{Rs} {Italic}{$"{dt,6:#####0}"}ms{Rs})-----------------------/{rid:000000}/
                | {scheme}{method} {Href(context.Request.Path + context.Request.QueryString)}
                | {Faint}ORIGIN {context.Request.Host}{Rs}
                +---/{tid}/--------------( {Fg(statusOk ? Color.Green : Color.Red)}{context.Response.StatusCode:000}{Rs} )--->>
{responseHeaders}
{(responseBodyObj == null ? $"({statusStr}) " : "")}{ToJsonDebug(responseBodyObj)}
---( Request )
{requestHeaders}
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
                !--( {Fg(Color.Orange)}Canceled{Rs} {Italic}{dt,6:#####0}ms{Rs})-----------------------/{rid:000000}/
                | {scheme}{method} {Href(context.Request.Path + context.Request.QueryString)}
                | {Faint}ORIGIN {context.Request.Host}{Rs}
                !!!!!{tid}!!!!!!!!!!!!!!( {Fg(Color.Orange)}{context.Response.StatusCode:000}{Rs} )!!!!!X
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
                !--( {Fg(Color.Red)}Aborted{Rs} {dt,6:#####0}ms)------------------------/{rid:000000}/
                | {scheme}{method} {Href(context.Request.Path + context.Request.QueryString)}
                | ORIGIN {Faint}{context.Request.Host}{Rs}
                !!!!!{tid}!!!!!!!!!!!!!!( {Fg(Color.Red)}{context.Response.StatusCode:000}{Rs} )!!!!!X
Failure cause by {ex.GetType().Name}:
{ex.Message}
{ex.StackTrace}
---");
        }
    }



    private static string ToJsonDebug(ExpandoObject? response)
    {
        var ret = ToJsonDebug_(response);

        return string.Join('\n', ret.Split('\n').Select(line => 
            
            $"{Faint}{line}{Rs}"
        
        ));
    }

    private static string ToJsonDebug_(ExpandoObject? response)
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