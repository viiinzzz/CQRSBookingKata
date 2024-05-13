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



                var hotelId = bus.AskResult<Id>(Recipient.Admin, Verb.Admin.RequestCreateHotel,
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