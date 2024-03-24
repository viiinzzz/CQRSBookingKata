﻿namespace CQRSBookingKata.Billing;

public class BookingCommandService
(
    ISalesRepository sales,

    IAdminRepository admin,
    IMoneyRepository money,
    IPlanningRepository planning,

    PaymentCommandService payment,

    IMessageBus bus
)
{
    public void OpenHotelSeason(int hotelId, int[]? exceptRoomNumbers, DateTime openingDate, DateTime closingDate, bool scoped)
    {
        var hotel = admin.GetHotel(hotelId);

        if (hotel == default)
        {
            throw new HotelNotFoundException();
        }

        if (hotel.Disabled)
        {
            throw new AccountLockedException();
        }

        try
        {
            using var scope = !scoped ? null : new TransactionScope();

            var nearestKnownCityName =
            hotel.Position == default
                ? default
                : SalesQueryService.Cities
                    .Select(city => new
                    {
                        cityName = city.name,
                        km = city.Position == default 
                            ? double.MaxValue 
                            : city?.Cells?[0].Id.EarthKm(hotel.Cells.First().S2CellId)
                    })
                    .MinBy(c => c.km)
                    ?.cityName;


            var roomNumbers = admin.Rooms(hotelId);

            if (exceptRoomNumbers != default)
            {
                roomNumbers = roomNumbers
                    .Where(room => !exceptRoomNumbers
                        .Contains(room.RoomNum));
            }

            var firstNight = OvernightStay.From(openingDate);
            var lastNight = OvernightStay.FromCheckOutDate(closingDate);

            var dayNumbers = Enumerable.Range(firstNight.DayNum, lastNight.DayNum);


            var vacancies = roomNumbers
                //
                //
                .ToArray() //query db
                //
                //
                .SelectMany(room => dayNumbers
                    .Select(dayNum => new Vacancy(dayNum, 
                        room.PersonMaxCount, hotel.Latitude, hotel.Longitude,
                        hotel.Cells12.Level12,
                        hotel.Cells12.Level11,
                        hotel.Cells12.Level10,
                        hotel.Cells12.Level9,
                        hotel.Cells12.Level8,
                        hotel.Cells12.Level7,
                        hotel.Cells12.Level6,
                        hotel.Cells12.Level5,
                        hotel.Cells12.Level4,
                        hotel.Cells12.Level3,
                        hotel.Cells12.Level2,
                        hotel.Cells12.Level1,
                        hotel.Cells12.Level0,
                        hotel.HotelName, nearestKnownCityName, false, room.Urid)));

            sales.AddVacancies(vacancies, scoped);

            scope?.Complete();
        }
        catch (Exception e)
        {
            throw new ServerErrorException(e);
        }
    }

    public int Book(StayProposition prop, int customerId, 
        string lastName, string firstName, long debitCardNumber, DebitCardSecrets secrets,
        bool scoped)
    {
        var customer = sales.GetCustomer(customerId);

        if (customer == default)
        {
            throw new CustomerNotFoundException();
        }

        if (customer.Disabled)
        {
            throw new AccountLockedException();
        }

        var invoice = new Invoice(debitCardNumber, prop.Price, prop.Currency, customerId);

        if (invoice.Amount != prop.Price || invoice.Currency != prop.Currency)
        {
            throw new InvalidAmountException();
        }

        if (!payment.Pay(prop.Price, prop.Currency, debitCardNumber, secrets.ownerName, secrets.expire, secrets.CCV))
        {
            throw new PaymentFailureException();
        }


        try
        {
            using var scope = !scoped ? null : new TransactionScope();

            var booking = new Booking(prop.ArrivalDate, prop.DepartureDate, lastName, firstName, prop.PersonCount, prop.Urid, customerId);

            money.AddInvoice(invoice, scoped: false);
            admin.AddBooking(booking, scoped: false);
            
            var beginDay = OvernightStay.From(booking.ArrivalDate);
            var endDay = OvernightStay.FromCheckOutDate(booking.DepartureDate);

            var booked = beginDay.StayUntil(endDay, booking.UniqueRoomId);

            sales.RemoveVacancies(booked, scoped: false);


            bus.Notify(new NotifyMessage {
                Recipient = default,
                Verb = "NEW BOOKING",
                Message = new
                {
                    booking.BookingId,
                    booking.ArrivalDate,
                    booking.DepartureDate,
                    booking.LastName,
                    booking.FirstName,
                    booking.UniqueRoomId
                },
            });

            var room = new UniqueRoomId(booking.UniqueRoomId);

            planning.Add(new ReceptionCheck(beginDay.DayNum,
                booking.ArrivalDate, ReceptionEventType.CheckIn, 
                booking.LastName, booking.FirstName,
                room.RoomNum, false, room.HotelId,
                booking.BookingId, default, false, default, 0));

            planning.Add(new ReceptionCheck(endDay.DayNum + 1,
                booking.DepartureDate, ReceptionEventType.CheckOut, 
                booking.LastName, booking.FirstName,
                room.RoomNum, false, room.HotelId,
                booking.BookingId, default, false, default, 0));

            planning.Add(new RoomServiceDuty(booking.DepartureDate, default/****/,
                room.RoomNum, room.FloorNum, false, room.HotelId, 
                booking.BookingId, default, 0));

            scope?.Complete();

            return booking.BookingId;
        }
        catch (Exception e)
        {
            throw new ServerErrorException(e);
        }
    }

  
}