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

public class MessageBusClientNoNetwork : IMessageBusClient //ancien MessageBusClientBase
{
    private IMessageBus? _bus;

    private void CheckBus()
    {
        if (_bus == null)
        {
            throw new InvalidOperationException("Not connected to a bus");
        }
    }


    // public IMessageBusClient ConnectToBus(IMessageBus bus)
    // {
    //     if (_bus != null)
    //     {
    //         throw new InvalidOperationException("Already connected to a bus");
    //     }
    //
    //     _bus = bus;
    //
    //     return this;
    // }

    public ILogger<IMessageBus>? Log { get; set; }

    public IMessageBusClient ConnectToBus(IScopeProvider scp)
    {
        if (_bus != null)
        {
            throw new InvalidOperationException("Already connected to a bus");
        }

        var scope = scp.GetScope<IMessageBus>(out var bus);

        _bus = bus;

        return this;
    }

    public async Task<bool> Disconnect()
    {
        CheckBus();


        var done = await Unsubscribe(Omni, AnyVerb);

        if (!done)
        {
            return false;
        }

        _bus = null;

        // if (!done)
        // {
        //     throw new InvalidOperationException("Disconnection failure");
        // }

        return true;
    }


    public virtual async Task Configure() { }


    public Task Subscribe(string? recipient = default, string? verb = default)
    {
        CheckBus();

        _bus!.Subscribe(new SubscriptionRequest
        {
            _type = $"{nameof(Subscribe)}",
            name = null,
            url = null,
            recipient = recipient,
            verb = verb
        });

        return Task.CompletedTask;
    }

    public Task<bool> Unsubscribe(string? recipient = default, string? verb = default)
    {
        CheckBus();

        var done = _bus!.Unsubscribe(new SubscriptionRequest
        {
            _type = $"{nameof(Unsubscribe)}",
            name = null,
            url = null,
            recipient = recipient,
            verb = verb
        });

        return Task.FromResult(done);
    }


    public Task<NotifyAck> Notify(IClientNotificationSerialized notification)
    {
        CheckBus();

        _bus!.Notify(notification);

        return Task.FromResult(notification.Ack() with
        {
            Valid = true,
            Status = 0,
        });
    }

    public event EventHandler<IClientNotificationSerialized>? Notified;

    public virtual void OnNotified(IClientNotificationSerialized notification)
    {
        Notified?.Invoke(this, notification);
    }

    public TReturn? AskResult<TReturn>(string originator, string[] steps, string recipient, string verb, object? message)
        where TReturn : class
    {
        CheckBus();

        var ret = _bus!.AskResult<TReturn>(recipient, verb, message, originator, steps);

        return ret;
    }

}