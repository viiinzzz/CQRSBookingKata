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

namespace BookingKata.Infrastructure.Network;

public partial class SalesBus(IScopeProvider sp, BookingConfiguration bconf) : MessageBusClientBase
{
    public override async Task Configure()
    {
        // await
        Subscribe(Recipient.Sales);

        Notified += (sender, notification) =>
        {
            try
            {
                switch (notification.Verb)
                {
                    case Verb.Sales.RequestOpenHotelSeason:
                    {
                        Verb_Is_RequestOpenHotelSeason(notification);
                        break;
                    }
                    case Verb.Sales.RequestCreateCustomer:
                    {
                        Verb_Is_RequestCreateCustomer(notification);
                        break;
                    }
                    case Verb.Sales.RequestBooking:
                    {
                        Verb_Is_RequestBook(notification);
                        break;
                    }
                    case Verb.Sales.RequestKpi:
                    {
                        Verb_Is_RequestKpi(notification);
                        break;
                    }
                    case RequestPage:
                    {
                        Verb_Is_RequestPage(notification);
                        break;
                    }

                    case Verb.Sales.RequestStay:
                    {
                        Verb_Is_RequestStay(notification);
                        break;
                    }

                    default:
                    {
                        throw new VerbInvalidException(notification.Verb);
                    }
                }
            }
            catch (Exception ex)
            {
                Notify(new NegativeResponseNotification(notification.Originator, notification, ex));
            }
        };
    }
}