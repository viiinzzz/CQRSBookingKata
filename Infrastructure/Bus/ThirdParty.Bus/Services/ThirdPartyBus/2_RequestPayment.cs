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

namespace Support.Infrastructure.Network;

public partial class ThirdPartyBus
{
    private void Verb_Is_RequestPayment(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<PaymentRequest>();


        using var scope = sp.GetScope<IPaymentCommandService>(out var payment);


        //
        //
        var response = payment.RequestPayment
        (
            request.referenceId,

            request.amount,
            request.currency,

            request.debitCardNumber,
            request.debitCardOwnerName,
            request.debitCardExpire,
            request.debitCardCCV,

            request.vendorId,
            request.terminalId
        );
        //
        //

        var responseVerb = response.Accepted ? PaymentAccepted : PaymentRefused;

        Notify(new ResponseNotification(notification, Omni, responseVerb, response));
    }
}