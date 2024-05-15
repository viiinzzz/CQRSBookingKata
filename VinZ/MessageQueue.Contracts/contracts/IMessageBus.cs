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

public interface IMessageBus
{
    // void Subscribe(IMessageBusClient client, string? recipient, string? verb);
    // bool Unsubscribe(IMessageBusClient client, string? recipient, string? verb);
    void Subscribe(SubscriptionRequest sub, int busId = 0);
    bool Unsubscribe(SubscriptionRequest sub, int busId = 0);

    NotifyAck Notify(IClientNotificationSerialized notification, int busId = 0);

    Task<IClientNotificationSerialized?> Wait(NotifyAck ack, CancellationToken cancellationToken);
}