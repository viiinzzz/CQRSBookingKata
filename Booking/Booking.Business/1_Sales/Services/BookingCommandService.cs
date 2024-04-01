namespace BookingKata.Sales;

public class BookingCommandService
(
    ISalesRepository sales,

    IAdminRepository admin,
    IMoneyRepository money,
    IPlanningRepository planning,

    IPaymentCommandService payment,
    IGazetteerService geo,

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

        var hotelCell = geo.RefererGeoIndex(hotel);

        if (hotelCell == null)
        {
            throw new HotelNotGeoIndexedException();
        }

        try
        {
            using var scope = !scoped ? null : new TransactionScope();

            //sales search will be performed against known cities list,
            //hence determining nearest known city name for geo-indexing

            var (nearestKnownCity, nearestKnownCityKm) = geo.NearestCity(hotelCell);


            var roomNumbers = admin.Rooms(hotelId);

            if (exceptRoomNumbers != default)
            {
                roomNumbers = roomNumbers
                    .Where(room => !exceptRoomNumbers
                        .Contains(room.RoomNum));
            }

            var firstNight = OvernightStay.From(openingDate);
            var lastNight = OvernightStay.FromCheckOutDate(closingDate);

            var dayNumbers = firstNight.DayNum.RangeTo(lastNight.DayNum);


            var vacancies = roomNumbers
                //
                //
                .ToArray() //query db
                //
                //
                .SelectMany(room => dayNumbers
                    .Select(dayNum => new Vacancy(dayNum, 
                        room.PersonMaxCount, hotel.Latitude, hotel.Longitude,
                        hotel.HotelName, nearestKnownCity.name, false, room.Urid)));

            geo.CopyToReferers(hotel, vacancies, scoped: false);

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



        try
        {
            using var scope = !scoped ? null : new TransactionScope();


            var paid = payment.Pay(prop.Price, prop.Currency, debitCardNumber, secrets.ownerName, secrets.expire, secrets.CCV);

            if (!paid)
            {
                throw new PaymentFailureException();
            }

            var booking = new Booking(prop.ArrivalDate, prop.DepartureDate, lastName, firstName, prop.PersonCount, prop.Urid, customerId);

            //
            //
            money.AddInvoice(invoice, scoped: false);
            //
            //
            
            //
            //
            admin.AddBooking(booking, scoped: false);
            //
            //

            var beginDay = OvernightStay.From(booking.ArrivalDate);
            var endDay = OvernightStay.FromCheckOutDate(booking.DepartureDate);

            var booked = beginDay.StayUntil(endDay, booking.UniqueRoomId);

            sales.RemoveVacancies(booked, scoped: false);


            bus.Notify(new NotifyMessage(Bus.Any, Verb.Sales.NewBooking)
            {
                Message = new NewBooking
                (
                    booking.BookingId,
                    booking.ArrivalDate,
                    booking.DepartureDate,
                    booking.LastName,
                    booking.FirstName,
                    booking.UniqueRoomId
                )
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


            var departureFracDayNum = OvernightStay.From(booking.DepartureDate).DayNum;

            planning.Add(new RoomServiceDuty(
                booking.DepartureDate, DateTime.MaxValue,
                booking.DepartureDate.FractionalDayNum(), DateTime.MaxValue.FractionalDayNum(),
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

public record NewBooking(
    int BookingId,
    DateTime ArrivalDate,
    DateTime DepartureDate,
    string LastName,
    string FirstName,
    int UniqueRoomId
);