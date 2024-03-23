namespace CQRSBookingKata.API.Demo;

public partial class DemoService
{
    private int day = 0;

    private void Fake_BookingDay(bool scoped)
    {
        DateTime.Forward(TimeSpan.FromDays(1));
        day++;

        var todayBookingCount = (int)RandomHelper.Rand(CustomerCount * 0.05);

        for (var b = 0; b < todayBookingCount; b++)
        {
            try
            {
                var ci = RandomHelper.Rand(fakeCustomerIds.Length);
                var cid = fakeCustomerIds[ci];
                var c = fakeCustomers[ci];

                var stayStartDays = 1 + RandomHelper.Rand(45);
                var stayDurationDays = 1 + RandomHelper.Rand(5);
                var personCount = 1 + RandomHelper.Rand(2);

                var r = new StayRequest(
                    ArrivalDate: DateTime.UtcNow.AddDays(stayStartDays),
                    DepartureDate: DateTime.UtcNow.AddDays(stayStartDays).AddDays(stayDurationDays),
                    PersonCount: personCount
                    ) { CityName = "Paris" };

                var ms = _sales2.Find(r);

                var mi = RandomHelper.Rand(ms.Count());
                var m = ms.Skip(mi).FirstOrDefault() ?? throw new Exception("stay not found");

                var p = _sales2.LockProposition(m);

                var bookingId = _booking.Book(p, cid, c.LastName, c.FirstName, c.DebitCardNumber, c.DebitCardSecrets,
                    scoped: false);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($@"
day+{day}
Booking failure: {ex.Message}
{ex.StackTrace}");
            }
        }
    }
}