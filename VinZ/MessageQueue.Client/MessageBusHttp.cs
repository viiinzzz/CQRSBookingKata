﻿using System;

namespace VinZ.MessageQueue;

public class MessageBusHttp : IMessageBus
{
    public Uri Url { get; private set; }
    private readonly HttpClient _remote = new();

    private readonly ITimeService DateTime;
    private readonly ILogger<IMessageBus> log;

    public MessageBusHttp
    (
        string myUrl, 
        string remoteUrl,

        ITimeService dateTime,
        ILogger<IMessageBus> log
    )
    {
        DateTime = dateTime;
        if (!myUrl.EndsWith('/'))
        {
            myUrl += '/';
        }

        Url = new Uri(myUrl);


        if (!remoteUrl.EndsWith('/'))
        {
            remoteUrl += '/';
        }

        _remote.BaseAddress = new Uri(remoteUrl);
        this.log = log;
    }

    public void Subscribe(SubscriptionRequest sub, int busId)
    {
        if (busId != 0)
        {
            throw new ArgumentException("Only value 0 allowed", nameof(busId));
        }

        var uri = nameof(Subscribe).ToLower();

        try
        {
            var cancel = new CancellationTokenSource();

            var json = JsonConvert.SerializeObject(sub);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var post = _remote.PostAsync(uri, content, cancel.Token);

            post.Wait(cancel.Token);

            var res = post.Result;

            var statusCode = (int)res.StatusCode;

            if (statusCode is < 200 or > 299)
            {
                throw new Exception($"({(int)res.StatusCode}) {res.ReasonPhrase}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"{nameof(Subscribe)} failure: {_remote.BaseAddress + uri} {ex.Message}", ex);
        }
    }

    public bool Unsubscribe(SubscriptionRequest sub, int busId)
    {
        if (busId != 0)
        {
            throw new ArgumentException("Only value 0 allowed", nameof(busId));
        }

        var uri = nameof(Unsubscribe).ToLower();

        try
        {
            var cancel = new CancellationTokenSource();

            var json = JsonConvert.SerializeObject(sub);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var post = _remote.PostAsync(uri, content, cancel.Token);

            post.Wait(cancel.Token);

            var res = post.Result;

            var statusCode = (int)res.StatusCode;

            if (statusCode is >= 200 and < 299)
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
            throw new Exception($"{nameof(Unsubscribe)} failure: {_remote.BaseAddress + uri} {ex.Message}", ex);
        }
    }

    public INotifyAck Notify(IClientNotificationSerialized notification, int busId)
    {
        if (busId != 0)
        {
            throw new ArgumentException("Only value 0 allowed", nameof(busId));
        }

        try
        {
            var cancel = new CancellationTokenSource();

            var uri = nameof(Notify).ToLower();
            var json = JsonConvert.SerializeObject(notification);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var post = _remote.PostAsync(uri, content, cancel.Token);

            post.Wait(cancel.Token);

            var res = post.Result;

            var statusCode = (int)res.StatusCode;

            if (statusCode is < 200 or > 299)
            {
                return new NotifyAck
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
            return new NotifyAck
            {
                Valid = false,
                Status = HttpStatusCode.InternalServerError,
                data = ex.Message
            };
        }
    }




    public Task<IClientNotificationSerialized?> Wait(INotifyAck ack, CancellationToken cancellationToken)
    {
        var correlationId = ack.CorrelationId;

        if (correlationId == null)
        {
            throw new InvalidOperationException("Uncorrelated wait not allowed");
        }

        var awaitedResponse = new AwaitedResponse(correlationId, DateTime, cancellationToken, Track, Untrack);

        var ret = awaitedResponse?.ResultNotification;

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

        return new AwaitedBus(awaiters);
    }

   



}