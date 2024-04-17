namespace BookingKata.Sales;

public partial class BookingCommandService
{
    public void CancelBook
    (
        int bookingId,
        long correlationId1,
        long correlationId2
    )
    {
        var cancelDate = DateTime.UtcNow;
        var cancelDayNum = OvernightStay.From(cancelDate).DayNum;

        var booking = (
            from b in sales.Bookings
            where b.BookingId == bookingId &&
                  !b.Cancelled &&
                  b.ArrivalDayNum >= cancelDayNum
            select b
        ).FirstOrDefault();

        if (booking == default)
        {
            throw new CancellableBookingNotFoundException();
        }


        var customer = sales.GetCustomer(booking.CustomerId);

        if (customer == default)
        {
            throw new CustomerNotFoundException();
        }

        if (customer.Disabled)
        {
            throw new AccountLockedException();
        }


        var originator = GetType().FullName
                         ?? throw new Exception("invalid originator");

        var receiptId = bus.AskResult<Id>(
            originator, Common.Services.Billing.Recipient, Common.Services.Billing.Verb.RequestReceipt,
            new ReceiptRequest
            {
                referenceId = bookingId
            });

        if (receiptId == null)
        {
            throw new ArgumentException(ReferenceInvalid, nameof(bookingId));
        }


        var roomDetail = bus.AskResult<RoomDetails>(
            originator, Recipient.Admin, Verb.Admin.RequestSingleRoomDetails,
            new Id(booking.UniqueRoomId));

        if (roomDetail == null)
        {
            throw new RoomNotFoundException();
        }

        var refundId = bus.AskResult<Id>(
            originator, Common.Services.Billing.Recipient, Common.Services.Billing.Verb.RequestRefund,
            new RefundRequest
            {
                receiptId
            });

        if (refundId == null)
        {
            throw new ArgumentException(ReferenceInvalid, nameof(receiptId));
        }


        booking = sales.CancelBooking(bookingId);


        // var room = new UniqueRoomId(booking.UniqueRoomId);

        var firstNight = OvernightStay.From(booking.ArrivalDate);
        var lastNight = OvernightStay.FromCheckOutDate(booking.DepartureDate);


        var unbooked = 
            firstNight.DayNumssUntil(lastNight, booking.UniqueRoomId)

                .Select(dayNum => new Vacancy(dayNum,
                    roomDetail.PersonMaxCount, roomDetail.Latitude, roomDetail.Longitude,
                    roomDetail.HotelName, roomDetail.NearestKnownCityName, false, roomDetail.Urid)
                );


        sales.AddVacancies(unbooked);

        bus.Notify(originator, new Notification(Omni, BookCancelled)
        {
            Message = new Id(bookingId)
        });
    }
}