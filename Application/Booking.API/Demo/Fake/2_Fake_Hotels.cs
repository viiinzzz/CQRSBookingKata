namespace BookingKata.API.Demo;

public partial class DemoService
{
    private void Fake_Hotels(bool scoped)
    {
        try
        {
            using var scope = !scoped ? null : new TransactionScope();

            var originator = GetType().FullName
                             ?? throw new ArgumentException("invalid originator");

            bus.Notify(originator, new Notification(Recipient.Audit, InformationMessage)
            {
                Message = "Demo: Seeding Hotels...",
                Immediate = true
            });

            demo.FakeHotelsIds = FakeHelper
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
                    });


                    for (var floorNum = 0; floorNum < FloorPerHotel; floorNum++)
                    {
                        admin.Create(new NewRooms(hotelId, floorNum, RoomPerFloor, PersonPerRoom));
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