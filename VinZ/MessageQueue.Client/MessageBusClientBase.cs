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

namespace VinZ.MessageQueue;

public partial class MessageBusClientBase : IMessageBusClient, IDisposable
{
    private MessageBusHttp? _bus;
    public ILogger<IMessageBus>? log { get; set; }

    public int PingTimeoutMilliseconds { get; set; } = 2_000;
    public int DisposeDelaySeconds { get; set; } = 120;
    public int RetryDelaySeconds { get; set; } = 5;
    public int RetryMaxCount { get; set; } = 10;
    public int SubscribeWarmupMilliseconds { get; set; } = 1000;


    public ILogger<IMessageBus>? Log { get; set; }

    private LogLevel logLevelNetwork = LogLevel.Information;

    private string? _remoteHost;
    private string? _ping;

    public IMessageBusClient ConnectToBus(IScopeProvider scp)
    {
        var scope0 = scp.GetScope<IConfiguration>(out var appConfig);
        var scope1 = scp.GetScope<BusConfiguration>(out var busConfig);
        var scope2 = scp.GetScope<ITimeService>(out var dateTime);
        var scope3 = scp.GetScope<ILogger<IMessageBus>>(out var log);


        if (!Enum.TryParse<LogLevel>(appConfig["Logging:LogLevel:Default"], true, out var logLevelDefault))
        {
            logLevelDefault = LogLevel.Information;
        }

        if (!Enum.TryParse<LogLevel>(appConfig["Logging:LogLevel:MessageQueue:Network"], true, out logLevelNetwork))
        {
            logLevelNetwork = logLevelDefault;
        }

        var id = GetHashCode();

        var clientConfig = busConfig with
        {
            LocalUrl = $"{(busConfig.LocalUrl.EndsWith('/') ? busConfig.LocalUrl : busConfig.LocalUrl + '/')}{id.xby4()}"
        };

        _bus = new MessageBusHttp(clientConfig, appConfig, dateTime, log);

        var remoteUri = new Uri(clientConfig.RemoteUrl);
        _remoteHost = remoteUri.Host;
        // _ping = $"{clientConfig.RemoteUrl}{(clientConfig.RemoteUrl.EndsWith('/') ? "" : "/")}ping";
        _ping = $"{remoteUri.Scheme}://{remoteUri.Host}:{remoteUri.Port}/debug/ping";

        return this;
    }


    private async Task CheckBus()
    {
        if (_bus == null)
        {
            throw new InvalidOperationException("Not connected to a bus");
        }

        await ping();
    }

    public record RetryOptions
    (
        int WarmupDelayMilliseconds = default
    );

    public class MaxRetryReachedException(DateTime time, int retryCount, int retryMaxCount, Exception ex) 
        : Exception($"Max retry reached {retryCount}/{retryMaxCount}: {ex.Message}", ex)
    {

    }

    private async Task<T> Retry<T>(Func<T> action, RetryOptions? options = default)
    {
        var retryCount = 0;

        var t0 = DateTime.Now;

        if (options is { WarmupDelayMilliseconds: > 0 })
        {
            var dt = options.WarmupDelayMilliseconds;

            await Task.Delay(dt);
        }

        while (true)
        {
            retryCount++;

            try
            {
                var dt = RetryDelaySeconds * 1000;

                await Task.Delay(dt);

                await CheckBus();

                var t = action();

                return t;
            }
            catch (Exception ex)
            {
                var now = DateTime.Now;

                var elapsedSeconds = (now - t0).TotalSeconds;

                if (retryCount >= RetryMaxCount)
                {
                    log?.LogError($"[{now:O}] Bus error: max retry reached: {ex.Message}");

                    throw new MaxRetryReachedException(now, retryCount, RetryMaxCount, ex);
                }

                log?.LogInformation($"      --> waiting bus... {elapsedSeconds:0.0}s ({retryCount}/{RetryMaxCount})");
            }
        }
    }

    public void Dispose()
    {
        if (_bus == null)
        {
            return;
        }

        try
        {
            var done = Disconnect().Wait(DisposeDelaySeconds * 1000);
        }
        catch (Exception ex)
        {
            log?.LogError($"Failed to dispose {GetType().Name} bus: {_bus.Url}");

            // throw new Exception($"Failed to dispose {GetType().Name} bus: {_bus.Url}");
        }
    }

    public async Task<bool> Disconnect()
    {
        await CheckBus();

        var done = await Unsubscribe(Omni, AnyVerb);

        if (!done)
        {
            return false;
            // throw new InvalidOperationException("Disconnection failure");
        }

        _bus = null;
        _remoteHost = null;
        _ping = null;

        return true;
    }


    public virtual async Task Configure()
    {

    }


    public async Task Subscribe(string? recipient = default, string? verb = default)
    {
        try
        {
            var task = await Retry(() =>
            {
                _bus!.Subscribe(new SubscriptionRequest
                {
                    _type = $"{nameof(Subscribe)}",
                    name = GetType().Name,
                    url = _bus.Url.ToString(),
                    recipient = recipient,
                    verb =  verb
                }, 0);

                return true;
            }, new RetryOptions
            {
                WarmupDelayMilliseconds = SubscribeWarmupMilliseconds
            });
        }
        catch (MaxRetryReachedException ex)
        {
            log?.LogError(ex.Message);

            // return false;
        }
    }

    public async Task<bool> Unsubscribe(string? recipient = default, string? verb = default)
    {
        try
        {
            return await Retry(() =>
            {
                var done = _bus!.Unsubscribe(new SubscriptionRequest
                {
                    _type = $"{nameof(Unsubscribe)}",
                    name = GetType().Name,
                    url = _bus.Url.ToString(),
                    recipient = recipient,
                    verb = verb
                }, 0);

                return done;
            });
        }
        catch (MaxRetryReachedException ex)
        {
            log?.LogError(ex.Message);

            return false;
        }
    }


    public async Task<NotifyAck> Notify(IClientNotificationSerialized notification)
    {
        return await Retry(() =>
        {
            var ack = _bus!.Notify(notification, 0);

            return ack;
        });
    }

    public event EventHandler<IClientNotificationSerialized>? Notified;

    public virtual void OnNotified(IClientNotificationSerialized notification)
    {
        Notified?.Invoke(this, notification);
    }

    public TReturn? AskResult<TReturn>(string originator, string recipient, string verb, object? message)
    where TReturn : class
    {
        CheckBus();

        var ret = _bus!.AskResult<TReturn>(recipient, verb, message, originator);

        return ret;
    }

}
