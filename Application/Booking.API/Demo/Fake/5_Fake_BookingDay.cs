namespace BookingKata.API.Demo;

public partial class DemoService
{
    private void Fake_BookingDay()
    {
        if (demo.FakeCustomerIds == null)
        {
            throw new Exception("FakeCustomer not ready yet.");
        }

        {
            var message = "Demo: Seeding Bookings {0}...";

            var args = new object[] { demo.SimulationDay };

            bus.Notify(new AdvertisementNotification(message, args)
            {
                Originator = originator,
                Immediate = true
            });
        }

        var todayBookingCount = (int)(CustomerCount * 0.05).Rand();

        var errors = new List<Exception>();

        var vendor = new VendorIdentifiers(7777_7777, 007);

        for (var b = 0; b < todayBookingCount; b++)
        {
            try
            {
                var ci = demo.FakeCustomerIds.Length.Rand();
                var cid = demo.FakeCustomerIds[ci];
                var c = demo.FakeCustomers[cid];

                var stayStartDays = 1 + 45.Rand();
                var stayDurationDays = 1 + 5.Rand();
                var personCount = 1 + 2.Rand();

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
                    var message = "Demo: Booking skipped because no stay were found matching the customer's request!";

                    bus.Notify(new NegativeResponseNotification(Omni, InformationMessage, message)
                    {
                        Originator = originator,
                        Immediate = true
                    });

                    continue;
                    // throw new Exception("stay not found");
                }

                foreach (var m in preferredStayMatches)
                {
                    var lockProposition = sales2.LockStay(m, cid);

                    if (lockProposition == null)
                    {
                        continue;
                    }

                    var bookingId = booking.Book
                    (
                        c.LastName,
                        c.FirstName,

                        c.DebitCardNumber,
                        c.DebitCardSecrets,
                        vendor,

                        cid,
                        
                        lockProposition.StayPropositionId,

                        RandomHelper.Long(), RandomHelper.Long()//correlationId1, correlationId2
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