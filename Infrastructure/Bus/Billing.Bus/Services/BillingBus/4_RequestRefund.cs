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

namespace Support.Infrastructure.Network;

public partial class BillingBus
{
    private void Verb_Is_RequestRefund(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<RefundRequest>();

        using var scope = sp.GetScope<IBillingCommandService>(out var billing);

        //
        //
        var refundId = billing.EmitRefund
        (
            request.receiptId,
            notification.CorrelationId1,
            notification.CorrelationId2
        );
        //
        //

        Notify(new ResponseNotification(notification, Omni, RefundEmitted, refundId));
    }
}