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

using static VinZ.Common.MiniAnsi;
using System.Drawing;

namespace VinZ.MessageQueue;

public class MessageBusHttp : IMessageBus
{
    public Uri Url { get; private set; }
    private readonly HttpClient _remote = new();

    private readonly ITimeService DateTime;
    private readonly ILogger<IMessageBus> log;

    private static int RequestId = 500000;

    private LogLevel logLevelSubscribe = LogLevel.Warning;
    private LogLevel logLevelNotify = LogLevel.Warning;

    private static readonly string scheme = $"{Bg(Color.Gray)}{Fg(Color.Black)}HTTP{Rs}";

    private static readonly string postMethod = $"{Bg(Color.MediumSeaGreen)}{Fg(Color.Black)}POST{Rs}";


    public MessageBusHttp
    (
        BusConfiguration busConfig,
        IConfiguration appConfig,

        ITimeService dateTime,
        ILogger<IMessageBus> log
    )
    {
        // _isTrace = busConfig.IsTrace;

        if (!Enum.TryParse<LogLevel>(appConfig["Logging:LogLevel:Default"] , true, out var logLevelDefault))
        {
            logLevelDefault = LogLevel.Information;
        }

        if (!Enum.TryParse<LogLevel>(appConfig["Logging:LogLevel:MessageQueue.Subscribe"] , true, out logLevelSubscribe))
        {
            logLevelSubscribe = logLevelDefault;
        }

        if (!Enum.TryParse<LogLevel>(appConfig["Logging:LogLevel:MessageQueue.Notify"] , true, out logLevelNotify))
        {
            logLevelNotify = logLevelDefault;
        }

        DateTime = dateTime;
        var myUrl = busConfig.LocalUrl;
        if (!myUrl.EndsWith('/'))
        {
            myUrl += '/';
        }

        Url = new Uri(myUrl);

        var remoteUrl = busConfig.RemoteUrl;
        if (!remoteUrl.EndsWith('/'))
        {
            remoteUrl += '/';
        }

        _remote.BaseAddress = new Uri(remoteUrl);
        this.log = log;
    }

    private bool IsTraceSubscribe => logLevelSubscribe <= LogLevel.Trace;
    private bool IsTraceNotify => logLevelSubscribe <= LogLevel.Trace;


    public void Subscribe(SubscriptionRequest sub, int busId)
    {
        // if (busId != 0)
        // {
        //     Console.WriteLine($"==================subscribe from me {GetHashCode().xby4()} to busId {busId.xby4()}  me {sub.url} remote {(_remote.BaseAddress + nameof(Subscribe).ToLower())}");
        //     throw new ArgumentException("Only value 0 allowed", nameof(busId));
        // }

        var uri = nameof(Subscribe).ToLower();
        var url = _remote.BaseAddress + uri;

        int rid = RequestId++;

        try
        {
            var cancel = new CancellationTokenSource();

            var json = sub.ToJson();

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            if (IsTraceSubscribe) log.LogInformation(@$"
<<-( Subscribe )............................../{rid:000000}/
| {scheme}{postMethod} {Href(url)}
+.....................................................
{ToJsonDebug(sub)}
...");
            
            var post = _remote.PostAsync(uri, content, cancel.Token);

            post.Wait(cancel.Token);


            var res = post.Result;
            var statusCode = (int)res.StatusCode;
            var statusOk = statusCode >= 200 && statusCode < 300;

            if (IsTraceSubscribe) log.LogInformation(@$"
                        +..( Subscribe )............................../{rid:000000}/
                        | {scheme}{postMethod} {Href(url)}
                        +........................................( {Fg(statusOk ? Color.Green : Color.Red)}{(int)post.Result.StatusCode:000}{Rs} )....>>
");

            if (!statusOk)
            {
                throw new Exception($"({(int)res.StatusCode}) {res.ReasonPhrase}");
            }
        }
        catch (Exception ex)
        {
            if (ex.InnerException != null)
            {
                ex = ex.InnerException;
            }

            if (IsTraceSubscribe)
            {
                log.LogInformation(@$"
                        !..( {Fg(Color.Red)}Subscribe{Rs} )............................../{rid:000000}/
                        | {scheme}{postMethod} {Href(url)}
                        !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!( {(int)HttpStatusCode.InternalServerError:000} )!!!!!X

{(ex is HttpRequestException ? ex.Message : @$"!!! sensitive
{ex.Message}
{ex.StackTrace}
!!!")}");
            }

            throw new Exception($"{nameof(Subscribe)} failure: {url} {ex.Message}", ex);
        }
    }

    public bool Unsubscribe(SubscriptionRequest sub, int busId)
    {
        if (busId != 0)
        {
            throw new ArgumentException("Only value 0 allowed", nameof(busId));
        }

        var uri = nameof(Unsubscribe).ToLower();
        var url = _remote.BaseAddress + uri;

        int rid = RequestId++;


        try
        {
            var cancel = new CancellationTokenSource();

            var json = sub.ToJson();

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            if (IsTraceSubscribe) log.LogInformation(@$"
<<-( Unsubscribe )............................/{rid:000000}/
| {scheme}{postMethod} {Href(url)}
+.....................................................
{ToJsonDebug(sub)}
...");

            var post = _remote.PostAsync(uri, content, cancel.Token);

            post.Wait(cancel.Token);

            var res = post.Result;
            var statusCode = (int)res.StatusCode;
            var statusOk = statusCode >= 200 && statusCode < 300;

            if (IsTraceSubscribe) log.LogInformation(@$"
                        +..( Unsubscribe )............................/{rid:000000}/
                        | {scheme}{postMethod} {Href(url)} ({(int)post.Result.StatusCode})
                        +........................................( {Fg(statusOk ? Color.Green : Color.Red)}{(int)post.Result.StatusCode:000}{Rs} )....>>
");

            if (statusOk)
            {
                return true;
            }

            if (statusCode is >= 400 and < 499)
            {
                return false;
            }

            throw new Exception($"({(int)res.StatusCode}) {res.ReasonPhrase}");
        }
        catch (Exception ex)
        {
            if (ex.InnerException != null)
            {
                ex = ex.InnerException;
            }

            if (IsTraceSubscribe) log.LogInformation(@$"
                        !..( {Fg(Color.Red)}Unsubscribe{Rs} )............................/{rid:000000}/
                        | {scheme}{postMethod} {Href(url)}
                        !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!( {(int)HttpStatusCode.InternalServerError:000} )!!!!!X

{(ex is HttpRequestException ? ex.Message : @$"!!! sensitive
{ex.Message}
{ex.StackTrace}
!!!")}");

            throw new Exception($"{nameof(Unsubscribe)} failure: {url} {ex.Message}", ex);
        }
    }

    public NotifyAck Notify(IClientNotificationSerialized notification, int busId)
    {
        // if (busId != 0)
        // {
        //     Console.WriteLine($"==================notify from me {GetHashCode().xby4()} to busId {busId.xby4()} remote {(_remote.BaseAddress + nameof(Notify).ToLower())}");
        //     throw new ArgumentException("Only value 0 allowed", nameof(busId));
        // }

        var uri = nameof(Notify).ToLower();
        var url = _remote.BaseAddress + uri;
        
        int rid = RequestId++;

        var messageType =
            notification.Type == NotificationType.Response ? "Re: "
            : notification.Type == NotificationType.Advertisement ? "Ad: "
            : "";

        try
        {
            var cancel = new CancellationTokenSource();

            var json = notification.ToJsonIgnoring([ nameof(IHaveMessageObj.MessageObj) ]);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            if (IsTraceNotify) log.LogInformation(@$"
<<-( Notify )........................................./{rid:000000}/
| {scheme}{postMethod} {Href(url)}
| From: {notification.Originator ?? ""}
| To: {notification.Recipient ?? nameof(Omni)}
| Subject: {messageType}{notification.Verb ?? nameof(AnyVerb)}
+....{notification.CorrelationGuid()}...................
{ToJsonDebug(notification)}
...");

            var post = _remote.PostAsync(uri, content, cancel.Token);

            post.Wait(cancel.Token);

            var res = post.Result;
            var statusCode = (int)res.StatusCode;
            var statusOk = statusCode >= 200 && statusCode < 300;

            if (IsTraceNotify) log.LogInformation(@$"
                        +..( Notify )...................................../{rid:000000}/
                        | {scheme}{postMethod} {Href(url)}
                        | From: {notification.Originator ?? ""}
                        | To: {notification.Recipient ?? nameof(Omni)}
                        | Subject: {messageType}{notification.Verb ?? nameof(AnyVerb)}
                        +....{notification.CorrelationGuid()}..( {Fg(statusOk ? Color.Green : Color.Red)}{statusCode:000}{Rs} )....>>
");

            if (!statusOk)
            {
                return notification.Ack() with
                {
                    Valid = false,
                    Status = res.StatusCode,
                    data = res.ReasonPhrase
                };
            }

            var read = res.Content.ReadAsStringAsync(cancel.Token);

            read.Wait(cancel.Token);

            var ack = JsonConvert.DeserializeObject<NotifyAck>(read.Result);

            if (ack == null)
            {
                throw new NullReferenceException(nameof(ack));
            }

            return ack;
        }
        catch (Exception ex)
        {
            if (ex.InnerException != null)
            {
                ex = ex.InnerException;
            }

            if (IsTraceNotify) log.LogInformation(@$"
                        !--( {Fg(ex is HttpRequestException ? Color.Orange : Color.Red)}Notify{Rs} )-------------------------------------/{rid:000000}/
                        | {scheme}{postMethod} {Href(url)}
                        | From: {notification.Originator ?? ""}
                        | To: {notification.Recipient ?? nameof(Omni)}
                        | Subject: {messageType}{notification.Verb ?? nameof(AnyVerb)}
                        !!!!!{notification.CorrelationGuid()}!!!( {Fg(ex is HttpRequestException ? Color.Orange : Color.Red)}{(int)HttpStatusCode.InternalServerError:000}{Rs} )!!!!!X

{(ex is HttpRequestException ? ex.Message : @$"!!! sensitive
{ex.Message}
{ex.StackTrace}
!!!")}");

            // throw new Exception($"{nameof(Unsubscribe)} failure: {url} {ex.Message}", ex);

            return notification.Ack() with
            {
                Valid = false,
                Status = HttpStatusCode.InternalServerError,
                data = $"{nameof(Unsubscribe)} failure: {url} {ex.Message}"
            };
        }
    }



    public Task<IClientNotificationSerialized> Wait(NotifyAck ack, CancellationToken cancellationToken)
    {
        if (!ack.Valid)
        {
            throw new InvalidOperationException("Invalid wait. (ack.Valid == false)");
        }

        var correlationId = ack.CorrelationId;

        if (correlationId == null)
        {
            throw new InvalidOperationException("Uncorrelated wait. (ack.CorrelationId == null)");
        }

        AwaitedResponse awaitedResponse = new(correlationId.Value, DateTime, cancellationToken, Track, Untrack);

        var ret = awaitedResponse.ResultNotification;

        if (ret == null)
        {
            throw new InvalidOperationException("Unexpected wait");
        }

        return Task.FromResult(ret);
    }










    private readonly ConcurrentDictionary<string, AwaitedResponse> _awaiters = new();

    private void Track(AwaitedResponse awaitedResponse)
    {
        if (_awaiters.ContainsKey(awaitedResponse.Key))
        {
            throw new InvalidOperationException("Concurrent wait not allowed");
        }

        _awaiters[awaitedResponse.Key] = awaitedResponse;

        //
        //
        log.LogDebug($"...Track... Correlation={awaitedResponse.Key}");
        //
        //
    }

    private void Untrack(AwaitedResponse awaitedResponse)
    {
        //
        //
        log.LogDebug(
            $"        ...Untrack... Correlation={awaitedResponse.Key}, ElapsedSeconds={awaitedResponse.ElapsedSeconds}, Responded={awaitedResponse.Responded}, Cancelled={awaitedResponse.Cancelled}");
        //
        //

        if (!_awaiters.Remove(awaitedResponse.Key, out _))
        {
            throw new InvalidOperationException("Invalid wait state");
        }
    }


    private IMessageBusClient? GetAwaitedBus(IClientNotificationDeserialized notification)
    {
        if (notification.Type != NotificationType.Response)
        {
            return null;
        }

        var awaiters = _awaiters.Values
            .Where(awaitedResponse => awaitedResponse.IsCorrelatedTo(notification))
            .ToArray();

        var awaiterCount = awaiters.Length;

        var correlationId = new CorrelationId(notification.CorrelationId1, notification.CorrelationId2);
        //
        //
        log.LogDebug(
            $"...Awaiters... Count={awaiterCount}, CorrelationId={correlationId.Guid}, Recipient={notification.Recipient}, Verb={notification.Verb}");
        //
        //

        if (awaiterCount == 0)
        {
            return null;
        }

        return new AwaitersBus(awaiters);
    }


    private string ToJsonDebug(object obj)
    {
        var ret = obj.ToJson(true);

        return string.Join('\n', ret.Split('\n').Select(line =>
            
            $"{Faint}{line}{Rs}"
        
        ));
    }

}