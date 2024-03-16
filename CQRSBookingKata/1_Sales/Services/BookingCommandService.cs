
using CQRSBookingKata.Assets;
using CQRSBookingKata.Planning;
using CQRSBookingKata.Sales;

namespace CQRSBookingKata.Billing;

//accounts, purchasing

public class BookingCommandService(
    IBillingRepository billing, 
    ISalesRepository sales,
    IAssetsRepository assets,
    IPlanningRepository planning,
    PaymentCommandService payment)
{

    public int Book(StayProposition prop, int customerId, 
        string lastName, string firstName, long debitCardNumber, DebitCardSecrets secrets)
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

        var booking = new Booking(prop.ArrivalDate, prop.DepartureDate, lastName, firstName, prop.PersonCount, prop.Urid, customerId);

        billing.AddBookingAndInvoice(booking, invoice);
        
        var beginDayNum = OvernightStay.From(booking.ArrivalDate).DayNum;
        var endDayNum = OvernightStay.FromCheckOutDate(booking.DepartureDate).DayNum;

        var booked = Enumerable.Range(beginDayNum, endDayNum)

            .Select(dayNum => new Vacancy(dayNum, 0, 0, 0, booking.UniqueRoomId).VacancyId);

        sales.RemoveVacancies(booked);

        var room = new UniqueRoomId(booking.UniqueRoomId);

        planning.Add(new ReceptionCheck(
            booking.ArrivalDate, ReceptionEventType.CheckIn, 
            booking.LastName, booking.FirstName,
            room.RoomNum, false, room.HotelId,
            booking.BookingId, default, false, default, 0));

        planning.Add(new ReceptionCheck(
            booking.DepartureDate, ReceptionEventType.CheckOut, 
            booking.LastName, booking.FirstName,
            room.RoomNum, false, room.HotelId,
            booking.BookingId, default, false, default, 0));

        planning.Add(new RoomServiceDuty(booking.DepartureDate, default/****/,
            room.RoomNum, room.FloorNum, false, room.HotelId, 
            booking.BookingId, default, 0));

        return booking.BookingId;
    }

  
}