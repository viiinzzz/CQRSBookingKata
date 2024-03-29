namespace BookingKata.API.Demo;

public partial class DemoService
{
    private void Fake_BookingDay(bool scoped)
    {
        if (demo.FakeCustomerIds == null)
        {
            throw new Exception("FakeCustomer not ready yet.");
        }

        Console.WriteLine($"Demo: Seeding Bookings {demo.SimulationDay}...");

        var todayBookingCount = (int)RandomHelper.Rand(CustomerCount * 0.05);

        var errors = new List<Exception>();

        for (var b = 0; b < todayBookingCount; b++)
        {
            try
            {
                var ci = RandomHelper.Rand(demo.FakeCustomerIds.Length);
                var cid = demo.FakeCustomerIds[ci];
                var c = demo.FakeCustomers[cid];

                var stayStartDays = 1 + RandomHelper.Rand(45);
                var stayDurationDays = 1 + RandomHelper.Rand(5);
                var personCount = 1 + RandomHelper.Rand(2);

                var r = new StayRequest(
                    ArrivalDate: DateTime.UtcNow.AddDays(stayStartDays),
                    DepartureDate: DateTime.UtcNow.AddDays(stayStartDays).AddDays(stayDurationDays),
                    PersonCount: personCount
                    ) { CityName = "Paris", CountryCode = "FR" };

                var preferredStayMatches = sales2.Find(r)
                    .Take(50)//let's say customer only examine 50 first matches (at max)
                    .AsRandomEnumerable()
                    .Take(10);//and finally validate only 10 (at max)

                if (preferredStayMatches.Count() == 0)
                {
                    Console.WriteLine("Demo: Booking skipped because no stay were found matching the customer's request!");
                    continue;
                    // throw new Exception("stay not found");
                }

                foreach (var m in preferredStayMatches)
                {
                    var lockProposition = sales2.LockProposition(m);

                    if (lockProposition == null)
                    {
                        continue;
                    }

                    var bookingId = booking.Book(lockProposition, cid, c.LastName, c.FirstName, c.DebitCardNumber, c.DebitCardSecrets,
                        scoped: false);

                    break;
                }
            }
            catch (Exception ex)
            {
                errors.Add(new InvalidOperationException($"Booking failure during Day+{demo.SimulationDay}", ex));
            }
        }

        if (errors.Count > 0)
        {
            throw new AggregateException($"Booking failures: {errors.Count}", errors);

        }
    }
}