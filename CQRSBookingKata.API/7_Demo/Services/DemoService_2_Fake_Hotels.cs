namespace CQRSBookingKata.API.Demo;

public partial class DemoService
{
    public int[] fakeHotelsIds { get; private set; }

    private void Fake_Hotels(bool scoped)
    {
        try
        {
            using var scope = !scoped ? null : new TransactionScope();

            fakeHotelsIds = RandomHelper
                .GenerateFakeHotels(3)
                .Select((fake, hotelNum) =>
                {
                    var managerId = fakeManagerIds[hotelNum];
                    var hotelId = _admin.Create(new NewHotel(fake.HotelName, fake.Latitude, fake.Longitude));
                    _admin.Update(hotelId, new UpdateHotel
                    {
                        EarliestCheckInTime = 16_00,
                        LatestCheckOutTime = 10_00,
                        LocationAddress = fake.LocationAddress,
                        ReceptionPhoneNumber = fake.ReceptionTelephoneNumber,
                        Url = fake.Url,
                        Ranking = fake.ranking,
                        ManagerId = managerId,
                    }, scoped: false);


                    for (var floorNum = 0; floorNum < FloorPerHotel; floorNum++)
                    {
                        _admin.Create(new NewRooms(hotelId, floorNum, RoomPerFloor, PersonPerRoom), scoped: false);
                    }

                    return hotelId;
                })
                .ToArray();

            scope?.Complete();
        }
        catch (Exception e)
        {
            throw new TransactionException($"transaction '{nameof(Fake_Hotels)}' failed", e);
        }
    }
}