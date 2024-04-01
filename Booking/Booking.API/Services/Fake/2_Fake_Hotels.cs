namespace BookingKata.API.Demo;

public partial class DemoService
{
    private void Fake_Hotels(bool scoped)
    {
        try
        {
            using var scope = !scoped ? null : new TransactionScope();

            bus.Notify(new NotifyMessage(Recipient.Audit, Verb.Audit.Information)
            {
                Message = "Demo: Seeding Hotels...",
                Immediate = true
            });

            demo.FakeHotelsIds = RandomHelper
                .GenerateFakeHotels(HotelCount)
                .Select((fake, hotelNum) =>
                {
                    var managerId = demo.FakeManagerIds[hotelNum];
                    var hotelId = admin.Create(new NewHotel(fake.HotelName, fake.Latitude, fake.Longitude));
                    admin.Update(hotelId, new UpdateHotel
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
                        admin.Create(new NewRooms(hotelId, floorNum, RoomPerFloor, PersonPerRoom), scoped: false);
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