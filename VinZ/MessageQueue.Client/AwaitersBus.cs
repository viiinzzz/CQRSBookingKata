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

public class AwaitersBus(AwaitedResponse[] awaitedResponses) : IMessageBusClient
{
    public int SubscribersCount = awaitedResponses.Length;

    // public IMessageBusClient ConnectToBus(IMessageBus bus)
    // {
    //     return this;
    // }

    public ILogger<IMessageBus>? Log { get; set; }

    public IMessageBusClient ConnectToBus(IScopeProvider scp)
    {
        return this;
    }

    public Task<bool> Disconnect()
    {
        return Task.FromResult(true);
    }

    public Task Configure()
    {
        return Task.CompletedTask;
    }

    public Task Subscribe(string? recipient, string? verb)
    {
        return Task.CompletedTask;
    }

    public Task<bool> Unsubscribe(string? recipient, string? verb)
    {
        return Task.FromResult(true);
    }

    public Task<NotifyAck> Notify(IClientNotificationSerialized notification)
    {
        return Task.FromResult(new NotifyAck
        {
            Valid = true,
            Status = 0,
            data = default,
            CorrelationId = notification.CorrelationId()
        });
    }

    public void OnNotified(IClientNotificationSerialized notification)
    {
        foreach (var awaitedResponse in awaitedResponses)
        {
            awaitedResponse.Respond(notification);
        }
    }

    public event EventHandler<IClientNotificationSerialized>? Notified;
}