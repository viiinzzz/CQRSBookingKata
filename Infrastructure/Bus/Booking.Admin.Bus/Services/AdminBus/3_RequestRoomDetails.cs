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

public partial class AdminBus
{
    private void Verb_Is_RequestHotelRoomDetails(IClientNotificationSerialized notification)
    {
        using var scope = sp.GetScope<AdminQueryService>(out var adminQueryService);

        var request = notification.MessageAs<RoomDetailsRequest>();

        if (request.hotelId == null)
        {
            throw new ArgumentException(ReferenceInvalid, nameof(request.hotelId));
        }

        //
        //
        var roomDetails = adminQueryService
            .GetHotelRoomDetails(request.hotelId.Value, request.exceptRoomNumbers, request.onlyRoomNumbers)
            .ToArray();
        //
        //

        Notify(notification.Response(new ResponseOptions
        {
            Recipient = Omni, 
            Verb = RespondHotelRoomDetails, 
            MessageObj = roomDetails
        }));
    }
    
    private void Verb_Is_RequestSingleRoomDetails(IClientNotificationSerialized notification)
    {
        using var scope = sp.GetScope<AdminQueryService>(out var adminQueryService);

        var roomId = notification.MessageAs<Id<RoomRef>>();

        //
        //
        var roomDetails = adminQueryService
            .GetSingleRoomDetails(roomId.id);
        //
        //

        Notify(notification.Response(new ResponseOptions {
            Recipient = Omni, 
            Verb = RespondSingleRoomDetails,
            MessageObj =  roomDetails
        }));
    } 
    
    
    private void Verb_Is_RequestManyRoomDetails(IClientNotificationSerialized notification)
    {
        using var scope = sp.GetScope<AdminQueryService>(out var adminQueryService);

        var request = notification.MessageAs<RoomDetailsRequest>();

        if (request.hotelId != null)
        {
            throw new ArgumentException(ReferenceUnexpected, nameof(request.hotelId));
        }

        if (request.exceptRoomNumbers != null)
        {
            throw new ArgumentException(ReferenceUnexpected, nameof(request.exceptRoomNumbers));
        }

        if (request.onlyRoomNumbers == null)
        {
            throw new ArgumentException(ReferenceInvalid, nameof(request.onlyRoomNumbers));
        }

        //
        //
        var roomDetails = adminQueryService
            .GetManyRoomDetails(request.onlyRoomNumbers);
        //
        //

        Notify(notification.Response(new ResponseOptions { 
            Recipient = Omni,
            Verb = RespondSingleRoomDetails, 
            MessageObj = roomDetails
        }));
    }
}