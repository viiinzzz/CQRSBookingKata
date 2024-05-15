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

public partial class ThirdPartyBus
{
    private void Verb_Is_RequestPricing(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<PricingRequest>();

        var arrivalDateUtc = request.arrivalDateUtc.DeserializeUniversal_ThrowIfNull(nameof(request.arrivalDateUtc));
        var departureDateUtc = request.departureDateUtc.DeserializeUniversal_ThrowIfNull(nameof(request.departureDateUtc));

        var customerProfile = request.customerProfileJson == null
            ? null
            : JsonConvert.DeserializeObject<CustomerProfile>(request.customerProfileJson);


        using var scope = sp.GetScope<IPricingQueryService>(out var pricing);

        var price = pricing.GetPrice
        (
            request.personMaxCount,
            request.floorNum,
            request.floorNumMax,
            request.hotelRank,
            request.latitude,
            request.longitude,
            request.personCount,
            arrivalDateUtc,
            departureDateUtc,
            request.currency,
            customerProfile
        );

        Notify(new ResponseNotification(Omni, RespondPricing, price)
        {
            CorrelationId1 = notification.CorrelationId1, CorrelationId2 = notification.CorrelationId2,
        });
    }
}