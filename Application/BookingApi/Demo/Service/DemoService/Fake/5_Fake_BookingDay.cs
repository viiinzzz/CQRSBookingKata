/*
 * Copyright (C) 2024 Vincent Fontaine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace BookingKata.API.Demo;

public partial class DemoBus
{
    private void Fake_BookingDay()
    {
        if (demoContextService.FakeCustomerIds == null)
        {
            throw new Exception("FakeCustomer not ready yet.");
        }



        using var scope1 = sp.GetScope<SalesQueryService>(out var sales2);
        using var scope2 = sp.GetScope<BookingCommandService>(out var booking);


        {
            var message = "Demo: Seeding Bookings {0}...";

            var args = new object[] { demoContextService.SimulationDay };

            bus.Notify(new AdvertisementNotification(message, args)
            {
                Originator = originator,
                Immediate = true
            });
        }

        var todayBookingCount = (int)(CustomerCount * 0.05).Rand();

        if (todayBookingCount <= 0)
        {
            todayBookingCount = 1;
        }

        var errors = new List<Exception>();

        var vendor = new VendorIdentifiers(7777_7777, 007);

        for (var b = 0; b < todayBookingCount; b++)
        {
            try
            {
                var ci = demoContextService.FakeCustomerIds.Length.Rand();
                var cid = demoContextService.FakeCustomerIds[ci];
                var c = demoContextService.FakeCustomersDictionary[cid];

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
                        errors.Add(new InvalidOperationException($"Booking failure during Day+{demoContextService.SimulationDay}", ex2));
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is StayNotFoundException stayNotFoundException)
                {
                    bus.Notify(new NegativeResponseNotification(Omni, AuditMessage)
                    {
                        MessageObj = new
                        {
                            _error = nameof(StayNotFoundException),
                            stayNotFoundException.StayRequest
                        },
                        Originator = originator,
                        Immediate = true
                    });

                    continue;
                }

                errors.Add(new InvalidOperationException($"Booking failure during Day+{demoContextService.SimulationDay}", ex));
            }
        }

        if (errors.Count > 0)
        {
            throw new AggregateException($"Booking failures: {errors.Count}{Environment.NewLine}{Environment.NewLine}", errors);

        }
    }
}