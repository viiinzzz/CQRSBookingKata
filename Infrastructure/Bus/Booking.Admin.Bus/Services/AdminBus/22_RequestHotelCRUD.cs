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
    private void Verb_Is_RequestDisableHotel(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<IdDisable<Hotel>>();

        using var scope = sp.GetScope<IAdminRepository>(out var repo);

        var hotel = repo.DisableHotel(request.id, request.disable);

        Notify(new ResponseNotification(notification, notification.Originator, HotelDisabled, hotel));
    }

    private void Verb_Is_RequestModifyHotel(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<IdData<ModifyHotel>>();

        using var scope = sp.GetScope<IAdminRepository>(out var repo);

        var hotel = repo.Update(request.id, request.data);

        Notify(new ResponseNotification(notification, notification.Originator, HotelModified, hotel));
    }

    private void Verb_Is_RequestFetchHotel(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<Id<HotelRef>>();

        using var scope = sp.GetScope<IAdminRepository>(out var repo);

        var hotel = repo.GetHotel(request.id);

        Notify(new ResponseNotification(notification, notification.Originator, HotelFetched, hotel));
    }

    private void Verb_Is_RequestFetchHotelList(IClientNotificationSerialized notification)
    {
        using var scope = sp.GetScope<IAdminRepository>(out var repo);

        IdCollection<Hotel> list = new([.. repo.Hotels.Select(hotel => hotel.HotelId)]);

        Notify(new ResponseNotification(notification, notification.Originator, HotelListFetched, list));
    }
    
    private void Verb_Is_RequestFetchHotelGeoProxy(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<Id<HotelRef>>();

        using var scope = sp.GetScope<IAdminRepository>(out var repo);

        var ret = repo.GetHotel(request.id);

        var geoProxy = ret?.GetGeoProxy();
        
        Notify(new ResponseNotification(notification, notification.Originator, HotelGeoProxyFetched, geoProxy));
    }

    private void Verb_Is_RequestCreateHotel(IClientNotificationSerialized notification)
    {
        var request = notification.MessageAs<NewHotel>();

        using var scope = sp.GetScope<IAdminRepository>(out var repo);

        var hotelId = repo.Create(request);

        var id = new Id<HotelRef>(hotelId);

        Notify(new ResponseNotification(notification, notification.Originator, HotelCreated, id));
    }

    private void Verb_Is_RequestCreateFloorRooms(IClientNotificationSerialized notification)
    {
        var floors = notification.MessageAs<CreateHotelFloors>();

        using var scope = sp.GetScope<IAdminRepository>(out var repo);

        var urids = new List<int>();

        for (var floorNum = 0; floorNum < floors.FloorCount; floorNum++)
        {
            var floor = new CreateHotelFloor(floors.HotelId, floorNum, floors.RoomPerFloor, floors.PersonPerRoom);

            urids.AddRange(repo.Create(floor));
        }

        var ids = new Ids([.. urids]);

        Notify(new ResponseNotification(notification, notification.Originator, HotelCreated, ids));
    }
}