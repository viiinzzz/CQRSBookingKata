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

namespace BookingKata.API.Demo;

public partial class DemoService
{
    private void Fake_Hotels()
    {
        try
        {
            var originator = GetType().FullName
                             ?? throw new ArgumentException("invalid originator");

            {
                var message = "Demo: Seeding Hotels...";

                bus.Notify(new AdvertisementNotification(message, [])
                {
                    Originator = originator,
                    Immediate = true
                });
            }

            int createHotel(int hotelNum, FakeHelper.FakeHotel fakeHotel)
            {
                var managerId = demoContext.FakeManagerIds[hotelNum];

                var newHotel = new NewHotel(fakeHotel.HotelName, fakeHotel.Latitude, fakeHotel.Longitude);



                var hotelId = bus.AskResult<Id<HotelRef>>(Recipient.Admin, Verb.Admin.RequestCreateHotel,
                    newHotel,
                    originator);

                if (hotelId == null)
                {
                    throw new ArgumentException(ReferenceInvalid, nameof(hotelId));
                }


                var modifyHotel = new ModifyHotel
                {
                    EarliestCheckInTime = 16_00,
                    LatestCheckOutTime = 10_00,
                    LocationAddress = fakeHotel.LocationAddress,
                    ReceptionPhoneNumber = fakeHotel.ReceptionTelephoneNumber,
                    Url = fakeHotel.Url,
                    Ranking = fakeHotel.ranking,
                    ManagerId = managerId,
                };

                var hotel = bus.AskResult<Hotel>(Recipient.Admin, Verb.Admin.RequestModifyHotel,
                    new IdData<ModifyHotel>(hotelId.id, modifyHotel),
                    originator);

                if (modifyHotel == null)
                {
                    throw new ArgumentException(ReferenceInvalid, nameof(modifyHotel));
                }

                var createHotelFloor = new CreateHotelFloors
                {
                    HotelId = hotelId.id,
                    FloorCount = FloorPerHotel,
                    RoomPerFloor = RoomPerFloor,
                    PersonPerRoom = PersonPerRoom
                };

                var rooms = bus.AskResult<Ids>(Recipient.Admin, Verb.Admin.RequestCreateFloorRooms,
                    createHotelFloor,
                    originator);

                if (rooms == null)
                {
                    throw new ArgumentException(ReferenceInvalid, nameof(rooms));
                }

                return hotelId.id;
            }

            demoContext.FakeHotelsIds = FakeHelper
                .GenerateFakeHotels(HotelCount)
                .AsParallel()
                .Select((fakeHotel, hotelNum) => createHotel(hotelNum, fakeHotel))
                .ToArray();
        }
        catch (Exception e)
        {
            throw new TransactionException($"transaction '{nameof(Fake_Hotels)}' failed", e);
        }
    }
}