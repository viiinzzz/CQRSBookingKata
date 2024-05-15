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

namespace BookingKata.Infrastructure.Network;

public partial class SalesBus
{
    private void Verb_Is_RequestStay(IClientNotificationSerialized notification)
    {
        var pageRequest = notification.MessageAs<PageRequest>();
        var stayRequest = pageRequest.Filter as StayRequest;

        if (stayRequest == null)
        {
            throw new ArgumentNullException(nameof(pageRequest.Filter));
        }

        using var scope = sp.GetScope<SalesQueryService>(out var sales);

        //TODO!!!!
        var currentCustomerId = 9999_9999;

        //
        //
        var page = sales
            .FindStay(stayRequest, currentCustomerId)
            .Page($"/booking", pageRequest.Page, pageRequest.PageSize);
        //
        //

        Notify(new ResponseNotification(Omni, Verb.Sales.StayFound, page)
        {
            CorrelationId1 = notification.CorrelationId1, CorrelationId2 = notification.CorrelationId2
        });
    }
}