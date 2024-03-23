namespace CQRSBookingKata.API.Demo;

public partial class DemoService
{
    private void Fake_BookingDay(bool scoped)
    {
        if (demo.FakeCustomerIds == null)
        {
            throw new Exception("FakeCustomer not ready yet.");
        }

        var todayBookingCount = (int)RandomHelper.Rand(CustomerCount * 0.05);

        for (var b = 0; b < todayBookingCount; b++)
        {
            try
            {
                var ci = RandomHelper.Rand(demo.FakeCustomerIds.Length);
                var cid = demo.FakeCustomerIds[ci];
                var c = demo.FakeCustomers[ci];

                var stayStartDays = 1 + RandomHelper.Rand(45);
                var stayDurationDays = 1 + RandomHelper.Rand(5);
                var personCount = 1 + RandomHelper.Rand(2);

                var r = new StayRequest(
                    ArrivalDate: DateTime.UtcNow.AddDays(stayStartDays),
                    DepartureDate: DateTime.UtcNow.AddDays(stayStartDays).AddDays(stayDurationDays),
                    PersonCount: personCount
                    ) { CityName = "Paris" };

                var ms = sales2.Find(r);

                var mi = RandomHelper.Rand(ms.Count());
                var m = ms.Skip(mi).FirstOrDefault() ?? throw new Exception("stay not found");

                var p = sales2.LockProposition(m);

                var bookingId = booking.Book(p, cid, c.LastName, c.FirstName, c.DebitCardNumber, c.DebitCardSecrets,
                    scoped: false);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($@"
day+{demo.SimulationDay}
Booking failure: {ex.Message}
{ex.StackTrace}");
            }
        }
    }
}