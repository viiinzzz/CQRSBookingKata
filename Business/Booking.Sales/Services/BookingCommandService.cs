using Booking.Sales.Services;

namespace BookingKata.Sales;

public class BookingCommandService
(
    ISalesRepository sales,

    AdminQueryService admin,
    // IMoneyRepository money,
    // IPlanningRepository planning,
    // IPaymentCommandService payment,

    IGazetteerService geo,
    ITimeService DateTime
)
{
    public void OpenHotelSeason(int hotelId, int[]? exceptRoomNumbers, DateTime openingDate, DateTime closingDate)
    {
        var roomDetails = admin.GetHotelDetails(hotelId, exceptRoomNumbers);


        var firstNight = OvernightStay.From(openingDate);
        var lastNight = OvernightStay.FromCheckOutDate(closingDate);

        var dayNumbers = firstNight.DayNum.RangeTo(lastNight.DayNum);


        var vacancies = roomDetails
           
            .SelectMany(roomDetail => dayNumbers
                .Select(dayNum => new Vacancy(dayNum,
                    roomDetails.PersonMaxCount, roomDetails.Latitude, roomDetails.Longitude,
                    roomDetails.HotelName, roomDetails.NearestKnownCityName, false, roomDetails.Urid)))
            //
            //
            .ToArray(); //query db
            //
            //

        geo.CopyToReferers(hotel, vacancies);

        sales.AddVacancies(vacancies);
    }

    public int Book
    (
        string lastName, string firstName, long debitCardNumber, DebitCardSecrets secrets,

        int customerId,
        int stayPropositionId,
        long correlationId1,
        long correlationId2
    )
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

        var invoice = new Invoice(prop.Price, prop.Currency, DateTime.UtcNow, customerId);

        if (invoice.Amount != prop.Price || invoice.Currency != prop.Currency)
        {
            throw new InvalidAmountException();
        }

        //
        //
        money.AddInvoice(invoice, scoped: false);
        //
        //


        try
    {
        using var scope = !scoped ? null : new TransactionScope();


        var paid = payment.Pay(prop.Price, prop.Currency, debitCardNumber, secrets.ownerName, secrets.expire, secrets.CCV);

        if (!paid)
        {
            throw new PaymentFailureException();
        }

        var receipt = new Receipt(debitCardNumber, prop.Price, prop.Currency, DateTime.UtcNow, customerId, invoice.InvoiceId);

        //
        //
        bus.Notify(new NotifyMessage(Recipient.Billing, Verb.Billing.QuotationEmitError)
        {
            CorrelationGuid = correlationId.Guid,
            Message = new { id = quotationId }
        });
        money.AddReceipt(receipt, scoped: false);
        //
        //

        var booking = new Booking(prop.ArrivalDate, prop.DepartureDate, lastName, firstName, prop.PersonCount, prop.Urid, customerId);

        
        //
        //
        sales.AddBooking(booking, scoped: false);
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
            booking.DepartureDate, System.DateTime.MaxValue,
            booking.DepartureDate.FractionalDayNum(), System.DateTime.MaxValue.FractionalDayNum(),
            room.RoomNum, room.FloorNum, false, room.HotelId, 
            booking.BookingId, default, 0));

        scope?.Complete();

        return booking.BookingId;
       
    }

  
}