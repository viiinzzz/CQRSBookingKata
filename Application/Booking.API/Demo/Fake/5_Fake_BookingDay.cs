namespace BookingKata.API.Demo;

public partial class DemoService
{
    private void Fake_BookingDay()
    {
        if (demoContext.FakeCustomerIds == null)
        {
            throw new Exception("FakeCustomer not ready yet.");
        }

        {
            var message = "Demo: Seeding Bookings {0}...";

            var args = new object[] { demoContext.SimulationDay };

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
                var ci = demoContext.FakeCustomerIds.Length.Rand();
                var cid = demoContext.FakeCustomerIds[ci];
                var c = demoContext.FakeCustomers[cid];

                var stayStartDays = 1 + 45.Rand();
                var stayDurationDays = 1 + 5.Rand();
                var personCount = 1 + 2.Rand();

                var r = new StayRequest(
                    ArrivalDate: DateTime.UtcNow.AddDays(stayStartDays),
                    DepartureDate: DateTime.UtcNow.AddDays(stayStartDays).AddDays(stayDurationDays),
                    PersonCount: personCount
                    ) { CityName = "Paris", CountryCode = "FR" };

                var preferredStayMatches = sales2.FindStay(r, cid)
                    .Take(20)//let's say customer only examine 20 first matches (at max)
                    .AsRandomEnumerable()
                    .Take(5)//and finally validate only 5 (at max)
                    .ToArray();

                if (!preferredStayMatches.Any())
                {
                    var message = "Demo: Booking skipped because no stay were found matching the customer's request!";

                    bus.Notify(new NegativeResponseNotification(Omni, AuditMessage, message)
                    {
                        Originator = originator,
                        Immediate = true
                    });

                    continue;
                    // throw new Exception("stay not found");
                }

                foreach (var m in preferredStayMatches)
                {
                    try
                    {
                        var lockProposition = sales2.LockStay(m, cid);

                        if (lockProposition == null)
                        {
                            continue; //the stay is not available anymore, go next
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

                            RandomHelper.Long(), RandomHelper.Long() //correlationId1, correlationId2
                        );

                        break; //booked, exit
                    }
                    catch (Exception ex2)
                    {
                        errors.Add(new InvalidOperationException($"Booking failure during Day+{demoContext.SimulationDay}", ex2));
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add(new InvalidOperationException($"Booking failure during Day+{demoContext.SimulationDay}", ex));
            }
        }

        if (errors.Count > 0)
        {
            throw new AggregateException($"Booking failures: {errors.Count}{Environment.NewLine}{Environment.NewLine}", errors);

        }
    }
}