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

public partial class AdminBus(IScopeProvider sp, BookingConfiguration bconf) : MessageBusClientBase
{
    public override async Task Configure()
    {
        // await
        Subscribe(Recipient.Admin);

        Notified += (sender, notification) =>
        {
            try
            {
                switch (notification.Verb)
                {

                    case RequestPage:
                    {
                        Verb_Is_RequestPage(notification);
                        break;
                    }


                    case RequestCreateEmployee:
                    {
                        Verb_Is_RequestCreateEmployee(notification);
                        break;
                    }

                    case RequestFetchEmployee:
                    {
                        Verb_Is_RequestFetchEmployee(notification);
                        break;
                    }

                    case RequestModifyEmployee:
                    {
                        Verb_Is_RequestModifyEmployee(notification);
                        break;
                    }

                    case RequestDisableEmployee:
                    {
                        Verb_Is_RequestDisableEmployee(notification);
                        break;
                    }


                    case RequestCreateHotel:
                    {
                        Verb_Is_RequestCreateHotel(notification);
                        break;
                    }

                    case RequestCreateFloorRooms:
                    {
                        Verb_Is_RequestCreateFloorRooms(notification);
                        break;
                    }

                    case RequestFetchHotel:
                    {
                        Verb_Is_RequestFetchHotel(notification);
                        break;
                    }

                    case RequestFetchHotelList:
                    {
                        Verb_Is_RequestFetchHotelList(notification);
                        break;
                    }

                    case RequestFetchHotelGeoProxy:
                    {
                        Verb_Is_RequestFetchHotelGeoProxy(notification);
                        break;
                    }

                    case RequestModifyHotel:
                    {
                        Verb_Is_RequestModifyHotel(notification);
                        break;
                    }

                    case RequestDisableHotel:
                    {
                        Verb_Is_RequestDisableHotel(notification);
                        break;
                    }


                    case RequestHotelRoomDetails:
                    {
                        Verb_Is_RequestHotelRoomDetails(notification);
                        break;
                    }

                    case RequestSingleRoomDetails:
                    {
                        Verb_Is_RequestSingleRoomDetails(notification);
                        break;
                    }

                    case RequestManyRoomDetails:
                    {
                        Verb_Is_RequestManyRoomDetails(notification);
                        break;
                    }

                    case RequestServerContext:
                    {
                        Verb_Is_RequestServerContext(notification);
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