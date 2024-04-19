namespace BookingKata.API.Demo;

public partial class DemoService
{
    private void Fake_BookingDay()
    {
        if (demo.FakeCustomerIds == null)
        {
            throw new Exception("FakeCustomer not ready yet.");
        }

        bus.Notify(originator, new Notification(Recipient.Audit, InformationMessage)
        {
            Message = $"Demo: Seeding Bookings {demo.SimulationDay}...",
            Immediate = true
        });

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

                var preferredStayMatches = sales2.FindStay(r, cid)
                    .Take(50)//let's say customer only examine 50 first matches (at max)
                    .AsRandomEnumerable()
                    .Take(10);//and finally validate only 10 (at max)

                if (preferredStayMatches.Count() == 0)
                {
                    bus.Notify(originator, new Notification(Recipient.Audit, ErrorProcessingRequest)
                    {
                        Message = "Demo: Booking skipped because no stay were found matching the customer's request!",
                        Immediate = true
                    });

                    continue;
                    // throw new Exception("stay not found");
                }

                foreach (var m in preferredStayMatches)
                {
                    var lockProposition = sales2.LockStay(m);

                    if (lockProposition == null)
                    {
                        continue;
                    }

                    var bookingId = booking.Book(
                        c.LastName, c.FirstName,
                        c.DebitCardNumber, c.DebitCardSecrets,
                        cid,
                        lockProposition,
                        correlationId1, correlationId2
                        );

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