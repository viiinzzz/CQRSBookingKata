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
    private void Verb_Is_RequestQuotation(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<QuotationRequest>();

        var optionStartUtc = request.optionStartUtc.DeserializeUniversal_ThrowIfNull(nameof(request.optionStartUtc));
        var optionEndUtc = request.optionEndUtc.DeserializeUniversal_ThrowIfNull(nameof(request.optionEndUtc));

        var jsonMeta = request.jsonMeta == default
            ? "{}"
            : JsonConvert.SerializeObject(JsonConvert.DeserializeObject(request.jsonMeta));

        using var scope = sp.GetScope<IBillingCommandService>(out var billing);

        //
        //
        var id = billing.EmitQuotation
        (
            request.price,
            request.currency,
            optionStartUtc,
            optionEndUtc,
            jsonMeta,
            request.referenceId,
            notification.CorrelationId1,
            notification.CorrelationId2
        );
        //
        //

        Notify(new ResponseNotification(notification, Omni, QuotationEmitted, id));
    }
}